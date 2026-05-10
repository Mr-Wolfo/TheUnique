using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PatternLibrarySO))]
public class PatternLibraryEditor : Editor
{
    [SerializeField] private List<GameObject> _sourceObjects = new List<GameObject>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PatternLibrarySO library = (PatternLibrarySO)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Источники для обучения (Grids)", EditorStyles.boldLabel);

        for (int i = 0; i < _sourceObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _sourceObjects[i] = (GameObject)EditorGUILayout.ObjectField($"Источник {i+1}", _sourceObjects[i], typeof(GameObject), true);
            
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                _sourceObjects.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Добавить новый источник (+)"))
        {
            _sourceObjects.Add(null);
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Сканировать", GUILayout.Height(40)))
        {
            if (_sourceObjects.Count == 0 || _sourceObjects[0] == null)
            {
                EditorUtility.DisplayDialog("Ошибка", "Добавьте хотя бы один объект со сцены в список источников.", "ОК");
                return;
            }

            ScannerHelper.DoScan(_sourceObjects, library);
        }
    }
}