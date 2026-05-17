using System.Collections.Generic;
using UnityEngine;

public sealed class WorldMapSaveService : MonoBehaviour, IWorldSaveService
{
    [Header("References")]
    [SerializeField] private TileInteractionController tileInteractionController;
    [SerializeField] private CollectiblesSpawnerController collectiblesSpawner;

    [Header("Seed Catalog")]
    [SerializeField] private List<SeedData> seeds = new List<SeedData>();

    private Dictionary<string, SeedData> byName;

    private void Awake()
    {
        if (tileInteractionController == null)
        {
            tileInteractionController = GetComponent<TileInteractionController>();
        }

        if (collectiblesSpawner == null)
        {
            collectiblesSpawner = FindObjectOfType<CollectiblesSpawnerController>(includeInactive: true);
        }
    }

    private void EnsureIndex()
    {
        if (byName != null) return;

        byName = new Dictionary<string, SeedData>();
        for (int i = 0; i < seeds.Count; i++)
        {
            var s = seeds[i];
            if (s == null) continue;
            if (string.IsNullOrWhiteSpace(s.itemName)) continue;
            byName[s.itemName] = s;
        }
    }

    public WorldStateSaveData CaptureWorld()
    {
        if (tileInteractionController == null) return null;
        var data = tileInteractionController.CaptureWorldState();
        if (data == null) return null;

        CaptureCollectibles(data);
        return data;
    }

    public void ApplyWorld(WorldStateSaveData world)
    {
        if (tileInteractionController == null) return;

        EnsureIndex();
        tileInteractionController.ApplyWorldState(world, ResolveSeed);
        ApplyCollectibles(world);
    }

    private void CaptureCollectibles(WorldStateSaveData data)
    {
        if (data == null) return;
        if (collectiblesSpawner == null)
        {
            Debug.LogWarning("WorldMapSaveService: Missing CollectiblesSpawnerController, rocks/trees won't be saved.", this);
            return;
        }

        var rocksTilemap = GetPrivateFieldTilemap(collectiblesSpawner, "rocksTilemap");
        var treesTilemap = GetPrivateFieldTilemap(collectiblesSpawner, "treesTilemap");
        if (rocksTilemap == null) Debug.LogWarning("WorldMapSaveService: CollectiblesSpawnerController.rocksTilemap is null.", this);
        if (treesTilemap == null) Debug.LogWarning("WorldMapSaveService: CollectiblesSpawnerController.treesTilemap is null.", this);

        CaptureTilemapTiles(rocksTilemap, data.rocks);
        CaptureTilemapTiles(treesTilemap, data.trees);
    }

    private void ApplyCollectibles(WorldStateSaveData data)
    {
        if (data == null) return;
        if (collectiblesSpawner == null) return;

        var rocksTilemap = GetPrivateFieldTilemap(collectiblesSpawner, "rocksTilemap");
        var treesTilemap = GetPrivateFieldTilemap(collectiblesSpawner, "treesTilemap");

        var rockTiles = GetPrivateFieldTileArray(collectiblesSpawner, "rockTiles");
        var treeTiles = GetPrivateFieldTileArray(collectiblesSpawner, "treeTiles");

        if (rocksTilemap == null || treesTilemap == null)
        {
            Debug.LogWarning("WorldMapSaveService: Missing rocks/trees tilemaps, cannot restore collectibles.", this);
        }

        ApplyTilemapTiles(rocksTilemap, data.rocks, rockTiles);
        ApplyTilemapTiles(treesTilemap, data.trees, treeTiles);
    }

    // Reflection helpers keep this adapter non-invasive (no need to rewrite existing spawner public API).
    private static UnityEngine.Tilemaps.Tilemap GetPrivateFieldTilemap(object obj, string fieldName)
    {
        if (obj == null) return null;
        var f = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return f != null ? f.GetValue(obj) as UnityEngine.Tilemaps.Tilemap : null;
    }

    private static UnityEngine.Tilemaps.TileBase[] GetPrivateFieldTileArray(object obj, string fieldName)
    {
        if (obj == null) return null;
        var f = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return f != null ? f.GetValue(obj) as UnityEngine.Tilemaps.TileBase[] : null;
    }

    private static void CaptureTilemapTiles(UnityEngine.Tilemaps.Tilemap tilemap, System.Collections.Generic.List<WorldStateSaveData.PlacedTile> outList)
    {
        if (tilemap == null || outList == null) return;
        outList.Clear();

        tilemap.CompressBounds();
        var bounds = tilemap.cellBounds;
        if (bounds.size.x <= 0 || bounds.size.y <= 0) return;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                var tile = tilemap.GetTile(cell);
                if (tile == null) continue;
                outList.Add(new WorldStateSaveData.PlacedTile { x = x, y = y, tileName = tile.name });
            }
        }
    }

    private static void ApplyTilemapTiles(
        UnityEngine.Tilemaps.Tilemap tilemap,
        System.Collections.Generic.List<WorldStateSaveData.PlacedTile> tiles,
        UnityEngine.Tilemaps.TileBase[] catalog)
    {
        if (tilemap == null || tiles == null) return;
        tilemap.ClearAllTiles();
        if (catalog == null || catalog.Length == 0) return;

        for (int i = 0; i < tiles.Count; i++)
        {
            var t = tiles[i];
            if (string.IsNullOrWhiteSpace(t.tileName)) continue;

            UnityEngine.Tilemaps.TileBase found = null;
            for (int k = 0; k < catalog.Length; k++)
            {
                if (catalog[k] != null && catalog[k].name == t.tileName)
                {
                    found = catalog[k];
                    break;
                }
            }
            if (found == null) continue;
            tilemap.SetTile(new Vector3Int(t.x, t.y, 0), found);
        }
    }

    private SeedData ResolveSeed(string seedName)
    {
        if (string.IsNullOrWhiteSpace(seedName)) return null;

        EnsureIndex();
        if (byName != null && byName.TryGetValue(seedName, out var seed)) return seed;

        // Best-effort: try to find by name in scene (slow, but keeps load working in dev).
        // This will only work if seeds are referenced somewhere (not necessarily in Resources).
        for (int i = 0; i < seeds.Count; i++)
        {
            var s = seeds[i];
            if (s != null && s.itemName == seedName) return s;
        }

        return null;
    }
}
