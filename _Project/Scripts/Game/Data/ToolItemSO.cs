using TheUnique.Core.Player;
using TheUnique.Data;
using UnityEngine;

namespace TheUnique.Core.Items
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "The Unique/Items/Tool")]
    public class ToolItemSO : ItemSO
    {
        [Header("Характеристики инструмента")]
        public ToolType toolType;
        public int powerLevel = 1;
        public float attackRange = 1.5f; 
        public float attackSpeed = 1.0f; 
        
        [Header("Прочность")]
        public bool hasDurability = true;
        public int maxDurability = 100;

        private void Awake()
        {
            category = ItemCategory.Tool;
            isStackable = false;
        }

        public override void Use(PlayerEntity player)
        {
            Debug.Log($"Взмах инструментом: {displayName}");
        }
    }
}