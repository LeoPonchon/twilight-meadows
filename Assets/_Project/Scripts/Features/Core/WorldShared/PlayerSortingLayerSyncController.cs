using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSortingLayerSyncController : MonoBehaviour
{
    [SerializeField] private SortingGroup source;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (source == null) source = GetComponentInParent<SortingGroup>();
        Apply();
    }

    private void LateUpdate()
    {
        Apply();
    }

    private void Apply()
    {
        if (spriteRenderer == null || source == null) return;
        spriteRenderer.sortingLayerID = source.sortingLayerID;
        spriteRenderer.sortingOrder = source.sortingOrder;
    }
}
