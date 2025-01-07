using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Inventory inventory;   // Référence à l'inventaire du joueur
    private ItemData itemData;     // Référence à l'objet à ajouter à l'inventaire

    public void Setup(Inventory inventory, ItemData itemData)
    {
        this.inventory = inventory;
        this.itemData = itemData;
        GetComponent<SpriteRenderer>().sprite = itemData.icon;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Vérifie si le joueur entre en contact
        {
            // Vérifie si l'objet peut être ajouté
            if (inventory.CanAddItem(itemData, 1))
            {
                inventory.AddItem(itemData, 1); // Ajoute 1 unité à l'inventaire
                Debug.Log($"Ramassé : {itemData.itemName}");
                Destroy(gameObject); // Détruit le sprite ramassable
            }
            else
            {
                Debug.Log($"Pas assez de place pour : {itemData.itemName}");
            }
        }
    }
}
