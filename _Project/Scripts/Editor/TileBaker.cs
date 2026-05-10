using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TileBaker : EditorWindow
{
    public Tilemap source;
    private const string folderPath = "Assets/BakedTiles";

    [MenuItem("Tools/Tile Baker")]
    public static void ShowWindow() => GetWindow<TileBaker>("Tile Baker");

    private void OnGUI()
    {
        source = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", source, typeof(Tilemap), true);

        if (GUILayout.Button("Hard Bake Snapshot"))
        {
            if (source == null) { Debug.LogError("Выберите Tilemap!"); return; }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            Dictionary<Vector3Int, Sprite> snapshot = new Dictionary<Vector3Int, Sprite>();
            foreach (var pos in source.cellBounds.allPositionsWithin)
            {
                Sprite sprite = source.GetSprite(pos);
                if (sprite != null) snapshot[pos] = sprite;
            }

            source.ClearAllTiles();
            
            Dictionary<Sprite, Tile> bakedCache = new Dictionary<Sprite, Tile>();

            foreach (var kvp in snapshot)
            {
                Vector3Int pos = kvp.Key;
                Sprite sprite = kvp.Value;

                if (!bakedCache.TryGetValue(sprite, out Tile bakedTile))
                {
                    string assetPath = $"{folderPath}/{sprite.name}.asset";
                    bakedTile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);

                    if (bakedTile == null)
                    {
                        bakedTile = ScriptableObject.CreateInstance<Tile>();
                        bakedTile.sprite = sprite;
                        AssetDatabase.CreateAsset(bakedTile, assetPath);
                        Debug.Log($"Создан файл: {assetPath}");
                    }
                    bakedCache[sprite] = bakedTile;
                }

                source.SetTile(pos, bakedTile);
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(source);
            Debug.Log($"Запекание завершено! Обработано {snapshot.Count} тайлов.");
        }
    }
}