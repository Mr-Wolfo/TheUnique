using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheUnique.Core.World;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int ChunkSize = 16;
    public Vector2Int WorldInChunks = new Vector2Int(2, 2);
    public int Seed = 12345;
    public int CurrentSeed;

    [Header("References")]
    public Tilemap FloorTilemap;
    public Tilemap DecorTilemap;
    public Transform DecorRoot;

    [Header("Assets")]
    public BiomeConfigSO BiomeConfig;
    public PatternLibrarySO GlobalLibrary;

    public bool IsDone = false;

    private IBiomeManager _biomeManager;
    private WorldRenderer _renderer;
    private ILogger _logger;
    private Dictionary<Vector2Int, int[,]> _chunkCache = new();

    [ContextMenu("Generate World")]
    public async void Generate()
    {
        IsDone = false;
        if (_logger == null) _logger = new WorldLogger();
        _biomeManager = new BiomeDistributor(BiomeConfig.Biomes.Count, Seed);
        _renderer = new WorldRenderer(FloorTilemap, DecorTilemap, DecorRoot, Seed);
        
        _renderer.Clear();
        _chunkCache.Clear();
        GlobalLibrary.BuildIndex();

        for (int cy = 0; cy < WorldInChunks.y; cy++)
        {
            for (int cx = 0; cx < WorldInChunks.x; cx++)
            {
                Vector2Int coord = new Vector2Int(cx, cy);
                await GenerateChunk(coord);
            }
        }
        FindAnyObjectByType<PlayerSpawner>().SpawnPlayerAfterGeneration();
        IsDone = true;
        _logger.Log("--- FULL WORLD READY ---");
    }

    private async Task<bool> GenerateChunk(Vector2Int coord)
    {
        int chunkSeed = Seed + (coord.y * 1000 + coord.x);
        var resolver = new WFCResolver(GlobalLibrary, _logger, chunkSeed, _biomeManager, BiomeConfig);
        
        resolver.InitGrid(ChunkSize, ChunkSize);

        if (!ApplyNeighborConstraints(resolver, coord))
        {
            _logger.Error($"Boundary conflict at chunk {coord}!");
        }

        var progress = new Progress<WFCCell[,]>(grid => RenderChunk(grid, coord));
        
        bool success = await resolver.GenerateAsync(ChunkSize, ChunkSize, progress);
        if (success)
        {
            CaptureChunkResult(resolver.GetGrid(), coord);
        }

        resolver.Dispose();
        return success;
    }

    private bool ApplyNeighborConstraints(WFCResolver resolver, Vector2Int coord)
    {
        bool allOk = true;

        if (_chunkCache.TryGetValue(new Vector2Int(coord.x - 1, coord.y), out var leftChunk))
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                int patternOnEdge = leftChunk[ChunkSize - 1, y];
                if (patternOnEdge == -1) continue;

                using var allowedMask = resolver.GetAdjacencyMask(patternOnEdge, Direction.Right);
                if (!resolver.ApplyConstraint(0, y, allowedMask)) allOk = false;
            }
        }

        if (_chunkCache.TryGetValue(new Vector2Int(coord.x, coord.y - 1), out var bottomChunk))
        {
            for (int x = 0; x < ChunkSize; x++)
            {
                int patternOnEdge = bottomChunk[x, ChunkSize - 1];
                if (patternOnEdge == -1) continue;

                using var allowedMask = resolver.GetAdjacencyMask(patternOnEdge, Direction.Up);
                if (!resolver.ApplyConstraint(x, 0, allowedMask)) allOk = false;
            }
        }

        return allOk;
    }

    private int GetCompatiblePattern(int sourceId, Direction dir)
    {
        var rules = GlobalLibrary.Adjacencies;
        foreach(var r in rules)
        {
            if (r.FromId == sourceId && r.Direction == dir) return r.ToId;
        }
        return sourceId;
    }

    private void CaptureChunkResult(WFCCell[,] grid, Vector2Int coord)
    {
        int[,] result = new int[ChunkSize, ChunkSize];
        for (int x = 0; x < ChunkSize; x++)
            for (int y = 0; y < ChunkSize; y++)
                result[x, y] = grid[x, y].GetCollapsed();
        _chunkCache[coord] = result;
    }

    private void RenderChunk(WFCCell[,] grid, Vector2Int coord)
    {
        Vector3Int offset = new Vector3Int(coord.x * ChunkSize, coord.y * ChunkSize, 0);
        _renderer.RenderWithOffset(grid, GlobalLibrary, offset);
    }
    
    public void GenerateWithSeed(int seed, int width, int height)
    {
        IsDone = false;
        CurrentSeed = seed;

        this.ChunkSize = width; 
        this.ChunkSize = height;

        Random.InitState(seed);
    
        Generate(); 
    }
}