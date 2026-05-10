using System.Collections.Generic;
using UnityEngine;
using TheUnique.Core.Inventory;

namespace TheUnique.UI
{
    public class UI_Hotbar : MonoBehaviour
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private int _hotbarSize = 8;

        private List<UI_InventorySlot> _uiSlots = new();
        private InventoryController _inventory;

        public void Initialize(InventoryController inventory, UI_InventoryDisplay mainDisplay)
        {
            _inventory = inventory;

            var allSlots = _inventory.GetAllSlots();
            
            foreach (Transform child in _container) Destroy(child.gameObject);
            _uiSlots.Clear();

            for (int i = 0; i < _hotbarSize; i++)
            {
                var slotObj = Instantiate(_slotPrefab, _container);
                var uiSlot = slotObj.GetComponent<UI_InventorySlot>();
                
                uiSlot.Bind(allSlots[i], i, mainDisplay);
                _uiSlots.Add(uiSlot);
            }

            _inventory.OnSlotSelected += UpdateSelection;
            
            UpdateSelection(_inventory.SelectedSlotIndex);
        }

        private void UpdateSelection(int index)
        {
            for (int i = 0; i < _uiSlots.Count; i++)
            {
                _uiSlots[i].SetHighlight(i == index);
            }
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnSlotSelected -= UpdateSelection;
            }
        }
    }
}