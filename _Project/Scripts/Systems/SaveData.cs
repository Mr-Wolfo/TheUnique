using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheUnique.Core.SaveSystem
{
    // Главный файл сохранения
    [Serializable]
    public class GameSaveData
    {
        public int WorldSeed;
        public Vector3 PlayerPosition;
        public float TimeOfDay;
        public List<SavedObject> WorldObjects = new List<SavedObject>();
    }

    // Сохраненный префаб (срубленные деревья сюда не попадут, поэтому они исчезнут навсегда)
    [Serializable]
    public class SavedObject
    {
        public string PrefabName; // Например "Tree_1" или "Stone"
        public Vector3 Position;
    }
}