using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Inventory inventory;   // R�f�rence � l'inventaire du joueur
    private ItemData itemData;     // R�f�rence � l'objet � ajouter � l'inventaire

    public void Setup(Inventory inventory, ItemData itemData)
    {
        this.inventory = inventory;
        this.itemData = itemData;
        
        // Utiliser le sprite de pickup s'il existe, sinon utiliser l'icône
        Sprite spriteToUse = itemData.pickupSprite != null ? itemData.pickupSprite : itemData.icon;
        GetComponent<SpriteRenderer>().sprite = spriteToUse;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (inventory.CanAddItem(itemData, 1))
            {
                inventory.AddItem(itemData, 1);
                Destroy(gameObject);
            }
        }
    }
}
