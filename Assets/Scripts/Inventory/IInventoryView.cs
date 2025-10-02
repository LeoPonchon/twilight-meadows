using UnityEngine;

/// <summary>
/// Interface définissant les fonctionnalités d'une vue d'inventaire
/// </summary>
public interface IInventoryView
{
    /// <summary>
    /// Met à jour la vue de l'inventaire
    /// </summary>
    void UpdateView();

    /// <summary>
    /// Crée les slots visuels
    /// </summary>
    void CreateSlots();

    /// <summary>
    /// Nettoie les slots visuels
    /// </summary>
    void ClearSlots();

    /// <summary>
    /// Change l'image d'un slot spécifique
    /// </summary>
    void SetSlotImage(int slotID, Sprite sprite);

    /// <summary>
    /// Obtient l'objet GameObject d'un slot spécifique
    /// </summary>
    GameObject GetSlot(int slotID);
}