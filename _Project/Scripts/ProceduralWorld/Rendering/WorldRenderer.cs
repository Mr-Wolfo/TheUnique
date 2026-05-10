using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldRenderer
{
    private readonly Tilemap _floor;
    private readonly Tilemap _decor;
    private readonly Transform _root;
    private readonly int _seed;

    private const float OffsetRange = 0.25f;

    private Dictionary<Vector3Int, List<GameObject>> _spawnedObjects = new Dictionary<Vector3Int, List<GameObject>>();
    private Dictionary<Sprite, Tile> _tileCache = new Dictionary<Sprite, Tile>();

    public WorldRenderer(Tilemap floor, Tilemap decor, Transform root, int seed)
    {
        _floor = floor;
        _decor = decor; 
        _root = root; 
        _seed = seed;
    }

    public void RenderWithOffset(WFCCell[,] grid, PatternLibrarySO library, Vector3Int offset)
    {
        if (grid == null) return;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        List<Vector3Int> floorPositions = new List<Vector3Int>();
        List<TileBase> floorTiles = new List<TileBase>();

        List<Vector3Int> decorPositions = new List<Vector3Int>();
        List<TileBase> decorTiles = new List<TileBase>();

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Vector3Int pos = new Vector3Int(x + offset.x, y + offset.y, 0);
            int pId = grid[x, y].GetCollapsed();

            if (pId < 0 || pId >= library.Patterns.Count) 
            {
                ClearPrefabsAt(pos);
                continue;
            }

            var pattern = library.Get(pId);
            if (pattern == null) continue;

            var cell = library.GetCell(pattern.CellIndices[0]);
            if (cell == null) continue;
            
            if (cell.FloorSprite != null)
            {
                floorPositions.Add(pos);
                floorTiles.Add(GetOrCreateTile(cell.FloorSprite));
            }
            
            if (cell.DecorSprite != null)
            {
                decorPositions.Add(pos);
                decorTiles.Add(GetOrCreateTile(cell.DecorSprite));
            }

            if (cell.Prefabs != null && cell.Prefabs.Count > 0)
            {
                ClearPrefabsAt(pos);

                List<GameObject> cellInstances = new List<GameObject>();
                HashSet<string> uniqueNames = new HashSet<string>();

                Random.State oldState = Random.state;
                Random.InitState(_seed + x * 1000 + y);
                
                foreach (var prefab in cell.Prefabs)
                {
                    if (prefab == null || uniqueNames.Contains(prefab.name)) continue;
                    uniqueNames.Add(prefab.name);

                    float ox = Random.Range(-OffsetRange, OffsetRange);
                    float oy = Random.Range(-OffsetRange, OffsetRange);
                    Vector3 worldPos = _floor.CellToWorld(pos) + new Vector3(0.5f + ox, 0.5f + oy, 0);
                    
                    GameObject obj = Object.Instantiate(prefab, worldPos, Quaternion.identity, _root);
                    cellInstances.Add(obj);
                }
                Random.state = oldState;

                _spawnedObjects[pos] = cellInstances;
            }
            else
            {
                ClearPrefabsAt(pos);
            }
        }

        _floor.SetTiles(floorPositions.ToArray(), floorTiles.ToArray());
        _decor.SetTiles(decorPositions.ToArray(), decorTiles.ToArray());
    }

    private void ClearPrefabsAt(Vector3Int pos)
    {
        if (_spawnedObjects.TryGetValue(pos, out List<GameObject> oldObjects))
        {
            foreach (var obj in oldObjects)
            {
                if (obj != null) 
                {
                    if (Application.isPlaying) Object.Destroy(obj);
                    else Object.DestroyImmediate(obj);
                }
            }
            _spawnedObjects.Remove(pos);
        }
    }

    private TileBase GetOrCreateTile(Sprite sprite)
    {
        if (sprite == null) return null;

        if (!_tileCache.TryGetValue(sprite, out Tile tile))
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.name = sprite.name;
            
            _tileCache[sprite] = tile;
        }

        return tile;
    }

    public void Clear()
    {
        _floor.ClearAllTiles();
        _decor.ClearAllTiles();
        
        _spawnedObjects.Clear();
        _tileCache.Clear();

        if (_root != null)
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Object.Destroy(_root.GetChild(i).gameObject);
                else Object.DestroyImmediate(_root.GetChild(i).gameObject);
            }
        }
    }
}