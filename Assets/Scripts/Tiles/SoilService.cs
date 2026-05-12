using System.Collections.Generic;
using UnityEngine.Tilemaps;

public sealed class SoilService
{
    private readonly SoilWorld soilWorld;

    public SoilService()
    {
        soilWorld = new SoilWorld();
    }

    public void BootstrapFromOverTilemap(Tilemap overGrassTilemap, System.Func<TileBase, bool> isSoilOrWet, System.Func<TileBase, bool> isWet)
    {
        if (overGrassTilemap == null || isSoilOrWet == null || isWet == null) return;

        overGrassTilemap.CompressBounds();
        BoundsInt bounds = overGrassTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = overGrassTilemap.GetTile(cell);
                if (tile == null) continue;

                if (!isSoilOrWet(tile)) continue;
                soilWorld.RegisterSoil(new GridPos(cell.x, cell.y), isWet(tile));
            }
        }
    }

    public List<GridPos> SnapshotSoils() => soilWorld.SnapshotSoils();

    public List<GridPos> SnapshotWetSoils() => soilWorld.SnapshotWetSoils();

    public void RegisterSoil(GridPos pos, bool isWet) => soilWorld.RegisterSoil(pos, isWet);

    public void Unregister(GridPos pos) => soilWorld.Unregister(pos);

    public void MarkWet(GridPos pos) => soilWorld.MarkWet(pos);

    public void MarkDry(GridPos pos) => soilWorld.MarkDry(pos);
}

