using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 20; // Nombre maximum de slots
    public int maxHotbarSlots = 10; // Nombre maximum de slots dans la hotbar

    public List<ItemData> defaultItems = new List<ItemData>(); // Liste personnalisable dans l'inspecteur
    private Dictionary<int, ItemStack> items = new Dictionary<int, ItemStack>();

    public event Action OnInventoryChanged;

    public void Start()
    {
        foreach(ItemData item in defaultItems)
        {
            AddItem(item, 1);
        }
    }

    public bool CanAddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData.isStackable)
        {
            foreach (var item in items)
            {
                if (item.Value.itemData == itemData)
                {
                    return true;
                }
            }

            return items.Count < maxHotbarSlots + maxSlots;
        }

        return items.Count + quantity <= maxHotbarSlots + maxSlots;
    }

    public void AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData.isStackable)
        {
            foreach (var item in items)
            {
                if (item.Value.itemData == itemData)
                {
                    item.Value.quantity += quantity;
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
        }

        for (int i = 0; i < maxHotbarSlots + maxSlots; i++)
        {
            if (!items.ContainsKey(i))
            {
                items[i] = new ItemStack(itemData, quantity);
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        Debug.LogWarning("No available slots in inventory.");
    }

    public void RemoveItem(ItemData itemData, int quantity = 1)
    {
        foreach (var item in items)
        {
            if (item.Value.itemData == itemData)
            {
                item.Value.quantity -= quantity;

                if (item.Value.quantity <= 0)
                {
                    items.Remove(item.Key);
                }

                OnInventoryChanged?.Invoke();
                return;
            }
        }

        Debug.LogWarning($"Item '{itemData.itemName}' not found in inventory.");
    }

    public Dictionary<int, ItemStack> GetAllItemsWithIDs()
    {
        return new Dictionary<int, ItemStack>(items);
    }

    public ItemStack GetItemInSlot(int slotIndex)
    {
        if (items.TryGetValue(slotIndex, out ItemStack itemStack))
        {
            return itemStack;
        }
        return null;
    }
    public void MoveItem(int fromSlotID, int toSlotID)
    {
        if (items.ContainsKey(fromSlotID) && !items.ContainsKey(toSlotID) && !(fromSlotID == toSlotID))
        {
            items[toSlotID] = items[fromSlotID];
            items.Remove(fromSlotID);
            OnInventoryChanged?.Invoke();
        }
        else if (fromSlotID == toSlotID)
        {
            Debug.Log("Item déplacé dans la case d'origine.");
        }
        else
        {
            Debug.LogWarning("Impossible de déplacer l'item.");
        }
    }

}
