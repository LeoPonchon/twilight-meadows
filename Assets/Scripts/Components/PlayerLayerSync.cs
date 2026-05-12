using UnityEngine;
using UnityEngine.Rendering;

public class PlayerLayerSync : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SortingGroup playerSortingGroup;

    [SerializeField] private TopDownMovement player;
    [SerializeField] private SceneContext sceneContext;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            if (sceneContext == null)
            {
                sceneContext = FindObjectOfType<SceneContext>();
            }
            player = sceneContext != null ? sceneContext.GetRequired<TopDownMovement>(this, nameof(player)) : null;
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
