using UnityEngine;
using TheUnique.Core.Items;
using TheUnique.Core.Player;
using TheUnique.Core.Interaction;

namespace TheUnique.Core.World
{
    public class GatherableEntity : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _entityName = "Камень";
        [SerializeField] private ResourceItemSO _resourceToDrop;
        [SerializeField] private int _amount = 1;
        
        private bool _isGathered = false;

        public string GetInteractionPrompt() => $"[ЛКМ] Собрать {_entityName}";
        public float GetInteractionDistance() => 3.0f;

        public void Interact(PlayerEntity player)
        {
            if (_isGathered) return;
            _isGathered = true;

            if (_resourceToDrop != null && _resourceToDrop.worldItemPrefab != null)
            {
                GameObject drop = Instantiate(_resourceToDrop.worldItemPrefab, transform.position, Quaternion.identity);
                WorldItem worldItem = drop.GetComponent<WorldItem>();
                if (worldItem != null) worldItem.Initialize(_resourceToDrop, _amount);
            }
            
            Destroy(gameObject);
        }
    }
}