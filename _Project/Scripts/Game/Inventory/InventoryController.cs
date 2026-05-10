using System;
using System.Collections.Generic;
using UnityEngine;
using TheUnique.Core.Items;
using TheUnique.Core.World;

namespace TheUnique.Core.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private int _inventorySize = 24;
        private List<InventorySlot> _slots;
        public int SelectedSlotIndex { get; private set; }

        public event Action OnInventoryUpdated;
        public event Action<int> OnSlotSelected;

        private void Awake() => InitializeInventory();

        private void InitializeInventory()
        {
            _slots = new List<InventorySlot>(_inventorySize);
            for (int i = 0; i < _inventorySize; i++) _slots.Add(new InventorySlot());
        }

        public bool AddItem(ItemSO item, int amount)
        {
            if (item.isStackable)
            {
                foreach (var slot in _slots)
                {
                    if (!slot.IsEmpty && slot.Item == item && !slot.IsFull)
                    {
                        int added = slot.AddAmountWithOverflow(amount);
                        amount -= added;
                        if (amount <= 0) { OnInventoryUpdated?.Invoke(); return true; }
                    }
                }
            }

            while (amount > 0)
            {
                var empty = _slots.Find(s => s.IsEmpty);
                if (empty == null) { OnInventoryUpdated?.Invoke(); return false; }
                
                int toAdd = item.isStackable ? Mathf.Min(amount, item.maxStack) : 1;
                empty.SetItem(item, toAdd);
                amount -= toAdd;
            }

            OnInventoryUpdated?.Invoke();
            return true;
        }

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexB < 0 || indexA >= _slots.Count || indexB >= _slots.Count) return;

            var itemA = _slots[indexA].Item;
            var amountA = _slots[indexA].Amount;

            _slots[indexA].SetItem(_slots[indexB].Item, _slots[indexB].Amount);
            _slots[indexB].SetItem(itemA, amountA);

            OnInventoryUpdated?.Invoke();
        }

        public void DropItem(int index)
        {
            var slot = _slots[index];
            if (slot.IsEmpty) return;

            if (slot.Item.worldItemPrefab != null)
            {
                GameObject drop = Instantiate(slot.Item.worldItemPrefab, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle, Quaternion.identity);
                drop.GetComponent<WorldItem>()?.Initialize(slot.Item, slot.Amount);
            }
            slot.Clear();
            OnInventoryUpdated?.Invoke();
        }

        public void SelectSlot(int index) { SelectedSlotIndex = index; OnSlotSelected?.Invoke(index); }
        public ItemSO GetActiveItem() => _slots[SelectedSlotIndex].Item;
        public IReadOnlyList<InventorySlot> GetAllSlots() => _slots;

        public bool RemoveItem(ItemSO itemToRemove, int amount)
        {
            int count = 0;
            foreach (var slot in _slots) if (!slot.IsEmpty && slot.Item == itemToRemove) count += slot.Amount;
            if (count < amount) return false;

            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (!_slots[i].IsEmpty && _slots[i].Item == itemToRemove)
                {
                    int toRemove = Mathf.Min(_slots[i].Amount, amount);
                    _slots[i].RemoveAmount(toRemove);
                    amount -= toRemove;
                    if (amount <= 0) break;
                }
            }
            OnInventoryUpdated?.Invoke();
            return true;
        }
    }
}