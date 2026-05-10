using System;
using System.Collections.Generic;
using UnityEngine;
using TheUnique.Core.Inventory;
using TheUnique.Core.Items;
using TheUnique.Core.Player;

namespace TheUnique.Core.Crafting
{
    [RequireComponent(typeof(PlayerEntity))]
    public class CraftingManager : MonoBehaviour
    {
        [Header("База данных")]
        [SerializeField] private ItemDatabase _database;

        [Header("Настройки станций")]
        [Tooltip("На каком расстоянии верстак перестает работать")]
        [SerializeField] private float _stationDropDistance = 3.5f;

        private PlayerEntity _player;
        private string _activeStationTag = "None";
        private Transform _activeStationTransform;

        public event Action OnCraftingListChanged;

        private void Awake()
        {
            _player = GetComponent<PlayerEntity>();
        }

        private void Update()
        {
            if (_activeStationTag != "None" && _activeStationTransform != null)
            {
                float distance = Vector2.Distance(transform.position, _activeStationTransform.position);
                
                if (distance > _stationDropDistance)
                {
                    SetNearbyStation("None", null);
                    Debug.Log("Игрок отошел от станции крафта.");
                }
            }
        }

        public void SetNearbyStation(string stationTag, Transform stationTransform = null)
        {
            _activeStationTag = stationTag;
            _activeStationTransform = stationTransform;
            OnCraftingListChanged?.Invoke();
        }

        public List<RecipeSO> GetAvailableRecipes()
        {
            List<RecipeSO> available = new List<RecipeSO>();
            
            if (_database == null || _database.AllRecipes == null) return available;

            foreach (var recipe in _database.AllRecipes)
            {
                if (recipe.IsAvailable(_player.Inventory, _activeStationTag))
                {
                    available.Add(recipe);
                }
            }
            return available;
        }

        public bool TryCraft(RecipeSO recipe)
        {
            if (!recipe.IsAvailable(_player.Inventory, _activeStationTag))
            {
                Debug.Log("Недостаточно ресурсов для крафта!");
                return false;
            }

            if (!_player.Inventory.AddItem(recipe.resultItem, recipe.resultAmount))
            {
                Debug.Log("Нет места для результата!");
                return false;
            }

            foreach (var ing in recipe.ingredients)
            {
                _player.Inventory.RemoveItem(ing.item, ing.amount);
            }

            Debug.Log($"Создан: {recipe.resultItem.displayName}");
            OnCraftingListChanged?.Invoke();
            return true;
        }
    }
}