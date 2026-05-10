using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "WFC/Pattern Library")]
public class PatternLibrarySO : ScriptableObject
{
    public int N = 2;
    public List<CellContent> UniqueCells = new();
    public List<PatternData> Patterns = new();
    public List<WFCAdjacencyRule> Adjacencies = new();

    [Header("Binary Data")]
    [Tooltip("Перетащи сюда файл _Data.bytes, который создал сканер")]
    public TextAsset BinaryDataFile; // ТЕПЕРЬ ДАННЫЕ ЖИВУТ ТУТ

    [Header("Precomputed Metadata")]
    public int WordsPerPattern;
    public int FlatTableLength;
    public int WordTableLength;
    public int ByteLutLength;

    [HideInInspector] public ulong[] FlatAdjacencyTable;
    [HideInInspector] public ulong[] WordAdjacencyTable;
    [HideInInspector] public ulong[] ByteLut;

    private Dictionary<int, PatternData> _byId;

    public void BuildIndex() 
    {
        _byId = new Dictionary<int, PatternData>();
        foreach (var p in Patterns) _byId[p.Id] = p;
        LoadBinaryDataFromAsset();
    }

    private void LoadBinaryDataFromAsset()
    {
        if (BinaryDataFile == null) 
        {
            Debug.LogError("[PatternLibrary] ОШИБКА: Файл BinaryDataFile не прикреплен!");
            return;
        }
        
        using (MemoryStream ms = new MemoryStream(BinaryDataFile.bytes))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            FlatAdjacencyTable = new ulong[FlatTableLength];
            for (int i = 0; i < FlatTableLength; i++) FlatAdjacencyTable[i] = reader.ReadUInt64();

            WordAdjacencyTable = new ulong[WordTableLength];
            for (int i = 0; i < WordTableLength; i++) WordAdjacencyTable[i] = reader.ReadUInt64();

            ByteLut = new ulong[ByteLutLength];
            for (int i = 0; i < ByteLutLength; i++) ByteLut[i] = reader.ReadUInt64();
        }
        Debug.Log($"[PatternLibrary] Загружено: {FlatAdjacencyTable.Length} правил из свежего файла.");
    }

    public PatternData Get(int id) => _byId != null && _byId.TryGetValue(id, out var p) ? p : null;
    public CellContent GetCell(int index) => (index >= 0 && index < UniqueCells.Count) ? UniqueCells[index] : null;

    public string GetBinaryDataPath() 
    {
#if UNITY_EDITOR
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(assetPath)) return "";
        return Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace(".asset", "_Data.bytes"));
#else
        return "";
#endif
    }
}

[System.Serializable]
public struct WFCAdjacencyRule
{
    public int FromId;
    public int ToId;
    public Direction Direction;
}