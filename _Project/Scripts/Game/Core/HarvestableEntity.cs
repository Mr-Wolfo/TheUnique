using UnityEngine;
using TheUnique.Core.Interaction;
using TheUnique.Core.Items;
using TheUnique.Core.Player;
using TheUnique.Data;

namespace TheUnique.Core.World
{
    public class HarvestableEntity : MonoBehaviour, IInteractable
    {
        [Header("Настройки ресурса")]
        [SerializeField] private string _entityName = "Дерево";
        [SerializeField] private ResourceItemSO _resourceToDrop;
        [SerializeField] private int _minDrop = 2;
        [SerializeField] private int _maxDrop = 4;
        
        [Header("Требования")]
        [SerializeField] private ToolType _requiredTool;
        [SerializeField] private int _requiredPower = 1;
        
        [Header("Прочность")]
        [SerializeField] private float _maxDurability = 100f;
        private float _currentDurability;
        private bool _isDestroyed = false;
        
        private void Awake() => _currentDurability = _maxDurability;

        public string GetInteractionPrompt() => $"[E] {_entityName}";
        public float GetInteractionDistance() => 1.5f;

        public void Interact(PlayerEntity player)
        {
            ItemSO activeItem = player.Inventory.GetActiveItem();
            ToolItemSO tool = activeItem as ToolItemSO;

            if (tool != null && tool.toolType == _requiredTool)
            {
                if (tool.powerLevel >= _requiredPower)
                    OnHit(tool.powerLevel);
                else 
                    Debug.Log("Слишком слабый инструмент!");
            }
            else
            {
                Debug.Log($"Нужен инструмент: {_requiredTool}");
            }
        }

        private void OnHit(int power)
        {
            if (_isDestroyed) return;

            _currentDurability -= 20f * power;
            
            if (_currentDurability <= 0) 
            {
                _isDestroyed = true;
                OnDestroyed();
            }
        }

        private void OnDestroyed()
        {
            int count = Random.Range(_minDrop, _maxDrop + 1);
            for (int i = 0; i < count; i++)
            {
                SpawnDrop();
            }
            Destroy(gameObject);
        }

        private void SpawnDrop()
        {
            if (_resourceToDrop.worldItemPrefab == null) return;
            
            GameObject drop = Instantiate(_resourceToDrop.worldItemPrefab, transform.position, Quaternion.identity);
            WorldItem worldItem = drop.GetComponent<WorldItem>();
            if (worldItem != null) worldItem.Initialize(_resourceToDrop, 1);
        }
    }
}