using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHighlight : MonoBehaviour
{
    [SerializeField] private GameObject highlightBorderPrefab;
    [SerializeField] private Tilemap targetTilemap; // Tilemap sur laquelle on veut highlight

    private GameObject currentHighlight;
    private Vector3Int lastHighlightedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    private void Update()
    {
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        if (targetTilemap == null || highlightBorderPrefab == null)
            return;

        // Obtenir la position de la souris dans le monde
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Convertir en position de cellule
        Vector3Int cellPosition = targetTilemap.WorldToCell(mouseWorldPos);

        // Vérifier si la cellule a changé
        if (cellPosition != lastHighlightedCell)
        {
            lastHighlightedCell = cellPosition;
            
            // Vérifier s'il y a une tile à cette position
            TileBase tile = targetTilemap.GetTile(cellPosition);
            if (tile != null)
            {
                HighlightTile(cellPosition);
            }
            else
            {
                RemoveHighlight();
            }
        }
    }

    private void HighlightTile(Vector3Int cellPosition)
    {
        if (currentHighlight == null)
        {
            currentHighlight = Instantiate(highlightBorderPrefab);
            currentHighlight.transform.SetParent(transform);
        }

        // Positionner le highlight au centre de la cellule
        Vector3 worldPos = targetTilemap.CellToWorld(cellPosition);
        currentHighlight.transform.position = worldPos + new Vector3(0.5f, 0.5f, 0);
    }

    public void RemoveHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
        lastHighlightedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    }

    private void OnDestroy()
    {
        RemoveHighlight();
    }
}
