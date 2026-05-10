using System.Collections.Generic;
using UnityEngine;

public class SpatialGridIndex
{
    private readonly Dictionary<Vector3Int, List<DecorItem>> _map = new();

    public void Add(Vector3Int cell, DecorItem item)
    {
        if (!_map.TryGetValue(cell, out var list))
        {
            list = new List<DecorItem>();
            _map[cell] = list;
        }
        list.Add(item);
    }

    public List<DecorItem> Get(Vector3Int cell)
    {
        return _map.TryGetValue(cell, out var list) ? list : null;
    }
}