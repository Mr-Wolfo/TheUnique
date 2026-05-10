using UnityEngine;
using TheUnique.Core.Attributes;
using TheUnique.Core.Inventory;
using TheUnique.Core.Items;

namespace TheUnique.Core.Player
{
    [RequireComponent(typeof(AttributeSystem))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(InventoryController))]
    [RequireComponent(typeof(Crafting.CraftingManager))]
    public class PlayerEntity : MonoBehaviour
    {
        public AttributeSystem Stats { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public InventoryController Inventory { get; private set; }
        public Crafting.CraftingManager Crafting { get; private set; }

        private void Awake()
        {
            Stats = GetComponent<AttributeSystem>();
            Movement = GetComponent<PlayerMovement>();
            Inventory = GetComponent<InventoryController>();
            Crafting = GetComponent<Crafting.CraftingManager>();
        }
        
        public void UseItem(ItemSO item)
        {
            if (item == null) return;
            item.Use(this);
        }
    }
}