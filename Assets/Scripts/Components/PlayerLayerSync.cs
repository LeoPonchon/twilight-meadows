using UnityEngine;
using UnityEngine.Rendering;

public class PlayerLayerSync : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SortingGroup playerSortingGroup;

    [SerializeField] private TopDownMovement player;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            player = FindObjectOfType<TopDownMovement>();
        }
        if (player != null)
        {
            playerSortingGroup = player.GetComponent<SortingGroup>();
        }
    }

    private void Start()
    {
        SyncWithPlayer();
    }

    private void SyncWithPlayer()
    {
        if (spriteRenderer == null || playerSortingGroup == null) return;

        spriteRenderer.sortingLayerID = playerSortingGroup.sortingLayerID;
        spriteRenderer.sortingOrder = playerSortingGroup.sortingOrder;
    }
}
