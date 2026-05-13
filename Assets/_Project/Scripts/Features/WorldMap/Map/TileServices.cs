using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class DropService
{
    public sealed class DropGroup
    {
        public readonly List<TileBase> Tiles;
        public readonly List<Entry> Drops;
        public readonly ItemData RequiredToolItem;
        public readonly ToolKind RequiredToolKind;

        public DropGroup(List<TileBase> tiles, List<Entry> drops, ItemData requiredToolItem, ToolKind requiredToolKind)
        {
            Tiles = tiles;
            Drops = drops;
            RequiredToolItem = requiredToolItem;
            RequiredToolKind = requiredToolKind;
        }
    }

    public sealed class Entry
    {
        public readonly ItemData Item;
        public readonly int MinQuantity;
        public readonly int MaxQuantity;
        public readonly float Chance;

        public Entry(ItemData item, int minQuantity, int maxQuantity, float chance)
        {
            Item = item;
            MinQuantity = minQuantity;
            MaxQuantity = maxQuantity;
            Chance = chance;
        }
    }

    private readonly Dictionary<TileBase, DropGroup> lookup;

    public DropService(Dictionary<TileBase, DropGroup> lookup)
    {
        this.lookup = lookup ?? new Dictionary<TileBase, DropGroup>();
    }

    public bool TryGetGroup(TileBase tile, out DropGroup group)
    {
        if (tile == null)
        {
            group = null;
            return false;
        }
        return lookup.TryGetValue(tile, out group);
    }

    public void SpawnConfiguredDropsForTile(TileBase tileType, Vector3 worldPosition, System.Action<ItemData, Vector3> spawn)
    {
        if (tileType == null || spawn == null) return;
        if (!lookup.TryGetValue(tileType, out var group) || group == null || group.Drops == null || group.Drops.Count == 0) return;

        for (int i = 0; i < group.Drops.Count; i++)
        {
            var entry = group.Drops[i];
            if (entry == null || entry.Item == null) continue;
            if (entry.Chance < 1f && Random.value > Mathf.Clamp01(entry.Chance)) continue;

            int minQ = Mathf.Max(0, entry.MinQuantity);
            int maxQ = Mathf.Max(minQ, entry.MaxQuantity);
            int count = Random.Range(minQ, maxQ + 1);
            for (int k = 0; k < count; k++)
            {
                spawn(entry.Item, worldPosition);
            }
        }
    }
}

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

