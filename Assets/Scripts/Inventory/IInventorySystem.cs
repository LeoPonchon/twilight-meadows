using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface définissant les fonctionnalités essentielles d'un système d'inventaire
/// </summary>
public interface IInventorySystem
{
    /// <summary>
    /// Événement déclenché lorsque l'inventaire change
    /// </summary>
    event Action OnInventoryChanged;

    /// <summary>
    /// Vérifie si un objet peut être ajouté à l'inventaire
    /// </summary>
    bool CanAddItem(ItemData itemData, int quantity = 1);

    /// <summary>
    /// Ajoute un objet à l'inventaire
    /// </summary>
    void AddItem(ItemData itemData, int quantity = 1);

    /// <summary>
    /// Retire un objet de l'inventaire
    /// </summary>
    void RemoveItem(ItemData itemData, int quantity = 1);

    /// <summary>
    /// Récupère tous les objets de l'inventaire avec leurs IDs
    /// </summary>
    Dictionary<int, ItemStack> GetAllItemsWithIDs();

    /// <summary>
    /// Récupère l'objet dans un slot spécifique
    /// </summary>
    ItemStack GetItemInSlot(int slotIndex);

    /// <summary>
    /// Ajoute un objet à un slot spécifique
    /// </summary>
    void AddItemToSlot(int slotIndex, ItemData itemData, int quantity = 1);

    /// <summary>
    /// Retire un objet d'un slot spécifique
    /// </summary>
    void RemoveItemFromSlot(int slotIndex, int quantity = 1);
}