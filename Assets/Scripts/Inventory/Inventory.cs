                                        using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Système d'inventaire principal qui gère les objets et leurs emplacements
/// </summary>
public class Inventory : MonoBehaviour
{
    #region Classes et Types

    /// <summary>
    /// Classe représentant un objet par défaut dans l'inventaire
    /// </summary>
    [Serializable]
    public class DefaultItem
    {
        public ItemData itemData; // Données de l'objet
        public int quantity = 1;  // Quantité par défaut
        public bool isHotbarTool = false; // Si vrai, cet objet sera placé dans la hotbar
    }

    #endregion

    #region Variables publiques

    [Header("Configuration")]
    [Tooltip("Nombre maximum de slots dans l'inventaire principal")]
    public int maxSlots = 20;

    [Tooltip("Nombre maximum de slots dans la hotbar")]
    public int maxHotbarSlots = 10;

    [Header("Items par défaut")]
    [Tooltip("Configure les objets disponibles au démarrage")]
    public List<DefaultItem> defaultItems = new List<DefaultItem>();

    #endregion

    #region Variables privées

    // Dictionnaire stockant les objets de l'inventaire: clé = ID du slot, valeur = pile d'objets
    private Dictionary<int, ItemStack> items = new Dictionary<int, ItemStack>();

    #endregion

    #region Événements

    /// <summary>
    /// Événement déclenché lorsque l'inventaire change
    /// </summary>
    public event Action OnInventoryChanged;

    #endregion

    #region Méthodes Unity

    /// <summary>
    /// Initialise l'inventaire au démarrage
    /// </summary>
    public void Start()
    {
        InitializeInventory();
    }

    #endregion

    #region Méthodes d'initialisation

    /// <summary>
    /// Initialise l'inventaire avec les objets par défaut
    /// </summary>
    private void InitializeInventory()
    {
        // Trier les items: outils pour la hotbar d'abord, puis le reste
        List<DefaultItem> hotbarTools = new List<DefaultItem>();
        List<DefaultItem> inventoryItems = new List<DefaultItem>();

        // Séparer les objets en deux catégories
        SeparateDefaultItems(hotbarTools, inventoryItems);

        // Ajouter les outils à la hotbar
        AddToolsToHotbar(hotbarTools);

        // Ajouter les autres objets à l'inventaire
        AddItemsToInventory(inventoryItems);

        // Notifier les écouteurs que l'inventaire a changé
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Sépare les objets par défaut en outils et objets ordinaires
    /// </summary>
    private void SeparateDefaultItems(List<DefaultItem> hotbarTools, List<DefaultItem> inventoryItems)
    {
        foreach (DefaultItem item in defaultItems)
        {
            if (item.itemData == null) continue;

            if (item.isHotbarTool)
                hotbarTools.Add(item);
            else
                inventoryItems.Add(item);
        }
    }

    /// <summary>
    /// Ajoute les outils à la hotbar
    /// </summary>
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

            items[hotbarIndex] = new ItemStack(tool.itemData, tool.quantity);
            hotbarIndex++;
        }
    }

    /// <summary>
    /// Ajoute les objets ordinaires à l'inventaire
    /// </summary>
    private void AddItemsToInventory(List<DefaultItem> inventoryItems)
    {
        foreach (DefaultItem inventoryItem in inventoryItems)
        {
            AddItem(inventoryItem.itemData, inventoryItem.quantity);
        }
    }

    #endregion

    #region Méthodes publiques - Interface IInventorySystem

    /// <summary>
    /// Vérifie si un objet peut être ajouté à l'inventaire
    /// </summary>
    public bool CanAddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null) return false;

        // Si l'objet est empilable, vérifie s'il existe déjà dans l'inventaire
        if (itemData.isStackable)
        {
            foreach (var item in items)
            {
                if (item.Value.itemData == itemData)
                {
                    return true;
                }
            }
        }

        // Sinon, vérifie s'il y a assez de place dans l'inventaire
        return items.Count + (itemData.isStackable ? 1 : quantity) <= maxHotbarSlots + maxSlots;
    }

    /// <summary>
    /// Ajoute un objet à l'inventaire
    /// </summary>
    public void AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return;

        // Pour les objets empilables, essaie d'abord de les ajouter à une pile existante
        if (itemData.isStackable)
        {
            foreach (var item in items)
            {
                if (item.Value.itemData == itemData)
                {
                    item.Value.AddToStack(quantity);
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
        }

        // Cherche un slot libre dans l'inventaire principal d'abord (pas dans la hotbar)
        AddItemToFirstFreeSlot(itemData, quantity, maxHotbarSlots, maxHotbarSlots + maxSlots);
    }

    /// <summary>
    /// Retire un objet de l'inventaire
    /// </summary>
    public void RemoveItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return;

        foreach (var item in items)
        {
            if (item.Value.itemData == itemData)
            {
                if (item.Value.RemoveFromStack(quantity))
                {
                    if (item.Value.IsEmpty())
                    {
                        items.Remove(item.Key);
                    }

                    OnInventoryChanged?.Invoke();
                }
                return;
            }
        }

        Debug.LogWarning($"Item '{itemData.itemName}' not found in inventory.");
    }

    /// <summary>
    /// Récupère tous les objets de l'inventaire avec leurs IDs
    /// </summary>
    public Dictionary<int, ItemStack> GetAllItemsWithIDs()
    {
        return new Dictionary<int, ItemStack>(items);
    }

    /// <summary>
    /// Récupère l'objet dans un slot spécifique
    /// </summary>
    public ItemStack GetItemInSlot(int slotIndex)
    {
        if (items.TryGetValue(slotIndex, out ItemStack itemStack))
        {
            return itemStack;
        }
        return null;
    }

    /// <summary>
    /// Ajoute un objet à un slot spécifique
    /// </summary>
    public void AddItemToSlot(int slotIndex, ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return;

        if (slotIndex < 0 || slotIndex >= maxHotbarSlots + maxSlots) return;

        if (items.ContainsKey(slotIndex))
        {
            if (itemData.isStackable && items[slotIndex].itemData == itemData)
            {
                // Si l'objet est empilable et le slot contient déjà cet objet
                items[slotIndex].AddToStack(quantity);
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

    /// <summary>
    /// Retire un objet d'un slot spécifique
    /// </summary>
    public void RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (quantity <= 0) return;

        if (slotIndex < 0 || slotIndex >= maxHotbarSlots + maxSlots) return;

        if (items.TryGetValue(slotIndex, out ItemStack itemStack))
        {
            if (itemStack.RemoveFromStack(quantity) && itemStack.IsEmpty())
            {
                items.Remove(slotIndex);
            }

            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"No item found in slot {slotIndex}.");
        }
    }

    /// <summary>
    /// Vérifie si un slot appartient à la hotbar
    /// </summary>
    public bool IsHotbarSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < maxHotbarSlots;
    }
    
    /// <summary>
    /// Vérifie si le joueur possède un item en quantité suffisante
    /// </summary>
    public bool HasItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return false;
        
        int totalQuantity = 0;
        foreach (var item in items)
        {
            if (item.Value.itemData == itemData)
            {
                totalQuantity += item.Value.quantity;
                if (totalQuantity >= quantity)
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Obtient la quantité totale d'un item dans l'inventaire
    /// </summary>
    public int GetItemQuantity(ItemData itemData)
    {
        if (itemData == null) return 0;
        
        int totalQuantity = 0;
        foreach (var item in items)
        {
            if (item.Value.itemData == itemData)
            {
                totalQuantity += item.Value.quantity;
            }
        }
        
        return totalQuantity;
    }

    #endregion

    #region Méthodes privées

    /// <summary>
    /// Ajoute un objet au premier slot libre dans une plage donnée
    /// </summary>
    private void AddItemToFirstFreeSlot(ItemData itemData, int quantity, int startSlot, int endSlot)
    {
        // Cherche un slot libre dans la plage spécifiée
        for (int i = startSlot; i < endSlot; i++)
        {
            if (!items.ContainsKey(i))
            {
                items[i] = new ItemStack(itemData, quantity);
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        // Si la plage spécifiée est l'inventaire principal et qu'il est plein,
        // essaie de mettre l'objet dans la hotbar en dernier recours
        if (startSlot == maxHotbarSlots)
        {
            AddItemToFirstFreeSlot(itemData, quantity, 0, maxHotbarSlots);
            return;
        }

    }

    #endregion
}
