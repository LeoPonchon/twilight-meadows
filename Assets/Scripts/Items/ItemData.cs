using UnityEngine;

public enum ToolKind
{
    None,
    Axe,
    Pickaxe,
    Hoe,
    Shovel,
    WateringCan,
    FishingRod,
    Sword,
    Spear
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Informations de base")]
    public string itemName;       // Nom de l'objet
    public string description;    // Description de l'objet
    public Sprite icon;           // Icône pour l'UI
    [Tooltip("Sprite affiché quand l'item est droppé au sol (pickup). Si vide, utilise l'icône.")]
    public Sprite pickupSprite;   // Sprite pour le pickup au sol

    [Header("Propriétés")]
    public bool isStackable;      // Indique si l'objet peut être empilé

    [Header("Economy")]
    [Tooltip("Prix de vente unitaire de cet item")]
    public int sellPrice = 0;
}
