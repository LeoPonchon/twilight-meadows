using UnityEngine;
using UnityEngine.Tilemaps;

public class SoilMap : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap overGroundTilemap;

    [Header("Tiles de terrain")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase dirtTile;
    [SerializeField] private TileBase soilTile;

    [Header("Variantes visuelles du soil (optionnel)")]
    [SerializeField] private TileBase soilWetTile;
    [SerializeField] private TileBase soilDryTile;

    public Vector3Int WorldToCell(Vector3 world)
    {
        if (groundTilemap == null)
        {
            world.z = 0f;
            return Vector3Int.FloorToInt(world);
        }
        return groundTilemap.WorldToCell(world);
    }

    public bool HasGroundAt(Vector3Int cell)
    {
        return groundTilemap != null && groundTilemap.GetTile(cell) != null;
    }

    public bool IsGrassCell(Vector3Int cell)
    {
        return groundTilemap != null && groundTilemap.GetTile(cell) == grassTile;
    }

    public bool IsDirtCell(Vector3Int cell)
    {
        if (overGroundTilemap == null) return false;
        var t = overGroundTilemap.GetTile(cell);
        return t != null && t == dirtTile;
    }

    public bool IsSoilCell(Vector3Int cell)
    {
        if (overGroundTilemap == null) return false;
        var t = overGroundTilemap.GetTile(cell);
        return t != null && (t == soilTile || t == soilWetTile || t == soilDryTile);
    }

    public bool IsSoil(Vector3Int cell) => IsSoilCell(cell);

    public void SetSoilWet(Vector3Int cell, bool wet)
    {
        if (overGroundTilemap == null) return;
        if (!IsSoilCell(cell)) return;

        if (wet)
        {
            if (soilWetTile != null)
                overGroundTilemap.SetTile(cell, soilWetTile);
            else if (soilTile != null)
                overGroundTilemap.SetTile(cell, soilTile);
        }
        else
        {
            if (soilDryTile != null)
                overGroundTilemap.SetTile(cell, soilDryTile);
            else if (soilTile != null)
                overGroundTilemap.SetTile(cell, soilTile);
        }
        RefreshNeighbors(cell);
    }

    public bool PlaceDirt(Vector3Int cell)
    {
        if (overGroundTilemap == null) return false;
        overGroundTilemap.SetTile(cell, dirtTile);
        RefreshNeighbors(cell);
        return true;
    }

    public bool ClearOverTile(Vector3Int cell)
    {
        if (overGroundTilemap == null) return false;
        if (overGroundTilemap.GetTile(cell) == null) return false;
        overGroundTilemap.SetTile(cell, null);
        RefreshNeighbors(cell);
        return true;
    }

    public bool ConvertDirtToSoil(Vector3Int cell)
    {
        if (overGroundTilemap == null) return false;
        if (!IsDirtCell(cell)) return false;
        if (soilDryTile != null)
            overGroundTilemap.SetTile(cell, soilDryTile);
        else
            overGroundTilemap.SetTile(cell, soilTile);
        RefreshNeighbors(cell);
        return true;
    }

    public bool ConvertSoilToDirt(Vector3Int cell)
    {
        if (overGroundTilemap == null) return false;
        if (!IsSoilCell(cell)) return false;
        overGroundTilemap.SetTile(cell, dirtTile);
        RefreshNeighbors(cell);
        return true;
    }

    public void RefreshNeighbors(Vector3Int cell)
    {
        RefreshAroundSingleMap(groundTilemap, cell);
        RefreshAroundSingleMap(overGroundTilemap, cell);
    }

    private void RefreshAroundSingleMap(Tilemap map, Vector3Int c)
    {
        if (map == null) return;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                map.RefreshTile(new Vector3Int(c.x + dx, c.y + dy, c.z));
            }
        }
    }
}
