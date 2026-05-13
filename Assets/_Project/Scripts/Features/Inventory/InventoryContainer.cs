using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Système d'inventaire principal qui gère les objets et leurs emplacements.
/// </summary>
public class InventoryContainer : MonoBehaviour
{
    [Serializable]
    public class DefaultItem
    {
        public ItemData itemData;
        public int quantity = 1;
        public bool isHotbarTool = false;
    }

    [Header("Configuration")]
    [Tooltip("Nombre maximum de slots dans l'inventaire principal")]
    public int maxSlots = 20;

    [Tooltip("Nombre maximum de slots dans la hotbar")]
    public int maxHotbarSlots = 10;

    [Header("Items par défaut")]
    [Tooltip("Configure les objets disponibles au démarrage")]
    public List<DefaultItem> defaultItems = new List<DefaultItem>();

    public event Action OnInventoryChanged;

    private InventoryCore<ItemData, ItemStack> core;

    public void Start()
    {
        core = new InventoryCore<ItemData, ItemStack>(
            hotbarSlots: maxHotbarSlots,
            inventorySlots: maxSlots,
            getItem: s => s.itemData,
            getQuantity: s => s.quantity,
            setQuantity: (s, q) => s.quantity = q,
            isStackable: i => i != null && i.isStackable,
            createStack: (i, q) => new ItemStack(i, q));
        core.Changed += () => OnInventoryChanged?.Invoke();

        InitializeInventory();
    }

    private void InitializeInventory()
    {
        List<DefaultItem> hotbarTools = new List<DefaultItem>();
        List<DefaultItem> inventoryItems = new List<DefaultItem>();

        SeparateDefaultItems(hotbarTools, inventoryItems);
        AddToolsToHotbar(hotbarTools);
        AddItemsToInventory(inventoryItems);

        OnInventoryChanged?.Invoke();
    }

    private void SeparateDefaultItems(List<DefaultItem> hotbarTools, List<DefaultItem> inventoryItems)
    {
        foreach (DefaultItem item in defaultItems)
        {
            if (item.itemData == null) continue;
            if (item.isHotbarTool) hotbarTools.Add(item);
            else inventoryItems.Add(item);
        }
    }

    private void AddToolsToHotbar(List<DefaultItem> hotbarTools)
    {
        int hotbarIndex = 0;
        foreach (DefaultItem tool in hotbarTools)
        {
            if (hotbarIndex >= maxHotbarSlots)
            {
                AddItem(tool.itemData, tool.quantity);
                continue;
            }
            core.AddToSlot(hotbarIndex, tool.itemData, tool.quantity);
            hotbarIndex++;
        }
    }

    private void AddItemsToInventory(List<DefaultItem> inventoryItems)
    {
        foreach (DefaultItem inventoryItem in inventoryItems)
        {
            AddItem(inventoryItem.itemData, inventoryItem.quantity);
        }
    }

    public bool CanAddItem(ItemData itemData, int quantity = 1)
    {
        return core != null && core.CanAdd(itemData, quantity);
    }

    public void AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return;
        core?.Add(itemData, quantity);
    }

    public void RemoveItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return;
        core?.Remove(itemData, quantity);
    }

    public Dictionary<int, ItemStack> GetAllItemsWithIDs()
    {
        return core != null ? core.Snapshot() : new Dictionary<int, ItemStack>();
    }

    public ItemStack GetItemInSlot(int slotIndex)
    {
        return core != null ? core.GetInSlot(slotIndex) : null;
    }

    public void AddItemToSlot(int slotIndex, ItemData itemData, int quantity = 1)
    {
        core?.AddToSlot(slotIndex, itemData, quantity);
    }

    public void RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        core?.RemoveFromSlot(slotIndex, quantity);
    }

    public bool IsHotbarSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < maxHotbarSlots;
    }

    public bool HasItem(ItemData itemData, int quantity = 1)
    {
        return core != null && core.Has(itemData, quantity);
    }

    public int GetItemQuantity(ItemData itemData)
    {
        return core != null ? core.GetQuantityTotal(itemData) : 0;
    }
}
