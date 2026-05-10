using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using TheUnique.Core.Inventory;

namespace TheUnique.UI
{
    public class UI_InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Ссылки")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _highlight;

        private InventorySlot _linkedSlot;
        private int _slotIndex;
        private UI_InventoryDisplay _display;

        public void Bind(InventorySlot slot, int index, UI_InventoryDisplay display)
        {
            _linkedSlot = slot;
            _slotIndex = index;
            _display = display;
            
            _linkedSlot.OnSlotChanged += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            if (_linkedSlot.IsEmpty)
            {
                _iconImage.enabled = false;
                _amountText.text = "";
            }
            else
            {
                _iconImage.enabled = true;
                _iconImage.sprite = _linkedSlot.Item.icon;
                _amountText.text = _linkedSlot.Amount > 1 ? _linkedSlot.Amount.ToString() : "";
            }
        }

        public void SetHighlight(bool active) => _highlight.enabled = active;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_linkedSlot.IsEmpty) return;
            _display.StartDragging(_slotIndex, _linkedSlot.Item.icon);
            _iconImage.color = new Color(1, 1, 1, 0.5f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _display.UpdateDragPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _iconImage.color = Color.white;
            _display.EndDragging(eventData.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _display.NotifySlotClicked(_slotIndex);
        }
    }
}