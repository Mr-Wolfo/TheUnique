using UnityEngine;
using UnityEngine.InputSystem;
using TheUnique.Core.Player;

namespace TheUnique.Core.Items
{
    [CreateAssetMenu(fileName = "New Placeable", menuName = "The Unique/Items/Placeable")]
    public class PlaceableItemSO : ItemSO
    {
        [Header("Настройки постройки")]
        [Tooltip("Префаб самой постройки")]
        public GameObject buildingPrefab;
        public float maxPlaceDistance = 3.0f;

        private void Awake()
        {
            category = ItemCategory.Equipment;
        }

        public override void Use(PlayerEntity player)
        {
            if (buildingPrefab == null) return;
            
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            
            float distance = Vector2.Distance(player.transform.position, mousePos);
            if (distance > maxPlaceDistance)
            {
                Debug.Log("Слишком далеко! Подойдите ближе.");
                return;
            }
            
            Instantiate(buildingPrefab, mousePos, Quaternion.identity);
            
            player.Inventory.RemoveItem(this, 1);
            Debug.Log($"Построен объект: {displayName}");
        }
    }
}