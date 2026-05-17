using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

public class TileInteractionController : MonoBehaviour
{
	[Header("Références Système")]
	[SerializeField] private SceneContext sceneContext;
	[SerializeField] private InventoryContainer playerInventory;
	[SerializeField] private InventoryHotbarController inventoryManager;
	[SerializeField] private SpriteRenderer playerSprite;
	[SerializeField] private GameObject collectiblePrefab;

	[Header("Temps / Météo")]
	[SerializeField] private TimeManager timeManager;
	[SerializeField] private WeatherManager weatherManager;

	[Header("Portée Outils")]
	[SerializeField] private float defaultToolUseRange = 1.5f;

	[Header("Tilemap (Nouveau Système)")]
	[Tooltip("Tilemap de sol sur laquelle on remplace la tuile cliquée selon l'outil.")]
	[FormerlySerializedAs("pathTilemap")]
	[SerializeField] private Tilemap soilTilemap;
	[Tooltip("Tilemap pour les cultures.")]
	[SerializeField] private Tilemap cropsTilemap;
	[Tooltip("Tilemap optionnelle pour supprimer du foliage quand on creuse.")]
	[SerializeField] private Tilemap foliageTilemap;

	[Header("Tiles d'États Sol")]
	[SerializeField] private SoilRuleTileSet soilTiles;

	public Tilemap SoilTilemap => soilTilemap;
	public SoilRuleTileSet SoilTiles => soilTiles;

	[Header("Tile Breaking / Drops")]
	[SerializeField] private GameObject tilemapsRoot;
	[SerializeField] private TilemapRegistry tilemapRegistry;

	[System.Serializable]
	public class DropEntry
	{
		public ItemData itemData;
		public int minQuantity = 1;
		public int maxQuantity = 1;
		[Range(0f, 1f)] public float dropChance = 1f;
	}

	[System.Serializable]
	public class TileDropGroup
	{
		public List<TileBase> tiles = new List<TileBase>();
		public List<DropEntry> drops = new List<DropEntry>();
		[System.Serializable]
		public sealed class RequiredTool
		{
			public ItemData itemData;
			[Min(1)] public int hitsToBreak = 1;
		}

		[Tooltip("Liste d'items autorisés pour casser ce groupe, avec le nombre de coups requis. Vide = n'importe quel outil.")]
		public List<RequiredTool> requiredToolItems = new List<RequiredTool>();
	}

	[Header("Configuration des Drops")]
	[SerializeField] private List<TileDropGroup> dropGroups = new List<TileDropGroup>();

	private CropService cropService;
	private SoilService soilService;
	private DropService dropService;
	private readonly Dictionary<Vector3Int, PlantedCrop> plantedCrops = new Dictionary<Vector3Int, PlantedCrop>();
	private readonly List<Tilemap> targetTilemaps = new List<Tilemap>();
	private readonly Dictionary<HitKey, HitState> breakHits = new Dictionary<HitKey, HitState>();
	private int lastFarmingTickDay = -1;

	private Camera mainCamera;

	private readonly struct HitKey
	{
		public readonly int TilemapId;
		public readonly Vector3Int Cell;

		public HitKey(int tilemapId, Vector3Int cell)
		{
			TilemapId = tilemapId;
			Cell = cell;
		}
	}

	private sealed class HitState
	{
		public TileBase Tile;
		public int Hits;
	}

	private void Awake()
	{
		mainCamera = Camera.main;

		if (sceneContext == null)
			sceneContext = FindObjectOfType<SceneContext>();

		if (sceneContext == null)
		{
			Debug.LogError("TileInteractionController: Missing SceneContext in scene.", this);
			enabled = false;
			return;
		}

		playerInventory ??= sceneContext.GetRequired<InventoryContainer>(this, nameof(playerInventory));
		inventoryManager ??= sceneContext.GetRequired<InventoryHotbarController>(this, nameof(inventoryManager));
		timeManager ??= sceneContext.GetRequired<TimeManager>(this, nameof(timeManager));
		weatherManager ??= sceneContext.GetRequired<WeatherManager>(this, nameof(weatherManager));

		InitializePlayerSprite();
		InitializeDropLookup();
		InitializeTargetTilemaps();

		cropService = new CropService();
		soilService = new SoilService();

		if (soilTilemap == null)
			Debug.LogError("TileInteractionController: Missing Soil Tilemap reference.", this);
		if (soilTiles == null || soilTiles.dugTile == null || soilTiles.tilledTile == null || soilTiles.tilledWetTile == null)
			Debug.LogError("TileInteractionController: Missing Soil Tiles (dug/tilled/tilledWet). Assign a SoilRuleTileSet.", this);

		BootstrapSoilWorldFromTilemap();

		if (timeManager != null)
		{
			timeManager.OnSeasonStarted += HandleSeasonChanged;
			timeManager.OnDayChanged += HandleDayChanged;
		}

		if (weatherManager != null)
		{
			weatherManager.OnWeatherChanged += HandleWeatherChanged;
		}
	}

	// Intentionnellement aucune auto-binding ni fallback d'anciennes tiles :
	// on passe par référence `pathTilemap` + `soilTiles` (RuleTiles) en Inspector.

	private void OnDestroy()
	{
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

	public WorldStateSaveData CaptureWorldState()
	{
		var data = new WorldStateSaveData();

		if (soilService != null)
		{
			var soils = soilService.SnapshotSoils();
			for (int i = 0; i < soils.Count; i++)
				data.soils.Add(new WorldStateSaveData.Cell(soils[i].X, soils[i].Y));

			var wet = soilService.SnapshotWetSoils();
			for (int i = 0; i < wet.Count; i++)
				data.wetSoils.Add(new WorldStateSaveData.Cell(wet[i].X, wet[i].Y));
		}

		foreach (var kvp in plantedCrops)
		{
			var cell = kvp.Key;
			var crop = kvp.Value;
			if (crop == null) continue;

			data.crops.Add(new WorldStateSaveData.Crop
			{
				x = cell.x,
				y = cell.y,
				seedName = crop.originalSeedData != null ? crop.originalSeedData.itemName : string.Empty,
				currentStage = crop.currentStage,
				dayPlanted = crop.dayPlanted,
				lastWateredDay = crop.lastWateredDay,
				isWithered = crop.isWithered,
				isPerennial = crop.isPerennial,
				lastProductionDay = crop.lastProductionDay,
				hasFruits = crop.hasFruits,
			});
		}

		return data;
	}

	public void ApplyWorldState(WorldStateSaveData data, System.Func<string, SeedData> resolveSeed)
	{
		if (data == null) return;

		cropService = new CropService();
		soilService = new SoilService();
		plantedCrops.Clear();
		lastFarmingTickDay = -1;

		ClearSoilTilemap();
		if (cropsTilemap != null) cropsTilemap.ClearAllTiles();

		if (soilTilemap != null && soilTiles != null)
		{
			for (int i = 0; i < data.soils.Count; i++)
			{
				var c = data.soils[i];
				var pos = new Vector3Int(c.x, c.y, 0);
				if (soilTiles.tilledTile != null)
					soilTilemap.SetTile(pos, soilTiles.tilledTile);
				soilService.RegisterSoil(new GridPos(c.x, c.y), isWet: false);
				RefreshAroundSingleMap(soilTilemap, pos);
			}

			for (int i = 0; i < data.wetSoils.Count; i++)
			{
				var c = data.wetSoils[i];
				var pos = new Vector3Int(c.x, c.y, 0);
				if (soilTiles.tilledWetTile != null)
					soilTilemap.SetTile(pos, soilTiles.tilledWetTile);
				else if (soilTiles.tilledTile != null)
					soilTilemap.SetTile(pos, soilTiles.tilledTile);
				soilService.RegisterSoil(new GridPos(c.x, c.y), isWet: true);
				RefreshAroundSingleMap(soilTilemap, pos);
			}
		}

		for (int i = 0; i < data.crops.Count; i++)
		{
			var c = data.crops[i];
			if (c == null) continue;

			var seed = resolveSeed != null ? resolveSeed(c.seedName) : null;
			if (seed == null) continue;

			var cell = new Vector3Int(c.x, c.y, 0);
			var crop = new PlantedCrop
			{
				originalSeedData = seed,
				seedInstance = new SeedInstance(seed),
				currentStage = c.currentStage,
				dayPlanted = c.dayPlanted,
				lastWateredDay = c.lastWateredDay,
				isWithered = c.isWithered,
				isPerennial = c.isPerennial,
				lastProductionDay = c.lastProductionDay,
				hasFruits = c.hasFruits,
			};

			plantedCrops[cell] = crop;
			cropService.RestoreCrop(
				new GridPos(c.x, c.y),
				new CropBlueprint(
					growthStagesCount: crop.seedInstance.growthSprites != null ? crop.seedInstance.growthSprites.Length : 0,
					growthSeasonId: (int)crop.seedInstance.growthSeason,
					witherTimeDays: crop.seedInstance.witherTime,
					isPerennial: crop.isPerennial,
					productionIntervalDays: crop.seedInstance.productionInterval),
				currentStage: c.currentStage,
				dayPlanted: c.dayPlanted,
				lastWateredDay: c.lastWateredDay,
				isWithered: c.isWithered,
				lastProductionDay: c.lastProductionDay,
				hasFruits: c.hasFruits);

			UpdateCropSprite(cell, crop);
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
		if (!Input.GetMouseButtonDown(0))
			return;

		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
			return;

		if (soilTilemap == null || soilTiles == null)
			return;

		if (mainCamera == null) mainCamera = Camera.main;
		if (mainCamera == null) return;

		Vector3 world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		world.z = 0f;

		Vector3Int cell = soilTilemap.WorldToCell(world);

		float range = GetSelectedUseRange();
		if (!IsWithinUseRangeCell(cell, range))
			return;

		if (TryHandlePlantingOrHarvest(cell)) return;
		if (TryHandleWatering(cell)) return;
		if (TryHandleSoilTools(cell)) return;

		HandleTileBreaking(world);
	}

	private bool IsWithinUseRangeCell(Vector3Int cell, float range)
	{
		if (range <= 0f) return true;
		var playerPos = GetPlayerWorldPosition();
		if (soilTilemap == null) return true;
		return Vector2.Distance(playerPos, soilTilemap.GetCellCenterWorld(cell)) <= range;
	}

	private float GetSelectedUseRange()
	{
		var item = GetSelectedItemData();
		if (item is ToolData toolData) return toolData.useRange > 0f ? toolData.useRange : defaultToolUseRange;
		if (item is WateringCanData wateringCanData) return wateringCanData.useRange > 0f ? wateringCanData.useRange : defaultToolUseRange;
		return defaultToolUseRange;
	}

	private Vector3 GetPlayerWorldPosition()
	{
		if (playerSprite != null) return playerSprite.transform.position;
		var player = sceneContext != null ? sceneContext.Get<PlayerMovementController>() : null;
		return player != null ? player.transform.position : transform.position;
	}

	private void InitializePlayerSprite()
	{
		if (playerSprite != null) return;
		var player = sceneContext != null ? sceneContext.Get<PlayerMovementController>() : null;
		if (player != null) playerSprite = player.GetComponent<SpriteRenderer>();
	}

	private void InitializeDropLookup()
	{
		var lookup = new Dictionary<TileBase, DropService.DropGroup>();
		for (int i = 0; i < dropGroups.Count; i++)
		{
			var group = dropGroups[i];
			if (group == null || group.tiles == null || group.tiles.Count == 0 || group.drops == null || group.drops.Count == 0)
				continue;

			var drops = new List<DropService.Entry>(group.drops.Count);
			for (int d = 0; d < group.drops.Count; d++)
			{
				var entry = group.drops[d];
				if (entry == null || entry.itemData == null) continue;
				drops.Add(new DropService.Entry(entry.itemData, entry.minQuantity, entry.maxQuantity, entry.dropChance));
			}

			var required = BuildToolRequirements(group.requiredToolItems);
			var dropGroup = new DropService.DropGroup(group.tiles, drops, required);
			for (int t = 0; t < group.tiles.Count; t++)
			{
				var tile = group.tiles[t];
				if (tile == null) continue;
				lookup[tile] = dropGroup;
			}
		}
		dropService = new DropService(lookup);
	}

	private static List<DropService.ToolHitRequirement> BuildToolRequirements(List<TileDropGroup.RequiredTool> requiredToolItems)
	{
		if (requiredToolItems == null || requiredToolItems.Count == 0) return null;

		var list = new List<DropService.ToolHitRequirement>(requiredToolItems.Count);
		for (int i = 0; i < requiredToolItems.Count; i++)
		{
			var r = requiredToolItems[i];
			if (r == null || r.itemData == null) continue;
			int hits = r.hitsToBreak <= 0 ? 1 : r.hitsToBreak;
			list.Add(new DropService.ToolHitRequirement(r.itemData, hits));
		}
		return list.Count > 0 ? list : null;
	}

	private void InitializeTargetTilemaps()
	{
		targetTilemaps.Clear();

		if (tilemapsRoot == null && tilemapRegistry != null)
			tilemapsRoot = tilemapRegistry.TilemapsRoot;

		if (tilemapRegistry != null && tilemapRegistry.Tilemaps != null && tilemapRegistry.Tilemaps.Length > 0)
		{
			targetTilemaps.AddRange(tilemapRegistry.Tilemaps);
			return;
		}

		if (tilemapsRoot != null)
		{
			tilemapsRoot.GetComponentsInChildren(true, targetTilemaps);
		}

		// Éviter de casser/modifier par erreur les tilemaps "système" de ce contrôleur.
		targetTilemaps.Remove(soilTilemap);
		targetTilemaps.Remove(cropsTilemap);
		targetTilemaps.Remove(foliageTilemap);
	}

	private void BootstrapSoilWorldFromTilemap()
	{
		if (soilService == null || soilTilemap == null || soilTiles == null)
			return;

		soilService.BootstrapFromOverTilemap(
			soilTilemap,
			t => IsTilled(t) || IsWet(t),
			IsWet);
	}

	private void ClearSoilTilemap()
	{
		if (soilTilemap == null) return;

		soilTilemap.CompressBounds();
		var bounds = soilTilemap.cellBounds;
		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				var cell = new Vector3Int(x, y, 0);
				var t = soilTilemap.GetTile(cell);
				if (t == null) continue;
				if (IsDug(t) || IsTilled(t) || IsWet(t))
					soilTilemap.SetTile(cell, null);
			}
		}
	}

	private void TickFarmingWorld(int currentDay)
	{
		if (cropService == null || timeManager == null) return;
		if (cropService.Crops.Count == 0) return;

		int currentSeasonId = timeManager.GetCurrentSeasonId();
		var updates = cropService.TickDay(currentDay, currentSeasonId);
		for (int i = 0; i < updates.Count; i++)
		{
			var u = updates[i];
			var cell = new Vector3Int(u.Pos.X, u.Pos.Y, 0);
			if (!plantedCrops.TryGetValue(cell, out var crop) || crop == null) continue;
			if (!cropService.TryGetCrop(u.Pos, out var state) || state == null) continue;

			crop.currentStage = state.CurrentStage;
			crop.isWithered = state.IsWithered;
			crop.lastWateredDay = state.LastWateredDay;
			crop.lastProductionDay = state.LastProductionDay;
			crop.hasFruits = state.HasFruits;

			UpdateCropSprite(cell, crop);
		}
	}

	private void HandleSeasonChanged(int seasonId, string _)
	{
		foreach (var kvp in plantedCrops)
		{
			var cell = kvp.Key;
			var crop = kvp.Value;
			if (crop == null || crop.seedInstance == null) continue;

			if ((int)crop.seedInstance.growthSeason != seasonId && !crop.isWithered)
			{
				crop.isWithered = true;
				UpdateCropSprite(cell, crop);
			}
		}
	}

	private void HandleWeatherChanged(WeatherType weather)
	{
		if (weather == WeatherType.Rainy || weather == WeatherType.Stormy)
			WaterAllSoils();
		else
			DryAllWetSoils();
	}

	private void HandleDayChanged(int _)
	{
		DryAllWetSoils();
	}

	private void WaterAllSoils()
	{
		if (soilTilemap == null || soilTiles == null || timeManager == null || soilService == null) return;

		int currentDay = timeManager.GetCurrentDay();
		cropService?.WaterAllCrops(currentDay);

		var soils = soilService.SnapshotSoils();
		for (int i = 0; i < soils.Count; i++)
		{
			var p = soils[i];
			var cell = new Vector3Int(p.X, p.Y, 0);
			var t = soilTilemap.GetTile(cell);
			if (!IsTilled(t)) continue;

			if (soilTiles.tilledWetTile != null)
				soilTilemap.SetTile(cell, soilTiles.tilledWetTile);

			soilService.MarkWet(p);

			if (plantedCrops.TryGetValue(cell, out var crop) && crop != null)
				crop.lastWateredDay = currentDay;

			RefreshAroundSingleMap(soilTilemap, cell);
		}
	}

	private void DryAllWetSoils()
	{
		if (soilTilemap == null || soilTiles == null || soilService == null) return;

		var wet = soilService.SnapshotWetSoils();
		for (int i = 0; i < wet.Count; i++)
		{
			var p = wet[i];
			var cell = new Vector3Int(p.X, p.Y, 0);
			var t = soilTilemap.GetTile(cell);
			if (!IsWet(t)) continue;

			if (soilTiles.tilledTile != null)
				soilTilemap.SetTile(cell, soilTiles.tilledTile);

			soilService.MarkDry(p);
			RefreshAroundSingleMap(soilTilemap, cell);
		}
	}

	private bool TryHandlePlantingOrHarvest(Vector3Int cell)
	{
		if (cropsTilemap == null)
			return false;

		if (plantedCrops.TryGetValue(cell, out var existing) && existing != null)
		{
			if (CanHarvest(existing))
				return TryHarvestCrop(cell, existing);
			return false;
		}

		var selected = GetSelectedItemData();
		if (selected is SeedData seedData)
			return TryPlantSeed(cell, seedData);

		return false;
	}

	private static bool CanHarvest(PlantedCrop crop)
	{
		if (crop == null || crop.seedInstance == null || crop.seedInstance.growthSprites == null) return false;

		if (crop.isPerennial)
			return crop.hasFruits || crop.isWithered;

		return crop.currentStage >= crop.seedInstance.growthSprites.Length - 1 || crop.isWithered;
	}

	private bool TryHarvestCrop(Vector3Int cell, PlantedCrop crop)
	{
		if (crop == null || crop.seedInstance == null) return false;

		if (crop.isPerennial && crop.hasFruits && !crop.isWithered)
		{
			if (crop.seedInstance.harvestProduct != null)
			{
				Vector3 dropPos = cropsTilemap.GetCellCenterWorld(cell);
				for (int i = 0; i < crop.seedInstance.harvestQuantity; i++)
					SpawnDrop(crop.seedInstance.harvestProduct, dropPos);
			}

			crop.hasFruits = false;
			UpdateCropSprite(cell, crop);
			return true;
		}

		cropsTilemap.SetTile(cell, null);

		if (!crop.isWithered && crop.seedInstance.harvestProduct != null)
		{
			Vector3 dropPos = cropsTilemap.GetCellCenterWorld(cell);
			for (int i = 0; i < crop.seedInstance.harvestQuantity; i++)
				SpawnDrop(crop.seedInstance.harvestProduct, dropPos);
		}

		plantedCrops.Remove(cell);
		cropService?.Remove(new GridPos(cell.x, cell.y));
		return true;
	}

	private bool TryPlantSeed(Vector3Int cell, SeedData seedData)
	{
		if (seedData == null || soilTilemap == null || soilTiles == null || timeManager == null)
			return false;

		if (plantedCrops.ContainsKey(cell))
			return false;

		var soilAtCell = soilTilemap.GetTile(cell);
		bool plantable = IsTilled(soilAtCell) || IsWet(soilAtCell);
		if (!plantable) return false;

		if (!seedData.CanPlant(timeManager.GetCurrentSeasonId()))
			return false;

		if (seedData.growthSprites == null || seedData.growthSprites.Length == 0)
			return false;

		int day = timeManager.GetCurrentDay();

		var crop = new PlantedCrop
		{
			originalSeedData = seedData,
			seedInstance = new SeedInstance(seedData),
			currentStage = 0,
			dayPlanted = day,
			isWithered = false,
			isPerennial = seedData.isPerennial,
			lastProductionDay = 0,
			hasFruits = false,
			lastWateredDay = day
		};

		plantedCrops[cell] = crop;
		cropService.Plant(
			new GridPos(cell.x, cell.y),
			new CropBlueprint(
				growthStagesCount: crop.seedInstance.growthSprites != null ? crop.seedInstance.growthSprites.Length : 0,
				growthSeasonId: (int)crop.seedInstance.growthSeason,
				witherTimeDays: crop.seedInstance.witherTime,
				isPerennial: crop.isPerennial,
				productionIntervalDays: crop.seedInstance.productionInterval),
			currentDay: day);

		cropsTilemap.SetTile(cell, CreateCropTile(crop.seedInstance.growthSprites[0]));
		playerInventory.RemoveItem(seedData, 1);
		return true;
	}

	private void UpdateCropSprite(Vector3Int cell, PlantedCrop crop)
	{
		if (cropsTilemap == null || crop == null || crop.seedInstance == null || crop.seedInstance.growthSprites == null)
			return;

		Sprite spriteToUse;

		if (crop.isWithered && crop.seedInstance.witheredSprite != null)
		{
			spriteToUse = crop.seedInstance.witheredSprite;
		}
		else if (crop.isPerennial && crop.hasFruits && crop.seedInstance.fruitSprite != null)
		{
			spriteToUse = crop.seedInstance.fruitSprite;
		}
		else
		{
			if (crop.isPerennial && !crop.hasFruits)
			{
				int spriteIndex = crop.currentStage - 1;
				if (spriteIndex >= 0 && spriteIndex < crop.seedInstance.growthSprites.Length)
					spriteToUse = crop.seedInstance.growthSprites[spriteIndex];
				else
					spriteToUse = crop.seedInstance.growthSprites[crop.seedInstance.growthSprites.Length - 1];
			}
			else
			{
				if (crop.currentStage < crop.seedInstance.growthSprites.Length)
					spriteToUse = crop.seedInstance.growthSprites[crop.currentStage];
				else
					return;
			}
		}

		cropsTilemap.SetTile(cell, CreateCropTile(spriteToUse));
	}

	private static TileBase CreateCropTile(Sprite sprite)
	{
		var tile = ScriptableObject.CreateInstance<Tile>();
		tile.sprite = sprite;
		return tile;
	}

	private bool TryHandleWatering(Vector3Int cell)
	{
		if (soilTilemap == null || soilTiles == null) return false;

		if (GetSelectedToolKind() != ToolKind.WateringCan)
			return false;

		var selectedItemStack = GetSelectedItemStack();
		if (selectedItemStack == null || selectedItemStack.itemData is not WateringCanData)
			return false;

		var wateringCan = selectedItemStack.GetItemInstance() as WateringCanInstance;
		if (wateringCan == null)
			return false;

		TileBase pathTile = soilTilemap.GetTile(cell);

		// Refill si on clique sur une source d'eau (peut être sur d'autres tilemaps que Path).
		bool isWater = IsWaterTile(pathTile, wateringCan);
		if (!isWater && targetTilemaps != null)
		{
			for (int i = 0; i < targetTilemaps.Count; i++)
			{
				var tm = targetTilemaps[i];
				if (tm == null) continue;
				if (IsWaterTile(tm.GetTile(cell), wateringCan))
				{
					isWater = true;
					break;
				}
			}
		}
		if (isWater)
		{
			if (wateringCan.currentCapacity < wateringCan.maxCapacity)
			{
				wateringCan.Refill();
				return true;
			}
			return false;
		}

		if (!IsTilled(pathTile))
			return false;

		if (!wateringCan.CanWater())
			return false;

		if (soilTiles.tilledWetTile != null)
			soilTilemap.SetTile(cell, soilTiles.tilledWetTile);

		soilService?.MarkWet(new GridPos(cell.x, cell.y));
		wateringCan.UseWater();

		if (plantedCrops.TryGetValue(cell, out var crop) && crop != null)
			crop.lastWateredDay = timeManager != null ? timeManager.GetCurrentDay() : crop.lastWateredDay;

		RefreshAroundSingleMap(soilTilemap, cell);
		return true;
	}

	private bool TryHandleSoilTools(Vector3Int cell)
	{
		if (soilTilemap == null || soilTiles == null)
			return false;

		ToolKind tool = GetSelectedToolKind();
		if (tool == ToolKind.None) return false;

		bool hasCrop = plantedCrops.ContainsKey(cell);
		if (hasCrop && (tool == ToolKind.Shovel || tool == ToolKind.Hoe))
			return false;

		TileBase at = soilTilemap.GetTile(cell);
		bool changed = false;

		switch (tool)
		{
			case ToolKind.Shovel:
				// Shovel = terre creusée
				if (soilTiles.dugTile == null) return false;
				if (at == soilTiles.dugTile) return false;
				soilTilemap.SetTile(cell, soilTiles.dugTile);
				soilService?.Unregister(new GridPos(cell.x, cell.y));
				changed = true;
				RemoveFoliageTileAtCell(cell);
				break;

			case ToolKind.Hoe:
				// Hoe = terre retournée (uniquement depuis terre creusée)
				if (!IsDug(at)) return false;
				if (soilTiles.tilledTile == null) return false;
				soilTilemap.SetTile(cell, soilTiles.tilledTile);
				soilService?.RegisterSoil(new GridPos(cell.x, cell.y), isWet: false);
				changed = true;
				break;

			case ToolKind.Pickaxe:
				if (hasCrop)
				{
					DestroyCropAtCell(cell);
					changed = true;
				}
				else if (at != null && (IsDug(at) || IsTilled(at) || IsWet(at)))
				{
					soilTilemap.SetTile(cell, null);
					soilService?.Unregister(new GridPos(cell.x, cell.y));
					changed = true;
				}
				break;
		}

		if (changed)
			RefreshAroundSingleMap(soilTilemap, cell);

		return changed;
	}

	private void DestroyCropAtCell(Vector3Int cell)
	{
		if (!plantedCrops.ContainsKey(cell))
			return;

		if (cropsTilemap != null)
			cropsTilemap.SetTile(cell, null);

		plantedCrops.Remove(cell);
		cropService?.Remove(new GridPos(cell.x, cell.y));
	}

	private void HandleTileBreaking(Vector3 world)
	{
		if (dropService == null || targetTilemaps == null || targetTilemaps.Count == 0)
			return;

		for (int i = 0; i < targetTilemaps.Count; i++)
		{
			var tm = targetTilemaps[i];
			if (tm == null) continue;

			Vector3Int cell = tm.WorldToCell(world);
			TileBase clickedTile = tm.GetTile(cell);
			if (clickedTile == null) continue;

			if (!IsWithinUseRange(tm, cell, GetSelectedUseRange()))
				return;

			if (!dropService.TryGetGroup(clickedTile, out var group) || group == null)
				continue;

			if (!TryGetAuthorizedBreakToolHits(group.RequiredTools, out int hitsToBreak))
				return;

			ApplyBreakHit(tm, cell, clickedTile, hitsToBreak);
			break;
		}
	}

	private void ApplyBreakHit(Tilemap tm, Vector3Int cell, TileBase clickedTile, int hitsToBreak)
	{
		if (tm == null || clickedTile == null) return;

		var key = new HitKey(tm.GetInstanceID(), cell);
		if (!breakHits.TryGetValue(key, out var state) || state == null)
		{
			state = new HitState { Tile = clickedTile, Hits = 0 };
			breakHits[key] = state;
		}
		else if (state.Tile != clickedTile)
		{
			state.Tile = clickedTile;
			state.Hits = 0;
		}

		state.Hits++;
		if (state.Hits < hitsToBreak)
			return;

		breakHits.Remove(key);
		BreakSingle(tm, cell);
	}

	private bool IsWithinUseRange(Tilemap tm, Vector3Int cell, float range)
	{
		if (range <= 0f) return true;
		if (tm == null) return true;
		return Vector2.Distance(GetPlayerWorldPosition(), tm.GetCellCenterWorld(cell)) <= range;
	}

	private ToolKind GetSelectedToolKind()
	{
		if (playerInventory == null || inventoryManager == null)
			return ToolKind.None;

		int slot = inventoryManager.GetCurrentHotbarSlot();
		ItemStack stack = playerInventory.GetItemInSlot(slot);
		if (stack == null || stack.itemData == null)
			return ToolKind.None;

		return ItemInstanceFactory.GetToolKind(stack.itemData);
	}

	private ItemData GetSelectedItemData()
	{
		if (playerInventory == null || inventoryManager == null)
			return null;

		int slot = inventoryManager.GetCurrentHotbarSlot();
		ItemStack stack = playerInventory.GetItemInSlot(slot);
		return stack?.itemData;
	}

	private ItemStack GetSelectedItemStack()
	{
		if (playerInventory == null || inventoryManager == null)
			return null;

		int slot = inventoryManager.GetCurrentHotbarSlot();
		return playerInventory.GetItemInSlot(slot);
	}

	private bool TryGetAuthorizedBreakToolHits(List<DropService.ToolHitRequirement> requiredTools, out int hitsToBreak)
	{
		hitsToBreak = 1;

		if (playerInventory == null || inventoryManager == null)
			return false;

		int slot = inventoryManager.GetCurrentHotbarSlot();
		ItemStack stack = playerInventory.GetItemInSlot(slot);
		if (stack == null || stack.itemData == null)
			return false;

		ItemData data = stack.itemData;

		// Si aucune liste n'est définie: n'importe quel outil casse en 1 coup.
		if (requiredTools == null || requiredTools.Count == 0)
		{
			return IsTool(data);
		}

		for (int i = 0; i < requiredTools.Count; i++)
		{
			var r = requiredTools[i];
			if (r == null || r.Item == null) continue;
			if (data == r.Item)
			{
				hitsToBreak = r.HitsToBreak <= 0 ? 1 : r.HitsToBreak;
				return true;
			}
		}

		return false;
	}

	private static bool IsTool(ItemData item)
	{
		return item is WateringCanData ||
		       item is ShovelData ||
		       item is AxeData ||
		       item is PickaxeData ||
		       item is SpearData ||
		       item is HoeData;
	}

	// ToolKind n'est plus utilisé pour la casse/destruction (uniquement ailleurs dans le contrôleur).

	private void BreakSingle(Tilemap tm, Vector3Int cell)
	{
		TileBase tile = tm.GetTile(cell);
		if (tile == null) return;

		Vector3 dropPos = tm.GetCellCenterWorld(cell);
		tm.SetTile(cell, null);
		dropService?.SpawnConfiguredDropsForTile(tile, dropPos, SpawnDrop);
	}

	private void SpawnDrop(ItemData itemData, Vector3 worldPosition)
	{
		if (collectiblePrefab == null || itemData == null) return;

		var drop = Instantiate(collectiblePrefab, worldPosition, Quaternion.identity);
		drop.name = $"Drop_{itemData.itemName}";

		var collectible = drop.GetComponent<ItemPickupController>();
		if (collectible != null)
			collectible.Setup(playerInventory, itemData);

		StartCoroutine(SmoothParabolicJump(drop, worldPosition));
	}

	private IEnumerator SmoothParabolicJump(GameObject drop, Vector3 initialPosition)
	{
		if (drop == null) yield break;

		float duration = 1f;
		float maxHeight = 1f;
		Vector3 finalPosition = initialPosition + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
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
			drop.transform.position = finalPosition;
	}

	private void RemoveFoliageTileAtCell(Vector3Int cell)
	{
		if (foliageTilemap == null) return;
		if (foliageTilemap.GetTile(cell) == null) return;
		foliageTilemap.SetTile(cell, null);
	}

	private bool IsDug(TileBase t) => t != null && soilTiles != null && soilTiles.dugTile != null && t == soilTiles.dugTile;
	private bool IsTilled(TileBase t) => t != null && soilTiles != null && soilTiles.tilledTile != null && t == soilTiles.tilledTile;
	private bool IsWet(TileBase t) => t != null && soilTiles != null && soilTiles.tilledWetTile != null && t == soilTiles.tilledWetTile;

	private static bool IsWaterTile(TileBase tile, WateringCanInstance wateringCanInstance)
	{
		if (tile == null || wateringCanInstance == null || wateringCanInstance.waterSourceTiles == null) return false;
		foreach (TileBase waterSourceTile in wateringCanInstance.waterSourceTiles)
		{
			if (tile == waterSourceTile)
				return true;
		}
		return false;
	}

	private static void RefreshAroundSingleMap(Tilemap map, Vector3Int c)
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
