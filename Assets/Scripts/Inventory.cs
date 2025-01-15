using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Serializable]
    public class DefaultItem
    {
        public ItemData itemData; // Données de l'objet
        public int quantity = 1;  // Quantité par défaut
    }

    public int maxSlots = 20; // Nombre maximum de slots    
    public int maxHotbarSlots = 10; // Nombre maximum de slots dans la hotbar

    public List<DefaultItem> defaultItems = new List<DefaultItem>(); // Liste personnalisable dans l'inspecteur
    private Dictionary<int, ItemStack> items = new Dictionary<int, ItemStack>();

    public event Action OnInventoryChanged;

    public void Start()
    {
        foreach (DefaultItem item in defaultItems)
        {
            AddItem(item.itemData, item.quantity);
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
    public void AddItemToSlot(int slotIndex, ItemData itemData, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= maxHotbarSlots + maxSlots)
        {
            Debug.LogWarning("Invalid slot index.");
            return;
        }

        if (items.ContainsKey(slotIndex))
        {
            if (itemData.isStackable && items[slotIndex].itemData == itemData)
            {
                // Si l'objet est empilable et le slot contient déjà cet objet
                items[slotIndex].quantity += quantity;
            }
            else
            {
                Debug.LogWarning($"Slot {slotIndex} is already occupied with a different item.");
            }
        }
        else
        {
            // Ajouter un nouvel objet dans le slot
            items[slotIndex] = new ItemStack(itemData, quantity);
        }

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= maxHotbarSlots + maxSlots)
        {
            Debug.LogWarning("Invalid slot index.");
            return;
        }

        if (items.ContainsKey(slotIndex))
        {
            ItemStack itemStack = items[slotIndex];
            itemStack.quantity -= quantity;

            if (itemStack.quantity <= 0)
            {
                // Si la quantité tombe à 0 ou moins, retirer l'objet du slot
                items.Remove(slotIndex);
            }

            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"No item found in slot {slotIndex}.");
        }
    }

}
