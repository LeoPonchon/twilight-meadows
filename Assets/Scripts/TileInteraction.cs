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
    [SerializeField] private TileBase turnipSeedTile;

    [SerializeField] private HotbarController hotbarController; // Référence au contrôleur de la hotbar
    [SerializeField] private Inventory inventory; // Référence à l'inventaire
    private string lastSeason = ""; // Pour stocker la dernière saison active.

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

        if (dayNightCycle.GetCurrentHour() == 5 && dayNightCycle.GetCurrentMins() == 50) {
            UpdateSeeds();
            ClearWateredFarmland();
        }

        string currentSeason = dayNightCycle.GetCurrentSeason();
        if (currentSeason != lastSeason)
        {
            lastSeason = currentSeason;
            ClearAllPlants();
        }
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
        Tilemap seedTilemap = tilemaps.Find(tm => tm.name == "SeedTilemap");

        // Obtenez l'item actif dans la hotbar
        int currentSlot = hotbarController.GetCurrentSlot();
        ItemStack activeItemStack = inventory.GetItemInSlot(currentSlot); // Méthode dans Inventory pour récupérer l'item d'un slot
        string activeItemName = activeItemStack?.itemData?.itemName ?? string.Empty; // Nom de l'item actif ou chaîne vide
        Debug.Log(activeItemName);
        switch (activeItemName)
        {
            case "Shovel":
                if (tilemap.name == "GrassTilemap")
                {
                    Debug.Log("Transforme l'herbe en terre avec la pelle");
                    dirtTilemap.SetTile(cellPosition, dirtTile);
                }
                else if (tilemap.name == "DirtTilemap" || tilemap.name == "FarmlandTilemap" || tilemap.name == "WateredFarmlandTilemap" || tilemap.name == "SeedTilemap")
                {
                    Debug.Log("Transforme la terre en herbe avec la pelle");
                    dirtTilemap.SetTile(cellPosition, null);
                    farmlandTilemap.SetTile(cellPosition, null);
                    wateredFarmlandTilemap.SetTile(cellPosition, null);
                    seedTilemap.SetTile(cellPosition, null);
                }
                break;

            case "Hoe":
                if (tilemap.name == "DirtTilemap" || tilemap.name == "SeedTilemap")
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
                if (tilemap.name == "FarmlandTilemap" || tilemap.name == "SeedTilemap")
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

            case "Turnip Seeds":
                if (tilemap.name == "FarmlandTilemap" || tilemap.name == "WateredFarmlandTilemap")
                {
                    Debug.Log("Plante des graines de navet");
                    seedTilemap.SetTile(cellPosition, turnipSeedTile);
                }
                break;

            case "Axe":
                if (tilemap.name == "CollisionTilemap" && tile.name.Contains("tree"))
                {
                    Debug.Log("Coupe un arbre avec la hache");
                    collisionTilemap.SetTile(cellPosition, null);
                    itemDropper.DropItems(tile, cellPosition);
                }
                if (seedTilemap)
                {
                    Debug.Log("Ramasse les fruits peu importe l'objet");
                    seedTilemap.SetTile(cellPosition, null);
                    itemDropper.DropItems(tile, cellPosition);
                }
                break;

            case "Pickaxe":
                if (tilemap.name == "CollisionTilemap" && tile.name.Contains("rock"))
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

    private int lastGrowthDay = -1; // Variable pour suivre le dernier jour global.
    private Dictionary<Vector3Int, int> plantGrowthDays = new Dictionary<Vector3Int, int>(); // Dernier jour de croissance pour chaque plante.

    private void UpdateSeeds()
    {
        int currentDay = dayNightCycle.GetCurrentDay(); // Obtenir le jour actuel.

        if (currentDay == lastGrowthDay)
        {
            Debug.Log("Les plantes ont déjà été traitées aujourd'hui.");
            return; // Ne pas traiter à nouveau si la méthode a déjà été appelée pour le jour actuel.
        }

        lastGrowthDay = currentDay; // Mettre à jour le dernier jour traité globalement.

        Tilemap seedTilemap = tilemaps.Find(tm => tm.name == "SeedTilemap");
        Tilemap farmlandTilemap = tilemaps.Find(tm => tm.name == "FarmlandTilemap");
        Tilemap wateredFarmlandTilemap = tilemaps.Find(tm => tm.name == "WateredFarmlandTilemap");

        if (seedTilemap == null || farmlandTilemap == null || wateredFarmlandTilemap == null)
        {
            Debug.LogError("Une des Tilemaps nécessaires est introuvable !");
            return;
        }

        BoundsInt bounds = seedTilemap.cellBounds;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase tile = seedTilemap.GetTile(position);
            if (tile != null)
            {
                // Vérifier si la plante suit le format "nom_seed_n"
                string tileName = tile.name;
                if (tileName.Contains("_seed_"))
                {
                    string[] parts = tileName.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[parts.Length - 1], out int growthStage))
                    {
                        // Vérifier la fréquence autorisée en fonction du type de terrain.
                        int lastGrowthDayForPlant = plantGrowthDays.ContainsKey(position) ? plantGrowthDays[position] : -1;

                        // Déterminer la fréquence de croissance en fonction du terrain.
                        int growthFrequency = 2; // Par défaut, croissance tous les deux jours.
                        if (wateredFarmlandTilemap.GetTile(position) != null)
                        {
                            growthFrequency = 1; // Croissance quotidienne pour farmland arrosé.
                        }
                        else if (farmlandTilemap.GetTile(position) == null)
                        {
                            // Si pas sur une farmland ou une wateredFarmland, pas de croissance.
                            Debug.Log($"Plante à {position} ignorée, pas sur farmland.");
                            continue;
                        }

                        // Vérifier si la plante peut grandir aujourd'hui.
                        if (currentDay - lastGrowthDayForPlant < growthFrequency)
                        {
                            Debug.Log($"Plante à {position} ne peut pas grandir (dernier jour de croissance : {lastGrowthDayForPlant}).");
                            continue;
                        }

                        // Mettre à jour la plante au prochain stade de croissance.
                        int nextGrowthStage = growthStage + 1;
                        string nextTileName = string.Join("_", parts.Take(parts.Length - 1)) + "_" + nextGrowthStage;

                        TileBase nextTile = Resources.Load<TileBase>("Tiles/" + nextTileName);
                        if (nextTile != null)
                        {
                            seedTilemap.SetTile(position, nextTile);
                            plantGrowthDays[position] = currentDay; // Mettre à jour le jour de croissance de cette plante.
                            Debug.Log($"Plante à {position} a grandi au stade {nextGrowthStage}.");
                        }
                        else
                        {
                            // Aucun sprite suivant, la plante a atteint son stade final.
                            Debug.Log($"Plante à {position} a atteint son stade final ({tileName}).");
                        }
                    }
                }
            }
        }
    }

    private void ClearAllPlants()
    {
        Tilemap seedTilemap = tilemaps.Find(tm => tm.name == "SeedTilemap");
        if (seedTilemap == null)
        {
            Debug.LogError("SeedTilemap introuvable !");
            return;
        }

        BoundsInt bounds = seedTilemap.cellBounds;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (seedTilemap.GetTile(position) != null)
            {
                seedTilemap.SetTile(position, null);
            }
        }

        // Réinitialiser les jours de croissance pour chaque plante
        plantGrowthDays.Clear();

        Debug.Log("Tous les plants ont été supprimés en raison du changement de saison.");
    }



}
