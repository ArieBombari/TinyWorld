using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count = 1;
        public bool IsEmpty => item == null;

        public InventorySlot() { item = null; count = 0; }
        public InventorySlot(ItemData item, int count = 1) { this.item = item; this.count = count; }
        public void Clear() { item = null; count = 0; }
    }

    [CreateAssetMenu(fileName = "PlayerInventory", menuName = "Game/Items/Inventory Data")]
    public class InventoryData : ScriptableObject
    {
        [Header("Configuration")]
        public int slotCount = 10;

        [Header("Runtime State")]
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
        [SerializeField] private int activeSlotIndex = 0;

        public event Action OnInventoryChanged;
        public event Action<int> OnActiveSlotChanged;
        public event Action<ItemData> OnItemAdded;
        public event Action<ItemData> OnItemRemoved;

        public int ActiveSlotIndex => activeSlotIndex;
        public IReadOnlyList<InventorySlot> Slots => slots;

        public void Initialize()
        {
            slots.Clear();
            for (int i = 0; i < slotCount; i++)
                slots.Add(new InventorySlot());
            activeSlotIndex = 0;
        }

        public void EnsureSlots()
        {
            while (slots.Count < slotCount) slots.Add(new InventorySlot());
            while (slots.Count > slotCount) slots.RemoveAt(slots.Count - 1);
        }

        public void SetActiveSlot(int index)
        {
            index = Mathf.Clamp(index, 0, slotCount - 1);
            if (index == activeSlotIndex) return;
            activeSlotIndex = index;
            OnActiveSlotChanged?.Invoke(activeSlotIndex);
        }

        public void MoveActiveSlot(int direction)
        {
            int newIndex = activeSlotIndex + direction;
            if (newIndex < 0) newIndex = slotCount - 1;
            if (newIndex >= slotCount) newIndex = 0;
            SetActiveSlot(newIndex);
        }

        public InventorySlot GetActiveSlot()
        {
            if (activeSlotIndex >= 0 && activeSlotIndex < slots.Count)
                return slots[activeSlotIndex];
            return null;
        }

        public bool AddItem(ItemData item, int count = 1)
        {
            if (item == null) return false;

            if (item.stackable)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].item == item && slots[i].count < item.maxStack)
                    {
                        slots[i].count = Mathf.Min(slots[i].count + count, item.maxStack);
                        OnItemAdded?.Invoke(item);
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].item = item;
                    slots[i].count = count;
                    OnItemAdded?.Invoke(item);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveItem(ItemData item, int count = 1)
        {
            if (item == null) return false;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].item == item)
                {
                    slots[i].count -= count;
                    if (slots[i].count <= 0) slots[i].Clear();
                    OnItemRemoved?.Invoke(item);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveAtSlot(int index, int count = 1)
        {
            if (index < 0 || index >= slots.Count || slots[index].IsEmpty) return false;
            var item = slots[index].item;
            slots[index].count -= count;
            if (slots[index].count <= 0) slots[index].Clear();
            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(ItemData item, int count = 1)
        {
            if (item == null) return false;
            int total = 0;
            for (int i = 0; i < slots.Count; i++)
                if (slots[i].item == item) total += slots[i].count;
            return total >= count;
        }

        public bool HasItem(string itemID, int count = 1)
        {
            int total = 0;
            for (int i = 0; i < slots.Count; i++)
                if (!slots[i].IsEmpty && slots[i].item.itemID == itemID) total += slots[i].count;
            return total >= count;
        }

        public bool IsFull()
        {
            for (int i = 0; i < slots.Count; i++)
                if (slots[i].IsEmpty) return false;
            return true;
        }
    }
}
