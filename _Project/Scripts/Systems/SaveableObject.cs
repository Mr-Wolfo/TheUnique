using UnityEngine;

namespace TheUnique.Core.SaveSystem
{
    public class SaveableObject : MonoBehaviour
    {
        [Tooltip("Имя префаба для восстановления")]
        public string PrefabName;
    }
}