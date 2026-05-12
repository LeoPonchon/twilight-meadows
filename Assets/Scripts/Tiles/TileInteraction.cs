using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TileInteraction : MonoBehaviour
{
	[Header("Références Système")]
	[SerializeField] private GameObject tilemapsRoot;
	[SerializeField] private Inventory playerInventory;
	[SerializeField] private InventoryUI inventoryUI;
	[SerializeField] private SpriteRenderer playerSprite; // pour caler l'ordre d'affichage des drops
	[SerializeField] private TimeManager timeManager;
	[SerializeField] private WeatherManager weatherManager;
	[SerializeField] private GameObject collectiblePrefab; // Prefab pour les objets droppés
	[SerializeField] private SceneContext sceneContext;
	[SerializeField] private TilemapRegistry tilemapRegistry;

	[Header("Sol / Autotiling (Grass + Dirt/Farmland)")]
	[Tooltip("Tilemap Grass utilisée pour identifier les cases cliquées")]
	[SerializeField] private Tilemap grassTilemap;
	[Tooltip("Tilemap Dirt/Farmland sur laquelle on pose/enlève la terre ou la farmland")]
	[SerializeField] private Tilemap overGrassTilemap;
	[Tooltip("Tilemap pour les cultures")]
	[SerializeField] private Tilemap cropsTilemap;
	[Tooltip("Tilemap pour le foliage (hautes herbes)")]
	[SerializeField] private Tilemap foliageTilemap;

	[Header("Tiles de Base")]
	[Tooltip("Tuile d'herbe de référence sur la tilemap Grass")]
	[SerializeField] private TileBase grassTile;
	[Tooltip("Tuile de terre (dirt) sur OverGrass")]
	[SerializeField] private TileBase dirtTile;
	[Tooltip("Tuile de soil/farmland sur OverGrass")]
	[SerializeField] private TileBase soilTile;
	[Tooltip("Tuile de soil mouillé sur OverGrass")]
	[SerializeField] private TileBase wetSoilTile;




	[System.Serializable]
	public class DropEntry
	{
		public ItemData itemData;
		public int minQuantity = 1;
		public int maxQuantity = 1;
		[Range(0f,1f)] public float dropChance = 1f; // probabilité par entrée
	}

	[System.Serializable]
	public class TileDropGroup
	{
		public List<TileBase> tiles = new List<TileBase>();
		public List<DropEntry> drops = new List<DropEntry>();
		[Tooltip("Outil requis (ItemData). Laisser vide si on utilise toolKind")]
		public ItemData requiredToolItem;
		[Tooltip("Type d'outil requis (enum). Utilisé si requiredToolItem est vide.")]
		public ToolKind requiredToolKind = ToolKind.None;
	}

	[Header("Configuration des Drops")]
	[SerializeField] private List<TileDropGroup> dropGroups = new List<TileDropGroup>();

	private FarmingWorld farmingWorld;
	private SoilWorld soilWorld;
	private Dictionary<Vector3Int, PlantedCrop> plantedCrops = new Dictionary<Vector3Int, PlantedCrop>();
	private int lastFarmingTickDay = -1;
	private Dictionary<TileBase, TileDropGroup> dropLookup;
	private List<Tilemap> targetTilemaps;

	private void Awake()
	{
		farmingWorld = new FarmingWorld();
		soilWorld = new SoilWorld();
		InitializeDropLookup();
		InitializeTargetTilemaps();
		InitializePlayerSprite();
		InitializeTimeManager();
		BootstrapSoilWorld();
	}

	private void BootstrapSoilWorld()
	{
		if (soilWorld == null || overGrassTilemap == null) return;

		overGrassTilemap.CompressBounds();
		BoundsInt bounds = overGrassTilemap.cellBounds;
		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				Vector3Int cell = new Vector3Int(x, y, 0);
				TileBase tile = overGrassTilemap.GetTile(cell);
				if (tile == null) continue;

				bool isSoil = IsSoil(tile);
				bool isWet = IsWetSoil(tile);
				if (!isSoil && !isWet) continue;

				soilWorld.RegisterSoil(new GridPos(cell.x, cell.y), isWet);
			}
		}
	}

	private void InitializeDropLookup()
	{
		dropLookup = new Dictionary<TileBase, TileDropGroup>();
		for (int i = 0; i < dropGroups.Count; i++)
		{
			var group = dropGroups[i];
			if (group == null || group.tiles == null || group.tiles.Count == 0 || group.drops == null || group.drops.Count == 0)
				continue;
			for (int t = 0; t < group.tiles.Count; t++)
			{
				var tile = group.tiles[t];
				if (tile == null) continue;
				dropLookup[tile] = group;
			}
		}
	}

	private void InitializeTargetTilemaps()
	{
		// Collecter les tilemaps cibles sous la racine
		targetTilemaps = new List<Tilemap>();

		if (tilemapsRoot == null && tilemapRegistry != null)
		{
			tilemapsRoot = tilemapRegistry.TilemapsRoot;
		}

		if (tilemapRegistry != null && tilemapRegistry.Tilemaps != null && tilemapRegistry.Tilemaps.Length > 0)
		{
			targetTilemaps.AddRange(tilemapRegistry.Tilemaps);
			return;
		}
		if (tilemapsRoot != null)
		{
			tilemapsRoot.GetComponentsInChildren(true, targetTilemaps);
		}
		else
		{
			// Fallback: toutes les tilemaps de la scène
			Debug.LogError("TileInteraction: tilemapsRoot or TilemapRegistry is required (no more scene-wide search fallback).", this);
		}
	}

	private void InitializePlayerSprite()
	{
		if (playerSprite == null)
		{
			var player = sceneContext != null ? sceneContext.Get<TopDownMovement>() : FindObjectOfType<TopDownMovement>();
			if (player != null)
			{
				playerSprite = player.GetComponent<SpriteRenderer>();
			}
		}
	}

	private void InitializeTimeManager()
	{
		if (sceneContext == null)
		{
			sceneContext = FindObjectOfType<SceneContext>();
		}

		if (timeManager == null)
		{
			timeManager = sceneContext != null ? sceneContext.Get<TimeManager>() : FindObjectOfType<TimeManager>();
		}
		
		if (weatherManager == null)
		{
			weatherManager = sceneContext != null ? sceneContext.Get<WeatherManager>() : FindObjectOfType<WeatherManager>();
		}
		
		// S'abonner aux changements de saison et de jour
		if (timeManager != null)
		{
			timeManager.OnSeasonStarted += HandleSeasonChanged;
			timeManager.OnDayChanged += HandleDayChanged;
		}
		
		// S'abonner aux changements de météo pour arroser immédiatement
		if (weatherManager != null)
		{
			weatherManager.OnWeatherChanged += HandleWeatherChanged;
		}
	}
	
	private void OnDestroy()
	{
		// Se désabonner des événements
		if (timeManager != null)
		{
			timeManager.OnSeasonStarted -= HandleSeasonChanged;
			timeManager.OnDayChanged -= HandleDayChanged;
		}
		
		if (weatherManager != null)
		{
			weatherManager.OnWeatherChanged -= HandleWeatherChanged;
		}
	}
	
	private void HandleSeasonChanged(int seasonId, string seasonName)
	{
		// Vérifier toutes les cultures et les faire flétrir si elles ne sont pas adaptées à la saison
		foreach (var kvp in plantedCrops)
		{
			Vector3Int cell = kvp.Key;
			PlantedCrop crop = kvp.Value;
			
			// Si la culture n'est pas adaptée à la saison actuelle, la faire flétrir immédiatement
			if ((int)crop.seedInstance.growthSeason != seasonId && !crop.isWithered)
			{
				crop.isWithered = true;
				UpdateCropSprite(cell, crop);
			}
		}
	}
	
	private void HandleWeatherChanged(WeatherType weather)
	{
		// Arroser immédiatement tous les sols quand il commence à pleuvoir
		if (weather == WeatherType.Rainy || weather == WeatherType.Stormy)
		{
			WaterAllSoils();
		}
		else
		{
			// Sécher tous les sols mouillés quand il arrête de pleuvoir
			DryAllWetSoils();
		}
	}
	
	private void HandleDayChanged(int day)
	{
		// Sécher tous les sols mouillés au changement de jour (fin de journée)
		DryAllWetSoils();
	}
	
	private void WaterAllSoils()
	{
		if (overGrassTilemap == null || timeManager == null) return;
		
		int currentDay = timeManager.GetCurrentDay();
		farmingWorld?.WaterAllCrops(currentDay);

		if (soilWorld != null)
		{
			var soils = soilWorld.SnapshotSoils();
			for (int i = 0; i < soils.Count; i++)
			{
				var p = soils[i];
				Vector3Int cell = new Vector3Int(p.X, p.Y, 0);
				TileBase tile = overGrassTilemap.GetTile(cell);
				if (!IsSoil(tile)) continue;

				overGrassTilemap.SetTile(cell, wetSoilTile);
				soilWorld.MarkWet(p);

				if (plantedCrops.TryGetValue(cell, out var crop) && crop != null)
				{
					crop.lastWateredDay = currentDay;
				}
			}
			return;
		}
		
		// Parcourir toutes les tiles de la tilemap
		BoundsInt bounds = overGrassTilemap.cellBounds;
		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				Vector3Int cell = new Vector3Int(x, y, 0);
				TileBase tile = overGrassTilemap.GetTile(cell);
				
				// Si c'est un sol normal, le transformer en sol mouillé
				if (IsSoil(tile))
				{
					overGrassTilemap.SetTile(cell, wetSoilTile);
					
					// Mettre à jour la date d'arrosage des cultures
					if (plantedCrops.ContainsKey(cell))
					{
						PlantedCrop crop = plantedCrops[cell];
						crop.lastWateredDay = currentDay;
					}
				}
			}
		}
	}
	
	private void DryAllWetSoils()
	{
		if (overGrassTilemap == null) return;

		if (soilWorld != null)
		{
			var wet = soilWorld.SnapshotWetSoils();
			for (int i = 0; i < wet.Count; i++)
			{
				var p = wet[i];
				Vector3Int cell = new Vector3Int(p.X, p.Y, 0);
				TileBase tile = overGrassTilemap.GetTile(cell);
				if (!IsWetSoil(tile)) continue;

				overGrassTilemap.SetTile(cell, soilTile);
				soilWorld.MarkDry(p);
			}
			return;
		}
		
		// Parcourir toutes les tiles de la tilemap
		BoundsInt bounds = overGrassTilemap.cellBounds;
		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				Vector3Int cell = new Vector3Int(x, y, 0);
				TileBase tile = overGrassTilemap.GetTile(cell);
				
				// Si c'est un sol mouillé, le transformer en sol normal
				if (IsWetSoil(tile))
				{
					overGrassTilemap.SetTile(cell, soilTile);
					soilWorld?.RegisterSoil(new GridPos(cell.x, cell.y), isWet: false);
				}
			}
		}
	}

	private void Update()
	{
		if (timeManager != null)
		{
			int day = timeManager.GetCurrentDay();
			if (day != lastFarmingTickDay)
			{
				lastFarmingTickDay = day;
				TickFarmingWorld(day);
			}
		}
		HandleMouseInput();
	}

	private void HandleMouseInput()
	{
		if (targetTilemaps == null || targetTilemaps.Count == 0 || !Input.GetMouseButtonDown(0))
			return;

		// Éviter les clics sur l'UI
		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
			return;

		Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		world.z = 0f;

		// Traiter les interactions par ordre de priorité
		if (TryHandlePlantingAction(world)) return;
		if (TryHandleWateringAction(world)) return;
		if (TryHandleSoilToolAction(world)) return;
		
		// Fallback: logique de casse et drop configurés
		HandleTileBreaking(world);
	}

	private void HandleTileBreaking(Vector3 world)
	{
		for (int i = 0; i < targetTilemaps.Count; i++)
		{
			var tm = targetTilemaps[i];
			if (tm == null) continue;
			Vector3Int cell = tm.WorldToCell(world);
			TileBase clickedTile = tm.GetTile(cell);
			if (clickedTile == null) continue;
			if (!dropLookup.ContainsKey(clickedTile)) continue;

			// Vérifier le bon outil selon le groupe ciblé
			var group = dropLookup[clickedTile];
			if (!IsToolSelected(group)) 
			{
				return;
			}
			BreakSingle(tm, cell);
			break; // n'agir que sur la première tilemap correspondante
		}
	}

	private ToolKind GetSelectedToolKind()
	{
		if (playerInventory == null || inventoryUI == null)
			return ToolKind.None;
		
		int slot = inventoryUI.GetCurrentHotbarSlot();
		ItemStack stack = playerInventory.GetItemInSlot(slot);
		if (stack == null || stack.itemData == null)
			return ToolKind.None;
		
		// Utiliser la factory pour déterminer le type d'outil
		return ItemInstanceFactory.GetToolKind(stack.itemData);
	}

	private ItemData GetSelectedItemData()
	{
		if (playerInventory == null || inventoryUI == null)
			return null;
		int slot = inventoryUI.GetCurrentHotbarSlot();
		ItemStack stack = playerInventory.GetItemInSlot(slot);
		return stack?.itemData;
	}

	private ItemStack GetSelectedItemStack()
	{
		if (playerInventory == null || inventoryUI == null)
			return null;
		int slot = inventoryUI.GetCurrentHotbarSlot();
		return playerInventory.GetItemInSlot(slot);
	}

	private void TickFarmingWorld(int currentDay)
	{
		if (farmingWorld == null || timeManager == null) return;
		if (farmingWorld.Crops.Count == 0) return;

		int currentSeasonId = timeManager.GetCurrentSeasonId();
		var updates = farmingWorld.TickDay(currentDay, currentSeasonId);
		for (int i = 0; i < updates.Count; i++)
		{
			var u = updates[i];
			var cell = new Vector3Int(u.Pos.X, u.Pos.Y, 0);
			if (!plantedCrops.TryGetValue(cell, out var crop) || crop == null) continue;
			if (!farmingWorld.TryGetCrop(u.Pos, out var state) || state == null) continue;

			crop.currentStage = state.CurrentStage;
			crop.isWithered = state.IsWithered;
			crop.lastWateredDay = state.LastWateredDay;
			crop.lastProductionDay = state.LastProductionDay;
			crop.hasFruits = state.HasFruits;

			UpdateCropSprite(cell, crop);
		}
	}

	private void UpdateCrops()
	{
		if (plantedCrops == null || plantedCrops.Count == 0)
			return;

		List<Vector3Int> cropsToRemove = new List<Vector3Int>();

		foreach (var kvp in plantedCrops)
		{
			Vector3Int cell = kvp.Key;
			PlantedCrop crop = kvp.Value;

			// Vérifier si la culture doit grandir
			if (ShouldGrow(crop))
			{
				crop.currentStage++;
				UpdateCropSprite(cell, crop);
			}

			// Pour les cultures pérennes, vérifier la production
			if (crop.isPerennial && IsMature(crop))
			{
				UpdatePerennialProduction(cell, crop);
			}

			// Vérifier si la culture doit flétrir (seulement pour la saison courante)
			if (ShouldWither(crop))
			{
				crop.isWithered = true;
				UpdateCropSprite(cell, crop);
			}
		}
	}

	private bool ShouldGrow(PlantedCrop crop)
	{
		if (timeManager == null || crop == null || crop.seedInstance == null || crop.seedInstance.growthSprites == null) return false;
		return FarmingCropLogic.ShouldGrow(
			crop.isWithered,
			crop.currentStage,
			crop.seedInstance.growthSprites.Length,
			timeManager.GetCurrentDay(),
			crop.dayPlanted);

		#if false
		if (crop.isWithered || crop.currentStage >= crop.seedInstance.growthSprites.Length - 1)
			return false;

		if (timeManager == null) return false;

		int currentDay = timeManager.GetCurrentDay();
		int daysSincePlanted = currentDay - crop.dayPlanted;
		
		// Chaque sprite = 1 jour, donc chaque étape = 1 jour
		// L'étape suivante est atteinte après (currentStage + 1) jours
		int requiredDays = crop.currentStage + 1;

		return daysSincePlanted >= requiredDays;
		#endif
	}

	private bool ShouldWither(PlantedCrop crop)
	{
		if (timeManager == null || crop == null || crop.seedInstance == null || crop.seedInstance.growthSprites == null) return false;
		int currentSeasonId = timeManager.GetCurrentSeasonId();
		bool isMature = FarmingCropLogic.IsMature(crop.currentStage, crop.seedInstance.growthSprites.Length);
		bool isInSeason = (int)crop.seedInstance.growthSeason == currentSeasonId;
		return FarmingCropLogic.ShouldWither(
			crop.isWithered,
			isInSeason,
			isMature,
			timeManager.GetCurrentDay(),
			crop.lastWateredDay,
			crop.seedInstance.witherTime);

		#if false
		if (crop.isWithered)
			return false;

		if (timeManager == null) return false;

		int currentDay = timeManager.GetCurrentDay();
		int currentSeasonId = timeManager.GetCurrentSeasonId();
		
		// Si la culture n'est pas dans sa saison, elle ne peut pas flétrir par manque d'eau
		// (elle flétrit automatiquement au changement de saison)
		if ((int)crop.seedInstance.growthSeason != currentSeasonId)
			return false;

		// Ne peut flétrir qu'après être devenue récoltable
		if (crop.currentStage < crop.seedInstance.growthSprites.Length - 1)
			return false;

		// Vérifier si la culture n'a pas été arrosée depuis trop longtemps
		int daysSinceLastWatered = currentDay - crop.lastWateredDay;
		return daysSinceLastWatered >= crop.seedInstance.witherTime;
		#endif
	}

	private bool IsMature(PlantedCrop crop)
	{
		// Une culture est mature quand elle atteint la dernière étape
		if (crop == null || crop.seedInstance == null || crop.seedInstance.growthSprites == null) return false;
		return FarmingCropLogic.IsMature(crop.currentStage, crop.seedInstance.growthSprites.Length);
	}

	private void UpdatePerennialProduction(Vector3Int cell, PlantedCrop crop)
	{
		if (crop == null || crop.seedInstance == null) return;
		if (FarmingCropLogic.TryUpdatePerennialProduction(
				timeManager != null ? timeManager.GetCurrentDay() : 0,
				crop.seedInstance.productionInterval,
				ref crop.lastProductionDay,
				ref crop.hasFruits))
		{
			UpdateCropSprite(cell, crop);
		}

		#if false
		if (timeManager == null) return;

		int currentDay = timeManager.GetCurrentDay();
		
		// Si c'est la première fois qu'on vérifie la production
		if (crop.lastProductionDay == 0)
		{
			crop.lastProductionDay = currentDay;
			crop.hasFruits = true;
			UpdateCropSprite(cell, crop);
			return;
		}

		// Vérifier si assez de temps s'est écoulé pour une nouvelle production
		int daysSinceLastProduction = currentDay - crop.lastProductionDay;
		if (daysSinceLastProduction >= crop.seedInstance.productionInterval && !crop.hasFruits)
		{
			crop.lastProductionDay = currentDay;
			crop.hasFruits = true;
			UpdateCropSprite(cell, crop);
		}
		#endif
	}

	private void UpdateCropSprite(Vector3Int cell, PlantedCrop crop)
	{
		if (cropsTilemap == null || crop.seedInstance.growthSprites == null)
			return;

		Sprite spriteToUse;
		
		// Si la culture est flétrie, utiliser le sprite flétri
		if (crop.isWithered && crop.seedInstance.witheredSprite != null)
		{
			spriteToUse = crop.seedInstance.witheredSprite;
		}
		// Pour les cultures pérennes avec fruits, utiliser le sprite avec fruits
		else if (crop.isPerennial && crop.hasFruits && crop.seedInstance.fruitSprite != null)
		{
			spriteToUse = crop.seedInstance.fruitSprite;
		}
		else
		{
			// Pour les cultures pérennes sans fruits, utiliser l'étape précédente (mature sans fruits)
			if (crop.isPerennial && !crop.hasFruits)
			{
				// Utiliser l'étape précédente (étape mature sans fruits)
				int spriteIndex = crop.currentStage - 1;
				if (spriteIndex >= 0 && spriteIndex < crop.seedInstance.growthSprites.Length)
				{
					spriteToUse = crop.seedInstance.growthSprites[spriteIndex];
				}
				else
				{
					spriteToUse = crop.seedInstance.growthSprites[crop.seedInstance.growthSprites.Length - 1];
				}
			}
			else
			{
				// Sprite normal selon l'étape
				if (crop.currentStage < crop.seedInstance.growthSprites.Length)
				{
					spriteToUse = crop.seedInstance.growthSprites[crop.currentStage];
				}
				else
				{
					return; // Index invalide
				}
			}
		}

		// Mettre à jour la tile sur la cropsTilemap
		TileBase cropTile = CreateCropTile(spriteToUse);
		cropsTilemap.SetTile(cell, cropTile);
	}

	private bool TryHandlePlantingAction(Vector3 world)
	{
		Vector3Int cell = grassTilemap.WorldToCell(world);
		
		// Vérifier s'il y a une culture à récolter à cette position
		if (plantedCrops.ContainsKey(cell))
		{
			PlantedCrop crop = plantedCrops[cell];
			if (CanHarvest(crop))
			{
				return TryHarvestCrop(cell, crop);
			}
		}
		
		// Vérifier si c'est une graine
		var selectedItemData = GetSelectedItemData();
		if (selectedItemData is SeedData seedData)
		{
			return TryPlantSeed(world, seedData);
		}
		
		return false;
	}

	private bool CanHarvest(PlantedCrop crop)
	{
		// Pour les cultures pérennes, peut récolter si elle a des fruits OU si elle est flétrie
		if (crop.isPerennial)
		{
			return crop.hasFruits || crop.isWithered;
		}
		
		// Pour les cultures normales, peut récolter si la culture est mûre (dernière étape) ou flétrie
		return crop.currentStage >= crop.seedInstance.growthSprites.Length - 1 || crop.isWithered;
	}

	private bool TryHarvestCrop(Vector3Int cell, PlantedCrop crop)
	{
		// Pour les cultures pérennes avec fruits, juste récolter les fruits
		if (crop.isPerennial && crop.hasFruits && !crop.isWithered)
		{
			// Dropper les produits de récolte
			if (crop.seedInstance.harvestProduct != null)
			{
				Vector3 dropPos = cropsTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
				for (int i = 0; i < crop.seedInstance.harvestQuantity; i++)
				{
					SpawnDrop(crop.seedInstance.harvestProduct, dropPos);
				}
			}

			// Retirer les fruits et revenir au sprite normal
			crop.hasFruits = false;
			UpdateCropSprite(cell, crop);
			return true;
		}

		// Pour toutes les autres cultures (normales ou pérennes flétries), récolter et supprimer
		// Supprimer la tile de la cropsTilemap
		if (cropsTilemap != null)
		{
			cropsTilemap.SetTile(cell, null);
		}

		// Dropper les produits de récolte (pas de récolte si flétrie)
		if (!crop.isWithered && crop.seedInstance.harvestProduct != null)
		{
			Vector3 dropPos = cropsTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
			for (int i = 0; i < crop.seedInstance.harvestQuantity; i++)
			{
				SpawnDrop(crop.seedInstance.harvestProduct, dropPos);
			}
		}

		// Supprimer la culture du dictionnaire
		plantedCrops.Remove(cell);

		return true;
	}

	private bool TryHandleWateringAction(Vector3 world)
	{
		ToolKind tool = GetSelectedToolKind();
		if (tool != ToolKind.WateringCan)
			return false;

		if (grassTilemap == null || overGrassTilemap == null)
			return false;

		Vector3Int cell = grassTilemap.WorldToCell(world);
		TileBase terrainTile = grassTilemap.GetTile(cell);
		if (terrainTile == null)
			return false;
		TileBase dirtAtCell = overGrassTilemap.GetTile(cell);
		bool isSoilAtCell = IsSoil(dirtAtCell);

		var selectedItemStack = GetSelectedItemStack();
		if (selectedItemStack != null && selectedItemStack.itemData is WateringCanData)
		{
			var wateringCanInstance = selectedItemStack.GetItemInstance() as WateringCanInstance;
			
			if (wateringCanInstance != null)
			{
				bool isWaterAtCell = IsWaterTile(terrainTile, wateringCanInstance) || IsWaterTile(dirtAtCell, wateringCanInstance);
				
				// Si on clique sur de l'eau, recharger l'arrosoir
				if (isWaterAtCell)
				{
				if (wateringCanInstance.currentCapacity < wateringCanInstance.maxCapacity)
				{
					wateringCanInstance.Refill();
					return true;
				}
				}
				// Si on clique sur du soil, arroser
				else if (isSoilAtCell)
				{
					if (wateringCanInstance.CanWater())
					{
						// Changer la tile de soil en soil mouillé
						overGrassTilemap.SetTile(cell, wetSoilTile);
						soilWorld?.MarkWet(new GridPos(cell.x, cell.y));
						wateringCanInstance.UseWater();
						
						if (plantedCrops.ContainsKey(cell))
						{
							PlantedCrop crop = plantedCrops[cell];
							crop.lastWateredDay = timeManager != null ? timeManager.GetCurrentDay() : 1;
						}
						
						return true;
					}
					else
					{
						return false;
					}
				}
			}
		}

		return false;
	}


	private bool TryHandleSoilToolAction(Vector3 world)
	{
		ToolKind tool = GetSelectedToolKind();
		if (tool == ToolKind.None)
		{
			return false;
		}

		if (grassTilemap == null || overGrassTilemap == null)
			return false;

		bool changed = false;

		Vector3Int cell = grassTilemap.WorldToCell(world);
		TileBase terrainTile = grassTilemap.GetTile(cell);
		if (terrainTile == null)
			return false;
		TileBase dirtAtCell = overGrassTilemap.GetTile(cell);
		bool isSoilAtCell = IsSoil(dirtAtCell);
		
		// Vérifier s'il y a une plantation sur cette tile
		bool hasCrop = plantedCrops.ContainsKey(cell);

		switch (tool)
		{
			case ToolKind.Shovel:
				// Pelle: Empêcher l'utilisation sur les plantations
				if (hasCrop)
				{
					return false; // Ne pas permettre d'utiliser la pelle sur une plantation
				}
				
				// Pelle:
				// - Si terrain = herbe et aucune dirt/farmland au-dessus → poser dirt
				// - Si soil présent ET arrosé → remettre herbe (supprimer crop et eau)
				// - Si dirt présent → remettre herbe
				if (IsGrass(terrainTile) && dirtAtCell == null)
				{
					overGrassTilemap.SetTile(cell, dirtTile);
					soilWorld?.Unregister(new GridPos(cell.x, cell.y));
					// Supprimer aussi la tile de foliage au même endroit si elle existe
					RemoveFoliageTileAtCell(cell);
					changed = true;
				}
				else if (isSoilAtCell || IsWetSoil(dirtAtCell))
				{
					// Si soil ou wet soil, on remet herbe
					overGrassTilemap.SetTile(cell, null);
					soilWorld?.Unregister(new GridPos(cell.x, cell.y));
					changed = true;
				}
				else if (IsDirt(dirtAtCell))
				{
					overGrassTilemap.SetTile(cell, null);
					soilWorld?.Unregister(new GridPos(cell.x, cell.y));
					changed = true;
				}
				break;

			case ToolKind.Hoe:
				// Hoe: Empêcher l'utilisation sur les plantations
				if (hasCrop)
				{
					return false; // Ne pas permettre d'utiliser la hoe sur une plantation
				}
				
				// Hoe:
				// - Si dirt présent → remplacer par soil (farmland)
				// - Si soil présent → remplacer par dirt
				// - Si wet soil présent → remplacer par dirt
				if (IsDirt(dirtAtCell))
				{
					overGrassTilemap.SetTile(cell, soilTile);
					changed = true;
				}
				else if (isSoilAtCell || IsWetSoil(dirtAtCell))
				{
					overGrassTilemap.SetTile(cell, dirtTile);
					changed = true;
				}
				break;
				
			case ToolKind.Pickaxe:
				// Pioche: Peut détruire les plantations
				if (hasCrop)
				{
					// Détruire la plantation
					DestroyCropAtCell(cell);
					changed = true;
				}
				else if (IsDirt(dirtAtCell) || isSoilAtCell || IsWetSoil(dirtAtCell))
				{
					// Retourner à l'herbe
					overGrassTilemap.SetTile(cell, null);
					changed = true;
				}
				break;
		}

		if (changed)
		{
			RefreshNeighborsForSoilLayers(cell);
		}

		return changed;
	}

	private void DestroyCropAtCell(Vector3Int cell)
	{
		if (!plantedCrops.ContainsKey(cell))
			return;

		// Supprimer la tile de la cropsTilemap
		if (cropsTilemap != null)
		{
			cropsTilemap.SetTile(cell, null);
		}

		// Supprimer la culture du dictionnaire
		plantedCrops.Remove(cell);
		farmingWorld?.Remove(new GridPos(cell.x, cell.y));
	}

	private bool TryPlantSeed(Vector3 world, SeedData seedData)
	{
		if (grassTilemap == null || overGrassTilemap == null || cropsTilemap == null)
			return false;

		Vector3Int cell = grassTilemap.WorldToCell(world);
		TileBase dirtAtCell = overGrassTilemap.GetTile(cell);
		bool isSoilAtCell = IsSoil(dirtAtCell);
		bool isWetSoilAtCell = IsWetSoil(dirtAtCell);

		// Vérifier qu'il n'y a pas déjà une culture à cette position
		if (plantedCrops.ContainsKey(cell))
		{
			return false;
		}

		// Planter seulement sur du soil ou wet soil
		if (isSoilAtCell || isWetSoilAtCell)
		{
			if (timeManager != null && seedData.CanPlant(timeManager.GetCurrentSeasonId()))
			{
				// Vérifier que les sprites de croissance sont définis
				if (seedData.growthSprites == null || seedData.growthSprites.Length == 0)
				{
					return false;
				}
				
				// Créer la culture plantée avec des données d'instance
				PlantedCrop crop = new PlantedCrop
				{
					originalSeedData = seedData,
					seedInstance = new SeedInstance(seedData),
					currentStage = 0, // Commence à l'étape graine
					dayPlanted = timeManager != null ? timeManager.GetCurrentDay() : 1,
					isWithered = false,
					isPerennial = seedData.isPerennial,
					lastProductionDay = 0,
					hasFruits = false,
					lastWateredDay = timeManager != null ? timeManager.GetCurrentDay() : 1
				};

				// Enregistrer la culture
				plantedCrops[cell] = crop;
				farmingWorld?.Plant(
					new GridPos(cell.x, cell.y),
					new CropBlueprint(
						growthStagesCount: crop.seedInstance.growthSprites != null ? crop.seedInstance.growthSprites.Length : 0,
						growthSeasonId: (int)crop.seedInstance.growthSeason,
						witherTimeDays: crop.seedInstance.witherTime,
						isPerennial: crop.isPerennial,
						productionIntervalDays: crop.seedInstance.productionInterval),
					currentDay: crop.dayPlanted);

				// Placer le sprite de culture (étape 0 = graine)
				PlaceCropSprite(cell, 0);

				// Consommer la graine
				playerInventory.RemoveItem(seedData, 1);
				return true;
			}
		}

		return false;
	}

	private void PlaceCropSprite(Vector3Int cell, int stage)
	{
		if (cropsTilemap == null) return;

		if (!plantedCrops.ContainsKey(cell)) return;

		PlantedCrop crop = plantedCrops[cell];
		if (crop.seedInstance.growthSprites == null || stage < 0 || stage >= crop.seedInstance.growthSprites.Length) return;

		TileBase cropTile = CreateCropTile(crop.seedInstance.growthSprites[stage]);
		cropsTilemap.SetTile(cell, cropTile);
	}

	private TileBase CreateCropTile(Sprite sprite)
	{
		GameObject tempGO = new GameObject("TempCropTile");
		SpriteRenderer sr = tempGO.AddComponent<SpriteRenderer>();
		sr.sprite = sprite;
		
		var tile = ScriptableObject.CreateInstance<Tile>();
		tile.sprite = sprite;
		
		DestroyImmediate(tempGO);
		
		return tile;
	}


	private bool IsGrass(TileBase tile) { return tile != null && grassTile != null && tile == grassTile; }
	private bool IsDirt(TileBase tile) { return tile != null && dirtTile != null && tile == dirtTile; }
	private bool IsSoil(TileBase tile) { return tile != null && soilTile != null && tile == soilTile; }
	private bool IsWetSoil(TileBase tile) { return tile != null && wetSoilTile != null && tile == wetSoilTile; }
	
	private bool IsWaterTile(TileBase tile, WateringCanInstance wateringCanInstance) 
	{ 
		if (tile == null || wateringCanInstance == null || wateringCanInstance.waterSourceTiles == null) return false;
		
		// Vérifier si la tile correspond à une source d'eau définie dans l'arrosoir
		foreach (TileBase waterSourceTile in wateringCanInstance.waterSourceTiles)
		{
			if (tile == waterSourceTile)
				return true;
		}
		return false;
	}

	private void RefreshNeighborsForSoilLayers(Vector3Int cell)
	{
		// Rafraîchir la cellule et ses 8 voisins sur grass + dirt pour un autotiling correct
		RefreshAroundSingleMap(grassTilemap, cell);
		RefreshAroundSingleMap(overGrassTilemap, cell);
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


	private bool IsToolSelected(TileDropGroup group)
	{
		if (playerInventory == null || inventoryUI == null)
			return false;

		int slot = inventoryUI.GetCurrentHotbarSlot();
		ItemStack stack = playerInventory.GetItemInSlot(slot);
		if (stack == null || stack.itemData == null)
			return false;

		ItemData data = stack.itemData;
		bool hasSpecificItem = group != null && group.requiredToolItem != null;
		bool hasKind = group != null && group.requiredToolKind != ToolKind.None;
		
		// Vérifier si l'item sélectionné est un outil
		bool isTool = IsTool(data);
		if (!isTool && (hasSpecificItem || hasKind))
			return false;

		// Si aucun outil spécifique/kind requis → n'importe quel outil suffit
		if (!hasSpecificItem && !hasKind)
			return isTool;

		// 1) Comparaison stricte par référence d'ItemData
		if (hasSpecificItem && data == group.requiredToolItem)
			return true;

		// 2) Si kind demandé, on compare via le type d'outil
		if (hasKind && GetToolKindFromItem(data) == group.requiredToolKind)
			return true;

		// 3) Fallback: si un item précis est fourni, accepter toute variante du même type d'outil
		if (hasSpecificItem && IsTool(group.requiredToolItem))
			return GetToolKindFromItem(data) == GetToolKindFromItem(group.requiredToolItem);

		return false;
	}

	private bool IsTool(ItemData item)
	{
		if (item == null) return false;
		
		return item is WateringCanData || 
		       item is ShovelData || 
		       item is AxeData || 
		       item is PickaxeData || 
		       item is SpearData || 
		       item is HoeData;
	}

	private ToolKind GetToolKindFromItem(ItemData item)
	{
		if (item == null) return ToolKind.None;
		
		if (item is WateringCanData) return ToolKind.WateringCan;
		if (item is ShovelData) return ToolKind.Shovel;
		if (item is AxeData) return ToolKind.Axe;
		if (item is PickaxeData) return ToolKind.Pickaxe;
		if (item is SpearData) return ToolKind.Spear;
		if (item is HoeData) return ToolKind.Hoe;
		
		return ToolKind.None;
	}



	private void BreakSingle(Tilemap tm, Vector3Int cell)
	{
		TileBase tile = tm.GetTile(cell);
		if (tile == null) return;
		Vector3 dropPos = tm.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
		tm.SetTile(cell, null);
		DropConfiguredItemsForTile(tile, dropPos);
	}

	private void DropConfiguredItemsForTile(TileBase tileType, Vector3 worldPosition)
	{
		if (tileType == null || dropLookup == null) return;
		if (!dropLookup.TryGetValue(tileType, out var group) || group.drops == null || group.drops.Count == 0) return;

		for (int i = 0; i < group.drops.Count; i++)
		{
			var entry = group.drops[i];
			if (entry == null || entry.itemData == null) continue;
			if (entry.dropChance < 1f && UnityEngine.Random.value > Mathf.Clamp01(entry.dropChance)) continue;
			int minQ = Mathf.Max(0, entry.minQuantity);
			int maxQ = Mathf.Max(minQ, entry.maxQuantity);
			int count = UnityEngine.Random.Range(minQ, maxQ + 1);
			for (int k = 0; k < count; k++)
			{
				SpawnDrop(entry.itemData, worldPosition);
			}
		}
	}

	private void SpawnDrop(ItemData itemData, Vector3 worldPosition)
	{
		if (collectiblePrefab == null) return;

		var drop = Instantiate(collectiblePrefab, worldPosition, Quaternion.identity);
		drop.name = $"Drop_{itemData.itemName}";

		var collectible = drop.GetComponent<Collectible>();
		if (collectible != null)
		{
			collectible.Setup(playerInventory, itemData);
		}

		StartCoroutine(SmoothParabolicJump(drop, worldPosition));
	}


	private IEnumerator SmoothParabolicJump(GameObject drop, Vector3 initialPosition)
	{
		if (drop != null)
		{
			float duration = 1f;
			float maxHeight = 1f;
			Vector3 finalPosition = CalculateRandomFinalPosition(initialPosition);
			float elapsedTime = 0f;
			while (elapsedTime < duration && drop != null)
			{
				elapsedTime += Time.deltaTime;
				float t = elapsedTime / duration;
				Vector3 horizontalPosition = Vector3.Lerp(initialPosition, finalPosition, t);
				float verticalOffset = 4 * maxHeight * t * (1 - t);
				drop.transform.position = horizontalPosition + Vector3.up * verticalOffset;
				yield return null;
			}
			if (drop != null)
			{
				drop.transform.position = finalPosition;
			}
		}
	}

	private Vector3 CalculateRandomFinalPosition(Vector3 initialPosition)
	{
		return initialPosition + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(-1.5f, 1.5f), 0);
	}
	
	private void RemoveFoliageTileAtCell(Vector3Int cell)
	{
		if (foliageTilemap == null)
			return;
		
		// Supprimer la tile de foliage au même endroit si elle existe
		TileBase foliageTile = foliageTilemap.GetTile(cell);
		if (foliageTile != null)
		{
			foliageTilemap.SetTile(cell, null);
		}
	}
}
