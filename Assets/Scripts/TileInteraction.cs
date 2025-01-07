using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class TileInteraction : MonoBehaviour
{
    [SerializeField] private ItemDropper itemDropper;

    [SerializeField] private Grid grid;
    [SerializeField] private List<Tilemap> tilemaps; // Liste pour tous les Tilemaps
    [SerializeField] private GameObject tileHighlight;

    [SerializeField] private DayNightCycle dayNightCycle;

    [SerializeField] private RuleTile dirtTile;
    [SerializeField] private RuleTile grassTile;
    [SerializeField] private RuleTile farmlandTile;
    [SerializeField] private RuleTile wateredFarmlandTile;

    [SerializeField] private HotbarController hotbarController; // Référence au contrôleur de la hotbar
    [SerializeField] private Inventory inventory; // Référence à l'inventaire


    private void Start()
    {
        if (tileHighlight == null)
        {
            Debug.LogError("TileHighlight n'est pas assigné dans l'inspecteur !");
            return;
        }

        // Désactiver le highlight au départ
        tileHighlight.SetActive(false);

        // Trier les Tilemaps par Order in Layer (de plus haut au plus bas)
        tilemaps = GetAllTilemaps(grid.transform);
        tilemaps = tilemaps.OrderByDescending(t => t.GetComponent<TilemapRenderer>().sortingOrder).ToList();
    }

    private void Update()
    {
        // Gérer le survol avec la souris
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemaps[0].WorldToCell(mouseWorldPosition);

        // Activer et positionner le highlight
        tileHighlight.SetActive(true);
        tileHighlight.transform.position = tilemaps[0].CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0f);

        // Gérer les clics
        if (Input.GetMouseButtonDown(0))
        {
            HandleTileClick(cellPosition);
        }

        if (dayNightCycle.GetCurrentHour() == 5 && dayNightCycle.GetCurrentMins() == 50) ClearWateredFarmland();
    }

    private void HandleTileClick(Vector3Int cellPosition)
    {
        // Parcourir les Tilemaps par priorité (du plus haut au plus bas en fonction de l'ordre dans le layer)
        foreach (Tilemap tilemap in tilemaps)
        {
            TileBase tile = tilemap.GetTile(cellPosition);
            if (tile != null)
            {
                Debug.Log($"Clicked on tile {tile.name} at {cellPosition} in Tilemap {tilemap.name}");
                HandleTileInteraction(cellPosition, tilemap, tile);
                return; // Arrêter après avoir trouvé une tuile valide
            }
        }

        Debug.Log("Aucune tuile valide n'a été cliquée.");
    }

    private void HandleTileInteraction(Vector3Int cellPosition, Tilemap tilemap, TileBase tile)
    {
        Tilemap collisionTilemap = tilemaps.Find(tm => tm.name == "CollisionTilemap");
        Tilemap grassTilemap = tilemaps.Find(tm => tm.name == "GrassTilemap");
        Tilemap dirtTilemap = tilemaps.Find(tm => tm.name == "DirtTilemap");
        Tilemap farmlandTilemap = tilemaps.Find(tm => tm.name == "FarmlandTilemap");
        Tilemap wateredFarmlandTilemap = tilemaps.Find(tm => tm.name == "WateredFarmlandTilemap");

        // Obtenez l'item actif dans la hotbar
        int currentSlot = hotbarController.GetCurrentSlot();
        ItemStack activeItemStack = inventory.GetItemInSlot(currentSlot); // Méthode dans Inventory pour récupérer l'item d'un slot
        string activeItemName = activeItemStack?.itemData?.itemName ?? string.Empty; // Nom de l'item actif ou chaîne vide

        switch (activeItemName)
        {
            case "Shovel":
                if (tilemap.name == "GrassTilemap")
                {
                    Debug.Log("Transforme l'herbe en terre avec la pelle");
                    dirtTilemap.SetTile(cellPosition, dirtTile);
                }
                else if (tilemap.name == "DirtTilemap" || tilemap.name == "FarmlandTilemap" || tilemap.name == "WateredFarmlandTilemap")
                {
                    Debug.Log("Transforme la terre en herbe avec la pelle");
                    dirtTilemap.SetTile(cellPosition, null);
                    farmlandTilemap.SetTile(cellPosition, null);
                    wateredFarmlandTilemap.SetTile(cellPosition, null);
                }
                break;

            case "Hoe":
                if (tilemap.name == "DirtTilemap")
                {
                    Debug.Log("Transforme la terre en farmland avec la houe");
                    farmlandTilemap.SetTile(cellPosition, farmlandTile);
                }
                else if (tilemap.name == "FarmlandTilemap")
                {
                    Debug.Log("Transforme le farmland en terre avec la houe");
                    farmlandTilemap.SetTile(cellPosition, null);
                }
                break;

            case "Watering Can":
                if (tilemap.name == "FarmlandTilemap")
                {
                    Debug.Log("Arrose le farmland avec l'arrosoir");
                    wateredFarmlandTilemap.SetTile(cellPosition, wateredFarmlandTile);
                }
                else if (tilemap.name == "WateredFarmlandTilemap")
                {
                    Debug.Log("Supprime l'arrosage du farmland avec l'arrosoir");
                    wateredFarmlandTilemap.SetTile(cellPosition, null);
                }
                break;

            case "Axe":
                if (tilemap.name == "CollisionTilemap" && tile.name == "tree")
                {
                    Debug.Log("Coupe un arbre avec la hache");
                    collisionTilemap.SetTile(cellPosition, null);
                    itemDropper.DropItems(tile, cellPosition);
                }
                break;

            case "Pickaxe":
                if (tilemap.name == "CollisionTilemap" && tile.name == "stone")
                {
                    Debug.Log("Mine une pierre avec la pioche");
                    collisionTilemap.SetTile(cellPosition, null);
                    itemDropper.DropItems(tile, cellPosition);
                }
                break;

            default:
                Debug.Log("Aucun outil valide n'est actif pour interagir avec cette tuile.");
                break;
        }
    }



    private void ChangeTile(Vector3Int cellPosition, TileBase newTile, Tilemap targetTilemap)
    {
        targetTilemap.SetTile(cellPosition, newTile);
    }

    private List<Tilemap> GetAllTilemaps(Transform parent)
    {
        List<Tilemap> tilemaps = new List<Tilemap>();

        // Vérifier si l'objet parent a une Tilemap
        Tilemap tilemap = parent.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            tilemaps.Add(tilemap);
        }

        // Récursivement parcourir tous les enfants
        foreach (Transform child in parent)
        {
            tilemaps.AddRange(GetAllTilemaps(child));
        }

        return tilemaps;
    }

    private void ClearWateredFarmland()
    {
        Tilemap wateredFarmlandTilemap = tilemaps.Find(tm => tm.name == "WateredFarmlandTilemap");
        BoundsInt bounds = wateredFarmlandTilemap.cellBounds;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (wateredFarmlandTilemap.GetTile(position) != null)
            {
                wateredFarmlandTilemap.SetTile(position, null);
            }
        }
    }
}
