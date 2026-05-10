using UnityEngine;
using TheUnique.Core.Player;

namespace TheUnique.Core.Items
{
    public enum ItemCategory { Resource, Tool, Consumable, Equipment, Material }

    public abstract class ItemSO : ScriptableObject
    {
        [Header("Визуальные данные")]
        public string id; 
        public string displayName;
        [TextArea(3, 10)] public string description;
        public Sprite icon;
        public ItemCategory category;

        [Header("Мир")]
        public GameObject worldItemPrefab;

        [Header("Параметры стека")]
        public bool isStackable = true;
        public int maxStack = 99;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
                id = System.Guid.NewGuid().ToString();
        }

        public abstract void Use(PlayerEntity player);
    }
}