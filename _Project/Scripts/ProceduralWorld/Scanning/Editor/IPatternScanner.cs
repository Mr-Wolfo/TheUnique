using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IPatternScanner
{
    Task<PatternLibrarySO> ScanAsync(IEnumerable<Grid> grids, IEnumerable<Tilemap> floorMaps, IEnumerable<Tilemap> decorMaps, IEnumerable<Transform> decorRoots);
}