using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Diagnostics;

public class WFCResolver : IWFCResolver, IDisposable
{
    private readonly PatternLibrarySO _library;
    private readonly ILogger _logger;
    private readonly IBiomeManager _biomeManager;
    private readonly BiomeConfigSO _biomeConfig;

    private System.Random _random;
    private int _width, _height, _numPatterns, _wordsPerPattern, _nibblesPerPattern;

    private NativeArray<ulong> _gridPossible;
    private NativeArray<int> _gridCount;
    private NativeArray<ulong> _flatAdjacency;
    private NativeArray<ulong> _wordAdjacency;
    private NativeArray<ulong> _nibbleLut;
    private NativeArray<ulong> _byteLut;
    private NativeArray<float> _patternWeights;
    
    private NativeList<int> _changedIndices;
    private NativeList<ulong> _oldValues;

    private List<int> _uncollapsed = new();
    private bool[] _isUncollapsed; 
    private Stack<StateSnapshot> _history = new Stack<StateSnapshot>();
    private WFCCell[,] _renderGrid;

    private Stopwatch _swPropagate = new Stopwatch();
    private Stopwatch _swEntropy = new Stopwatch();
    private Stopwatch _swRandom = new Stopwatch();
    private Stopwatch _swSnapshot = new Stopwatch();

    public WFCResolver(PatternLibrarySO library, ILogger logger, int seed, IBiomeManager biomeManager, BiomeConfigSO config)
    {
        _library = library; 
        _logger = logger; 
        _random = new System.Random(seed);
        _biomeManager = biomeManager; 
        _biomeConfig = config;
    
        _numPatterns = _library.Patterns.Count;
        _wordsPerPattern = _library.WordsPerPattern;

        _library.BuildIndex();

        if (_library.FlatAdjacencyTable == null || _library.FlatAdjacencyTable.Length == 0)
        {
            _logger.Error("[WORLD] Бинарные данные не загружены! Убедись, что файл _Data.bytes вставлен в поле Binary Data File в PatternLibrary.");
            return;
        }

        _flatAdjacency = new NativeArray<ulong>(_library.FlatAdjacencyTable.Length, Allocator.Persistent);
        _wordAdjacency = new NativeArray<ulong>(_library.WordAdjacencyTable.Length, Allocator.Persistent);
        _byteLut = new NativeArray<ulong>(_library.ByteLut.Length, Allocator.Persistent);
    
        _flatAdjacency.CopyFrom(_library.FlatAdjacencyTable);
        _wordAdjacency.CopyFrom(_library.WordAdjacencyTable);
        _byteLut.CopyFrom(_library.ByteLut);
        
        _logger.Log($"Initializing WFC: Patterns={_numPatterns}, Words={_wordsPerPattern}, Nibbles={_nibblesPerPattern}");

        _patternWeights = new NativeArray<float>(_numPatterns, Allocator.Persistent);
        for (int i = 0; i < _numPatterns; i++) _patternWeights[i] = _library.Patterns[i].Weight;

        _changedIndices = new NativeList<int>(Allocator.Persistent);
        _oldValues = new NativeList<ulong>(Allocator.Persistent);
    }

    private void BuildNibbleLut()
    {
        _nibbleLut = new NativeArray<ulong>(4 * _nibblesPerPattern * 16 * _wordsPerPattern, Allocator.Persistent);
        _logger.Log("Precomputing Nibble LUT...");

        for (int d = 0; d < 4; d++)
        for (int nIdx = 0; nIdx < _nibblesPerPattern; nIdx++)
        for (int val = 0; val < 16; val++)
        {
            int lutBase = (((d * _nibblesPerPattern + nIdx) << 4) + val) * _wordsPerPattern;
            for (int bit = 0; bit < 4; bit++)
            {
                if ((val & (1 << bit)) != 0)
                {
                    int pId = (nIdx << 2) + bit; 
                    if (pId >= _numPatterns) break;
                    
                    int adjBase = (d * _numPatterns + pId) * _wordsPerPattern;
                    for (int w = 0; w < _wordsPerPattern; w++)
                        _nibbleLut[lutBase + w] |= _flatAdjacency[adjBase + w];
                }
            }
        }
    }

    public async Task<bool> GenerateAsync(int width, int height, IProgress<WFCCell[,]> progress = null)
    {
        _history.Clear();

        _swPropagate.Reset(); _swEntropy.Reset(); _swRandom.Reset(); _swSnapshot.Reset();
        var totalWatch = Stopwatch.StartNew();

        return await Task.Run(() =>
        {
            if (_uncollapsed.Count == (_width * _height)) 
            {
                Vector2Int center = new Vector2Int(_width / 2, _height / 2);
                if (!CollapseAt(center)) return false;
            }

            int step = 0;
            while (_uncollapsed.Count > 0)
            {
                _swEntropy.Start();
                int nextIdx = GetLowestEntropyIndex();
                _swEntropy.Stop();

                if (nextIdx == -1) break;

                if (!CollapseAt(new Vector2Int(nextIdx % _width, nextIdx / _width)))
                {
                    if (!ApplyBacktrack()) return false;
                    continue;
                }

                step++;
                if (progress != null && step % 500 == 0) progress.Report(SyncRenderGrid());
            }

            totalWatch.Stop();
            
            string report = $"--- WFC PERFORMANCE REPORT ---\n" +
                            $"Total Time: {totalWatch.ElapsedMilliseconds} ms\n" +
                            $"Propagate (Burst+LUT): {_swPropagate.ElapsedMilliseconds} ms\n" +
                            $"Entropy Search: {_swEntropy.ElapsedMilliseconds} ms\n" +
                            $"Weighted Random: {_swRandom.ElapsedMilliseconds} ms\n" +
                            $"Snapshot/History: {_swSnapshot.ElapsedMilliseconds} ms\n" +
                            $"------------------------------";
            _logger.Log(report);

            progress?.Report(SyncRenderGrid());
            return true;
        });
    }

    public bool ForceCollapse(int x, int y, int patternId)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return false;
        int cellIdx = y * _width + x;
    
        int offset = cellIdx * _wordsPerPattern;
        for (int w = 0; w < _wordsPerPattern; w++) _gridPossible[offset + w] = 0;
    
        _gridPossible[offset + (patternId >> 6)] |= (1UL << (patternId & 63));
        _gridCount[cellIdx] = 1;
        _isUncollapsed[cellIdx] = false;
        _uncollapsed.Remove(cellIdx);

        _changedIndices.Clear(); _oldValues.Clear();
        var job = new PropagateJob {
            StartIdx = cellIdx, Width = _width, Height = _height, 
            NumPatterns = _numPatterns, WordsPerPattern = _wordsPerPattern,
            GridPossible = _gridPossible, GridCount = _gridCount, 
            WordAdjacency = _wordAdjacency,
            ByteLut = _byteLut,
            OutChangedIndices = _changedIndices, OutOldValues = _oldValues
        };
        job.Run();

        foreach (int idx in _changedIndices) if (_gridCount[idx] == 0) return false;
    
        return true;
    }
    
    private bool CollapseAt(Vector2Int pos)
    {
        int cellIdx = pos.y * _width + pos.x;
        
        _swRandom.Start();
        int chosenId = GetWeightedRandom(cellIdx);
        _swRandom.Stop();

        if (chosenId == -1) return false;

        _swSnapshot.Start();
        var snap = new StateSnapshot(pos.x, pos.y, chosenId, _wordsPerPattern);
        snap.Store(cellIdx, _gridPossible, _wordsPerPattern);
        _swSnapshot.Stop();

        int offset = cellIdx * _wordsPerPattern;
        for (int w = 0; w < _wordsPerPattern; w++) _gridPossible[offset + w] = 0;
        _gridPossible[offset + (chosenId >> 6)] |= (1UL << (chosenId & 63));
        _gridCount[cellIdx] = 1;
        
        _isUncollapsed[cellIdx] = false;
        _uncollapsed.Remove(cellIdx); 

        _changedIndices.Clear();
        _oldValues.Clear();

        var job = new PropagateJob {
            StartIdx = cellIdx, Width = _width, Height = _height, 
            NumPatterns = _numPatterns, WordsPerPattern = _wordsPerPattern,
            GridPossible = _gridPossible, GridCount = _gridCount, 
            WordAdjacency = _wordAdjacency,
            ByteLut = _byteLut,
            OutChangedIndices = _changedIndices, OutOldValues = _oldValues
        };

        _swPropagate.Start();
        job.Run();
        _swPropagate.Stop();

        foreach (int idx in _changedIndices) {
            if (_gridCount[idx] == 0) {
                _swSnapshot.Start();
                snap.RestoreNative(_gridPossible, _gridCount);
                _swSnapshot.Stop();
                if (!_isUncollapsed[cellIdx]) { _uncollapsed.Add(cellIdx); _isUncollapsed[cellIdx] = true; }
                return false;
            }
        }

        _swSnapshot.Start();
        for (int i = 0; i < _changedIndices.Length; i++)
            snap.StoreFromBurst(_changedIndices[i], _oldValues, i * _wordsPerPattern, _wordsPerPattern);
        _history.Push(snap);
        _swSnapshot.Stop();

        return true;
    }

    private int GetWeightedRandom(int cellIdx)
    {
        double total = 0;
        int offset = cellIdx * _wordsPerPattern;

        // Pass 1: Sum
        for (int w = 0; w < _wordsPerPattern; w++) {
            ulong bits = _gridPossible[offset + w];
            while (bits != 0) {
                int pId = (w << 6) + math.tzcnt(bits);
                if (pId < _numPatterns) total += _patternWeights[pId];
                bits &= bits - 1;
            }
        }

        if (total <= 0) return -1;
        double r = _random.NextDouble() * total;
        double cur = 0;

        for (int w = 0; w < _wordsPerPattern; w++) {
            ulong bits = _gridPossible[offset + w];
            while (bits != 0) {
                int bitIdx = math.tzcnt(bits);
                int pId = (w << 6) + bitIdx;
                if (pId < _numPatterns) {
                    cur += _patternWeights[pId];
                    if (r <= cur) return pId;
                }
                bits &= bits - 1;
            }
        }
        return -1;
    }

    private int GetLowestEntropyIndex()
    {
        int bestIdx = -1;
        float minEntropy = float.MaxValue;

        for (int i = _uncollapsed.Count - 1; i >= 0; i--)
        {
            int idx = _uncollapsed[i];
            int count = _gridCount[idx];
            
            if (count <= 1) { 
                _isUncollapsed[idx] = false;
                _uncollapsed.RemoveAt(i); 
                continue; 
            }

            float entropy = count + (float)_random.NextDouble() * 0.2f;
            if (entropy < minEntropy) { minEntropy = entropy; bestIdx = idx; }
        }
        return bestIdx;
    }
    
    public bool ApplyConstraint(int x, int y, NativeArray<ulong> constraintMask)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return true;

        int cellIdx = y * _width + x;
        int offset = cellIdx * _wordsPerPattern;
        bool changed = false;

        var snap = new StateSnapshot(x, y, -1, _wordsPerPattern);
        snap.Store(cellIdx, _gridPossible, _wordsPerPattern);

        for (int w = 0; w < _wordsPerPattern; w++)
        {
            ulong oldBits = _gridPossible[offset + w];
            ulong newBits = oldBits & constraintMask[w];
            if (oldBits != newBits)
            {
                _gridPossible[offset + w] = newBits;
                changed = true;
            }
        }

        if (changed)
        {
            int offset2 = cellIdx * _wordsPerPattern;
            int newCount = 0;
            for (int w = 0; w < _wordsPerPattern; w++) 
                newCount += math.countbits(_gridPossible[offset2 + w]);
            _gridCount[cellIdx] = newCount;
            if (_gridCount[cellIdx] == 0) return false;
            
            _changedIndices.Clear(); _oldValues.Clear();
            var job = new PropagateJob {
                StartIdx = cellIdx, Width = _width, Height = _height, 
                NumPatterns = _numPatterns, WordsPerPattern = _wordsPerPattern,
                GridPossible = _gridPossible, GridCount = _gridCount, 
                WordAdjacency = _wordAdjacency,
                ByteLut = _byteLut,
                OutChangedIndices = _changedIndices, OutOldValues = _oldValues
            };
            job.Run();

            foreach (int idx in _changedIndices) if (_gridCount[idx] == 0) return false;
            
            for (int i = 0; i < _changedIndices.Length; i++)
                snap.StoreFromBurst(_changedIndices[i], _oldValues, i * _wordsPerPattern, _wordsPerPattern);
            
            _history.Push(snap);
        }

        return true;
    }

    public NativeArray<ulong> GetAdjacencyMask(int patternId, Direction dir)
    {
        int offset = (((int)dir * _numPatterns) + patternId) * _wordsPerPattern;
        var mask = new NativeArray<ulong>(_wordsPerPattern, Allocator.Temp);
        for (int w = 0; w < _wordsPerPattern; w++) mask[w] = _flatAdjacency[offset + w];
        return mask;
    }

    private bool ApplyBacktrack()
    {
        _swSnapshot.Start();
        if (_history.Count == 0) { _swSnapshot.Stop(); return false; }
        var snap = _history.Pop();
        snap.RestoreNative(_gridPossible, _gridCount);
        _swSnapshot.Stop();

        int idx = snap.Y * _width + snap.X;
        if (!_isUncollapsed[idx]) { _uncollapsed.Add(idx); _isUncollapsed[idx] = true; }

        int offset = idx * _wordsPerPattern;
        _gridPossible[offset + (snap.ChosenId >> 6)] &= ~(1UL << (snap.ChosenId & 63));
        
        int newCount = 0;
        for (int w = 0; w < _wordsPerPattern; w++) 
            newCount += math.countbits(_gridPossible[offset + w]);
        _gridCount[idx] = newCount;

        if (newCount == 0) return ApplyBacktrack();

        _changedIndices.Clear(); _oldValues.Clear();
        var job = new PropagateJob {
            StartIdx = idx, Width = _width, Height = _height, 
            NumPatterns = _numPatterns, WordsPerPattern = _wordsPerPattern,
            GridPossible = _gridPossible, GridCount = _gridCount, 
            WordAdjacency = _wordAdjacency,
            ByteLut = _byteLut,
            OutChangedIndices = _changedIndices, OutOldValues = _oldValues
        };
        
        _swPropagate.Start();
        job.Run();
        _swPropagate.Stop();

        foreach (int cIdx in _changedIndices) if (_gridCount[cIdx] == 0) return ApplyBacktrack();
        
        _history.Push(snap);
        return true;
    }

    public void InitGrid(int width, int height)
    {
        _width = width; _height = height;
        
        if (_gridPossible.IsCreated) _gridPossible.Dispose();
        if (_gridCount.IsCreated) _gridCount.Dispose();

        int total = _width * _height;
        _gridPossible = new NativeArray<ulong>(total * _wordsPerPattern, Allocator.Persistent);
        _gridCount = new NativeArray<int>(total, Allocator.Persistent);
        _isUncollapsed = new bool[total];
        _uncollapsed.Clear();

        for (int i = 0; i < total; i++) {
            _uncollapsed.Add(i);
            _isUncollapsed[i] = true;
            _gridCount[i] = _numPatterns;
            int offset = i * _wordsPerPattern;
            for (int w = 0; w < _wordsPerPattern; w++) _gridPossible[offset + w] = ulong.MaxValue;
            if (_numPatterns % 64 != 0)
                _gridPossible[offset + _wordsPerPattern - 1] &= (1UL << (_numPatterns & 63)) - 1;
        }

        _renderGrid = new WFCCell[_width, _height];
        for (int x = 0; x < _width; x++) for (int y = 0; y < _height; y++) 
            _renderGrid[x, y] = new WFCCell(_numPatterns);
    }

    private WFCCell[,] SyncRenderGrid()
    {
        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
        {
            int idx = y * _width + x;
            int offset = idx * _wordsPerPattern;
            for (int w = 0; w < _wordsPerPattern; w++)
                _renderGrid[x, y].Possible.Bits[w] = _gridPossible[offset + w];
        }
        return _renderGrid;
    }

    public void Dispose()
    {
        if (_gridPossible.IsCreated) _gridPossible.Dispose();
        if (_gridCount.IsCreated) _gridCount.Dispose();
        if (_flatAdjacency.IsCreated) _flatAdjacency.Dispose();
        if (_wordAdjacency.IsCreated) _wordAdjacency.Dispose();
        if (_byteLut.IsCreated) _byteLut.Dispose();
        if (_nibbleLut.IsCreated) _nibbleLut.Dispose();
        if (_patternWeights.IsCreated) _patternWeights.Dispose();
        if (_changedIndices.IsCreated) _changedIndices.Dispose();
        if (_oldValues.IsCreated) _oldValues.Dispose();
    }

    public WFCCell[,] GetGrid() => SyncRenderGrid();
}