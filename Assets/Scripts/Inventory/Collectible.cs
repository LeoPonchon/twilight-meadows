using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Inventory inventory;
    private ItemData itemData;

    public void Setup(Inventory inventory, ItemData itemData)
    {
        this.inventory = inventory;
        this.itemData = itemData;

        Sprite spriteToUse = itemData.pickupSprite != null ? itemData.pickupSprite : itemData.icon;
        GetComponent<SpriteRenderer>().sprite = spriteToUse;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!inventory.CanAddItem(itemData, 1)) return;
        inventory.AddItem(itemData, 1);
        Destroy(gameObject);
    }
}

