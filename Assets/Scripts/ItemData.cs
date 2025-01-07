using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;       // Nom de l'objet
    public string description;    // Description de l'objet
    public Sprite icon;           // Icône pour l'UI
    public bool isStackable;      // Indique si l'objet peut être empilé
}
