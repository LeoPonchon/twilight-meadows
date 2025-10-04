using UnityEngine;

public class PlayerLayerSync : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        var player = FindObjectOfType<TopDownMovement>();
        if (player != null)
        {
            playerSprite = player.GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        SyncWithPlayer();
    }

    private void SyncWithPlayer()
    {
        if (spriteRenderer == null || playerSprite == null) return;

        spriteRenderer.sortingLayerID = playerSprite.sortingLayerID;
        spriteRenderer.sortingOrder = playerSprite.sortingOrder;
    }
}
