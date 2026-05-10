using System;
using TheUnique.Core.Items;

namespace TheUnique.Core.Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public ItemSO Item { get; private set; }
        public int Amount { get; private set; }

        public bool IsEmpty => Item == null || Amount <= 0;
        public bool IsFull => Item != null && Amount >= Item.maxStack;

        public event Action OnSlotChanged;

        public void SetItem(ItemSO newItem, int newAmount)
        {
            Item = newItem;
            Amount = newAmount;
            OnSlotChanged?.Invoke();
        }

        public int AddAmountWithOverflow(int value)
        {
            int canAdd = Item.maxStack - Amount;
            int toAdd = Math.Min(canAdd, value);
            Amount += toAdd;
            OnSlotChanged?.Invoke();
            return toAdd;
        }

        public void RemoveAmount(int value)
        {
            Amount -= value;
            if (Amount <= 0) Clear();
            else OnSlotChanged?.Invoke();
        }

        public void Clear()
        {
            Item = null;
            Amount = 0;
            OnSlotChanged?.Invoke();
        }
    }
}