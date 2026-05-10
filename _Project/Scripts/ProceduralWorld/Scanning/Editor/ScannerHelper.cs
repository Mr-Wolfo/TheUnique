using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ScannerHelper 
{
    public static async void DoScan(List<GameObject> sourceObjects, PatternLibrarySO targetLibrary) 
    {
        if (sourceObjects == null || sourceObjects.Count == 0 || targetLibrary == null) 
        {
            Debug.LogError("[Scanner] Ошибка: Список объектов пуст или библиотека не выбрана.");
            return;
        }

        var validGrids = new List<Grid>();
        var floorMaps = new List<Tilemap>();
        var decorMaps = new List<Tilemap>();
        var roots = new List<Transform>();

        foreach (var obj in sourceObjects)
        {
            if (obj == null) continue;

            Grid[] gridsInObject = obj.GetComponentsInChildren<Grid>();

            foreach (var g in gridsInObject)
            {
                var floor = g.transform.Find("Tilemap_Floor")?.GetComponent<Tilemap>();
                var decor = g.transform.Find("Tilemap_Decor")?.GetComponent<Tilemap>();
                var root = g.transform.Find("DecorRoot");
                
                if (floor != null && decor != null) 
                {
                    validGrids.Add(g); 
                    floorMaps.Add(floor); 
                    decorMaps.Add(decor); 
                    roots.Add(root);
                }
            }
        }

        if (validGrids.Count == 0)
        {
            Debug.LogError("[Scanner] Ошибка: Ни в одном объекте не найдены слои 'Tilemap_Floor' и 'Tilemap_Decor'.");
            return;
        }

        Debug.Log($"[Scanner] Начинаем сканирование {validGrids.Count} гридов...");

        var scanner = new PatternScanner(new WorldLogger());
        var lib = await scanner.ScanAsync(targetLibrary.N, validGrids, floorMaps, decorMaps, roots);
        
        targetLibrary.UniqueCells = lib.UniqueCells;
        targetLibrary.Patterns = lib.Patterns;
        targetLibrary.WordsPerPattern = lib.WordsPerPattern;
        targetLibrary.FlatTableLength = lib.FlatAdjacencyTable.Length;
        targetLibrary.WordTableLength = lib.WordAdjacencyTable.Length;
        targetLibrary.ByteLutLength = lib.ByteLut.Length;

        string dataPath = targetLibrary.GetBinaryDataPath();
        
        using (FileStream fs = new FileStream(dataPath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            foreach (var val in lib.FlatAdjacencyTable) writer.Write(val);
            foreach (var val in lib.WordAdjacencyTable) writer.Write(val);
            foreach (var val in lib.ByteLut) writer.Write(val);
        }

        EditorUtility.SetDirty(targetLibrary);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); 
        
        Debug.Log($"--- Сканирование завершено! Объединено {validGrids.Count} источников. Данные: {dataPath} ---");
    }
}