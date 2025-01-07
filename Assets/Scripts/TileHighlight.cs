using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHighlight : MonoBehaviour
{
    [SerializeField] private GameObject highlightBorderPrefab;

    private GameObject currentHighlight;

    public void HighlightTile(Vector3Int cellPosition, Tilemap tilemap)
    {
        if (currentHighlight == null)
        {
            currentHighlight = Instantiate(highlightBorderPrefab, tilemap.CellToWorld(cellPosition), Quaternion.identity);
            currentHighlight.transform.SetParent(transform);
        }

        currentHighlight.transform.position = tilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0);
    }

    public void RemoveHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }
    }
}
