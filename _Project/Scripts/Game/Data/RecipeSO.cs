using System.Collections.Generic;
using TheUnique.Core.Inventory;
using UnityEngine;

namespace TheUnique.Core.Items
{
    [System.Serializable]
    public struct Ingredient
    {
        public ItemSO item;
        public int amount;
    }

    [CreateAssetMenu(fileName = "New Recipe", menuName = "The Unique/Crafting/Recipe")]
    public class RecipeSO : ScriptableObject
    {
        [Header("Результат")]
        public ItemSO resultItem;
        public int resultAmount = 1;

        [Header("Ингредиенты")]
        public List<Ingredient> ingredients;

        [Header("Требования")]
        public string requiredStationTag = "None"; 
        public float craftTime = 0.5f;

        public bool IsAvailable(InventoryController inventory, string currentStationTag)
        {
            if (requiredStationTag != "None" && requiredStationTag != currentStationTag)
                return false;

            foreach (var ing in ingredients)
            {
                int count = 0;
                foreach (var slot in inventory.GetAllSlots())
                {
                    if (!slot.IsEmpty && slot.Item == ing.item)
                        count += slot.Amount;
                }

                if (count < ing.amount) return false;
            }

            return true;
        }
    }
}