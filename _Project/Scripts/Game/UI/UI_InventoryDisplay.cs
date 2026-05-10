using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TheUnique.Core.Inventory;
using UnityEngine.EventSystems;

namespace TheUnique.UI
{
    public class UI_InventoryDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _gridParent;
        
        [Header("Drag Visuals")]
        [SerializeField] private Image _dragIcon;

        private List<UI_InventorySlot> _uiSlots = new();
        private InventoryController _inventory;
        private int _draggingIndex = -1;

        public void Initialize(InventoryController inventory)
        {
            _inventory = inventory;
            _dragIcon.enabled = false;

            var slotsData = _inventory.GetAllSlots();
            for (int i = 0; i < slotsData.Count; i++)
            {
                var slotObj = Instantiate(_slotPrefab, _gridParent);
                var uiSlot = slotObj.GetComponent<UI_InventorySlot>();
                uiSlot.Bind(slotsData[i], i, this);
                _uiSlots.Add(uiSlot);
            }

            _inventory.OnSlotSelected += UpdateSelection;
        }

        private void UpdateSelection(int index)
        {
            for (int i = 0; i < _uiSlots.Count; i++) _uiSlots[i].SetHighlight(i == index);
        }

        public void NotifySlotClicked(int index) => _inventory.SelectSlot(index);

        public void StartDragging(int index, Sprite icon)
        {
            _draggingIndex = index;
            _dragIcon.sprite = icon;
            _dragIcon.enabled = true;
            _dragIcon.raycastTarget = false;
        }

        public void UpdateDragPosition(Vector2 position) => _dragIcon.transform.position = position;

        public void EndDragging(Vector2 screenPos)
        {
            _dragIcon.enabled = false;

            PointerEventData pointer = new PointerEventData(EventSystem.current) { position = screenPos };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            foreach (var result in results)
            {
                var targetSlot = result.gameObject.GetComponent<UI_InventorySlot>();
                if (targetSlot != null)
                {
                    int targetIndex = _uiSlots.IndexOf(targetSlot);
                    _inventory.SwapSlots(_draggingIndex, targetIndex);
                    return;
                }
            }

            _inventory.DropItem(_draggingIndex);
        }
    }
}