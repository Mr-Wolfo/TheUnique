using System.IO;
using UnityEngine;

namespace TheUnique.Core.SaveSystem
{
    public static class SaveManager
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "MySave.json");

        public static void SaveGame(GameSaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Игра сохранена сюда: {SavePath}");
        }

        public static GameSaveData LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("Файл сохранения не найден!");
                return null;
            }

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameSaveData>(json);
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }
    }
}