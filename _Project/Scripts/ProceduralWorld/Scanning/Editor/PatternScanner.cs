using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class PatternScanner : IPatternScanner
{
    private readonly ILogger _logger;
    private SpatialGridIndex _spatial;

    public PatternScanner(ILogger logger) => _logger = logger;

    public async Task<PatternLibrarySO> ScanAsync(int n, IEnumerable<Grid> grids, IEnumerable<Tilemap> floorMaps, IEnumerable<Tilemap> decorMaps, IEnumerable<Transform> decorRoots)
    {
        var uniqueCells = new List<CellContent>();
        var cellToId = new Dictionary<int, int>();
        var patterns = new Dictionary<int, PatternData>();
        var rules = new HashSet<(int, int, Direction)>();

        var fList = floorMaps.ToList();
        var dList = decorMaps.ToList();
        var rList = decorRoots.ToList();
        var gList = grids.ToList();

        for (int i = 0; i < fList.Count; i++)
        {
            BuildSpatialIndex(gList[i], rList[i]);
            var bounds = fList[i].cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var content = Extract(fList[i], dList[i], x, y);
                if (content.FloorSprite == null && content.DecorSprite == null) continue;

                int h = content.GetHash();
                if (!cellToId.ContainsKey(h))
                {
                    cellToId[h] = uniqueCells.Count;
                    uniqueCells.Add(content);
                }
            }
        }

        for (int i = 0; i < fList.Count; i++)
        {
            BuildSpatialIndex(gList[i], rList[i]);
            var bounds = fList[i].cellBounds;

            for (int x = bounds.xMin; x <= bounds.xMax - n; x++)
            for (int y = bounds.yMin; y <= bounds.yMax - n; y++)
            {
                int[] window = new int[n * n];
                bool emptyWindow = true;

                for (int ly = 0; ly < n; ly++)
                for (int lx = 0; lx < n; lx++)
                {
                    var content = Extract(fList[i], dList[i], x + lx, y + ly);
                    int h = content.GetHash();
                    if (cellToId.TryGetValue(h, out int cellId)) { window[ly * n + lx] = cellId; emptyWindow = false; }
                    else window[ly * n + lx] = -1;
                }

                if (emptyWindow) continue;

                int pHash = GetWindowHash(window);
                if (!patterns.TryGetValue(pHash, out var p))
                {
                    p = new PatternData { Id = patterns.Count, CellIndices = window, Weight = 1, Hash = pHash };
                    patterns.Add(pHash, p);
                }
                else p.Weight++;

                if (n == 1)
                {
                    CheckSimpleRule(p.Id, x, y, x, y + 1, Direction.Up, i, bounds, patterns, cellToId, fList, dList, rules);
                    CheckSimpleRule(p.Id, x, y, x, y - 1, Direction.Down, i, bounds, patterns, cellToId, fList, dList, rules);
                    CheckSimpleRule(p.Id, x, y, x - 1, y, Direction.Left, i, bounds, patterns, cellToId, fList, dList, rules);
                    CheckSimpleRule(p.Id, x, y, x + 1, y, Direction.Right, i, bounds, patterns, cellToId, fList, dList, rules);
                }
            }
        }

        var lib = ScriptableObject.CreateInstance<PatternLibrarySO>();
        lib.N = n;
        lib.UniqueCells = uniqueCells;
        lib.Patterns = patterns.Values.ToList();
        int patternCount = lib.Patterns.Count;

        if (n > 1) 
        {
            _logger.Log("Generating Overlap Rules...");
            lib.Adjacencies = BuildOverlapRules(lib.Patterns, n);
        }
        else 
        {
            lib.Adjacencies = rules.Select(r => new WFCAdjacencyRule { FromId = r.Item1, ToId = r.Item2, Direction = r.Item3 }).ToList();
        }

        int words = (patternCount + 63) / 64;
        lib.WordsPerPattern = words;
        lib.FlatAdjacencyTable = new ulong[4 * patternCount * words];
        lib.WordAdjacencyTable = new ulong[4 * words * words];
        lib.ByteLut = new ulong[4 * words * 8 * 256 * words];

        _logger.Log("Baking High-Speed Tables...");

        foreach (var rule in lib.Adjacencies)
        {
            int baseIdx = (((int)rule.Direction * patternCount) + rule.FromId) * words;
            lib.FlatAdjacencyTable[baseIdx + (rule.ToId >> 6)] |= (1UL << (rule.ToId & 63));
        }

        for (int d = 0; d < 4; d++)
        for (int w = 0; w < words; w++) {
            int baseW = (d * words + w) * words;
            for (int pInWord = 0; pInWord < 64; pInWord++) {
                int pId = (w << 6) + pInWord;
                if (pId >= patternCount) break;
                int adjBase = (d * patternCount + pId) * words;
                for (int uw = 0; uw < words; uw++) lib.WordAdjacencyTable[baseW + uw] |= lib.FlatAdjacencyTable[adjBase + uw];
            }
        }

        for (int d = 0; d < 4; d++)
        for (int w = 0; w < words; w++)
        for (int b = 0; b < 8; b++)
        for (int val = 0; val < 256; val++) {
            int lutBase = (((d * words + w) * 8 + b) * 256 + val) * words;
            for (int bit = 0; bit < 8; bit++) {
                if ((val & (1 << bit)) != 0) {
                    int pIdInWord = (b << 3) + bit;
                    int pId = (w << 6) + pIdInWord;
                    if (pId >= patternCount) break;
                    int adjBase = (d * patternCount + pId) * words;
                    for (int uw = 0; uw < words; uw++) lib.ByteLut[lutBase + uw] |= lib.FlatAdjacencyTable[adjBase + uw];
                }
            }
        }

        _logger.Log($"Scan Complete (N={n}). Patterns: {patternCount}, Rules: {lib.Adjacencies.Count}");
        return lib;
    }

    private void CheckSimpleRule(int fromId, int x, int y, int nx, int ny, Direction dir, int gridIdx, BoundsInt b, Dictionary<int, PatternData> patterns, Dictionary<int, int> cellToId, List<Tilemap> fList, List<Tilemap> dList, HashSet<(int, int, Direction)> rules)
    {
        if (nx < b.xMin || nx >= b.xMax || ny < b.yMin || ny >= b.yMax) return;
        var nContent = Extract(fList[gridIdx], dList[gridIdx], nx, ny);
        if (cellToId.TryGetValue(nContent.GetHash(), out int cId))
        {
            int[] nWindow = { cId };
            int nPHash = GetWindowHash(nWindow);
            if (patterns.TryGetValue(nPHash, out var nP)) rules.Add((fromId, nP.Id, dir));
        }
    }

    private List<WFCAdjacencyRule> BuildOverlapRules(List<PatternData> patterns, int n)
    {
        var result = new List<WFCAdjacencyRule>();
        foreach (var p1 in patterns)
        foreach (var p2 in patterns)
        {
            if (IsCompatible(p1, p2, Direction.Up, n)) result.Add(new WFCAdjacencyRule { FromId = p1.Id, ToId = p2.Id, Direction = Direction.Up });
            if (IsCompatible(p1, p2, Direction.Down, n)) result.Add(new WFCAdjacencyRule { FromId = p1.Id, ToId = p2.Id, Direction = Direction.Down });
            if (IsCompatible(p1, p2, Direction.Left, n)) result.Add(new WFCAdjacencyRule { FromId = p1.Id, ToId = p2.Id, Direction = Direction.Left });
            if (IsCompatible(p1, p2, Direction.Right, n)) result.Add(new WFCAdjacencyRule { FromId = p1.Id, ToId = p2.Id, Direction = Direction.Right });
        }
        return result;
    }

    private bool IsCompatible(PatternData a, PatternData b, Direction dir, int n)
    {
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            int ax = x, ay = y;
            int bx = x, by = y;
            switch (dir)
            {
                case Direction.Up:    by = y - 1; break;
                case Direction.Down:  by = y + 1; break;
                case Direction.Left:  bx = x + 1; break;
                case Direction.Right: bx = x - 1; break;
            }
            if (bx >= 0 && bx < n && by >= 0 && by < n)
            {
                if (a.CellIndices[ay * n + ax] != b.CellIndices[by * n + bx]) return false;
            }
        }
        return true;
    }

    private int GetWindowHash(int[] window)
    {
        unchecked { int hash = 19; foreach (int id in window) hash = hash * 31 + id; return hash; }
    }

    private CellContent Extract(Tilemap f, Tilemap d, int x, int y)
    {
        var pos = new Vector3Int(x, y, 0);
        var res = new CellContent { FloorSprite = f.GetSprite(pos), DecorSprite = d.GetSprite(pos) };
        if (_spatial != null) {
            var objs = _spatial.Get(pos);
            if (objs != null) res.Prefabs.AddRange(objs.Select(o => o.Prefab));
        }
        return res;
    }

    private void BuildSpatialIndex(Grid g, Transform r)
    {
        _spatial = new SpatialGridIndex();
        if (r == null) return;
        foreach (Transform child in r) _spatial.Add(g.WorldToCell(child.position), new DecorItem { Prefab = child.gameObject });
    }

    public Task<PatternLibrarySO> ScanAsync(IEnumerable<Grid> grids, IEnumerable<Tilemap> floorMaps, IEnumerable<Tilemap> decorMaps, IEnumerable<Transform> decorRoots) 
        => ScanAsync(1, grids, floorMaps, decorMaps, decorRoots);
}