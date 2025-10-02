using UnityEngine;

/// <summary>
/// Représente une pile d'objets dans l'inventaire
/// </summary>
[System.Serializable]
public class ItemStack
{
    /// <summary>
    /// Données de l'objet
    /// </summary>
    public ItemData itemData;

    /// <summary>
    /// Quantité de cet objet dans la pile
    /// </summary>
    public int quantity;

    /// <summary>
    /// Instance de l'objet (pour les outils avec état)
    /// </summary>
    public ItemInstance itemInstance;

    /// <summary>
    /// Constructeur d'une pile d'objets
    /// </summary>
    /// <param name="data">Données de l'objet</param>
    /// <param name="qty">Quantité initiale (défaut: 1)</param>
    public ItemStack(ItemData data, int qty = 1)
    {
        if (data == null)
        {
            Debug.LogError("Tentative de création d'un ItemStack avec un ItemData null");
            return;
        }

        itemData = data;
        quantity = Mathf.Max(0, qty); // Empêche les quantités négatives
        
        // Créer une instance si c'est un outil
        if (ItemInstanceFactory.IsTool(data))
        {
            itemInstance = ItemInstanceFactory.CreateInstance(data);
        }
    }

    /// <summary>
    /// Ajoute une quantité à la pile
    /// </summary>
    /// <param name="amount">Quantité à ajouter</param>
    /// <returns>True si l'ajout a réussi</returns>
    public bool AddToStack(int amount)
    {
        if (amount <= 0) return false;

        quantity += amount;
        return true;
    }

    /// <summary>
    /// Retire une quantité de la pile
    /// </summary>
    /// <param name="amount">Quantité à retirer</param>
    /// <returns>True si le retrait a réussi</returns>
    public bool RemoveFromStack(int amount)
    {
        if (amount <= 0 || amount > quantity) return false;

        quantity -= amount;
        return true;
    }

    /// <summary>
    /// Vérifie si la pile est vide
    /// </summary>
    public bool IsEmpty()
    {
        return quantity <= 0;
    }

    /// <summary>
    /// Clone cette pile d'objets
    /// </summary>
    public ItemStack Clone()
    {
        return new ItemStack(itemData, quantity);
    }

    /// <summary>
    /// Obtient l'instance de l'outil (crée une nouvelle si nécessaire)
    /// </summary>
    public ItemInstance GetItemInstance()
    {
        if (itemInstance == null && ItemInstanceFactory.IsTool(itemData))
        {
            itemInstance = ItemInstanceFactory.CreateInstance(itemData);
        }
        return itemInstance;
    }

    /// <summary>
    /// Vérifie si cet item a une instance (outil avec état)
    /// </summary>
    public bool HasInstance()
    {
        return itemInstance != null;
    }

    /// <summary>
    /// Représentation textuelle de la pile
    /// </summary>
    public override string ToString()
    {
        if (itemData == null) return "Item: null";
        return $"{itemData.itemName} x{quantity}";
    }
}
