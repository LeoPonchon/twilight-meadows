using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class CollectiblesGenerator : MonoBehaviour
{
	[Header("Références")]
	[SerializeField] private TimeManager timeManager;
	[Tooltip("Tilemap où placer les rochers")]
	[SerializeField] private Tilemap rocksTilemap;
	[Tooltip("Tilemap où placer les arbres")]
	[SerializeField] private Tilemap treesTilemap;
	[Tooltip("Tilemap du terrain (doit contenir l'herbe)")]
	[SerializeField] private Tilemap terrainTilemap;

	[Header("Tiles de sol autorisées")]
	[Tooltip("Tiles sur lesquelles on peut placer des rochers/arbres")]
	[SerializeField] private TileBase[] allowedGroundTiles;

	[Header("Rochers")]
	[Tooltip("Tiles de rochers à placer")]
	[SerializeField] private TileBase[] rockTiles;
	[Tooltip("Probabilité de génération (0-1)")]
	[Range(0f, 1f)] [SerializeField] private float rockProbability = 0.1f;
	[Tooltip("Activer le gradient vertical (plus de rochers en haut)")]
	[SerializeField] private bool useRockGradient = true;
	[Tooltip("Probabilité minimale (en bas de carte)")]
	[Range(0f, 1f)] [SerializeField] private float minRockProbability = 0.02f;
	[Tooltip("Probabilité maximale (en haut de carte)")]
	[Range(0f, 1f)] [SerializeField] private float maxRockProbability = 0.25f;
	[Tooltip("Courbure du gradient (1 = linéaire, >1 = accentue l'écart)")]
	[Range(0.1f, 5f)] [SerializeField] private float rockGradientCurve = 1.5f;

	[Header("Arbres")]
	[Tooltip("Tiles d'arbres à placer")]
	[SerializeField] private TileBase[] treeTiles;
	[Tooltip("Probabilité de génération (0-1)")]
	[Range(0f, 1f)] [SerializeField] private float treeProbability = 0.05f;
	[Tooltip("Activer le gradient vertical (plus d'arbres en haut)")]
	[SerializeField] private bool useTreeGradient = false;
	[Tooltip("Probabilité minimale (en bas de carte)")]
	[Range(0f, 1f)] [SerializeField] private float minTreeProbability = 0.02f;
	[Tooltip("Probabilité maximale (en haut de carte)")]
	[Range(0f, 1f)] [SerializeField] private float maxTreeProbability = 0.15f;
	[Tooltip("Courbure du gradient (1 = linéaire, >1 = accentue l'écart)")]
	[Range(0.1f, 5f)] [SerializeField] private float treeGradientCurve = 1.5f;
	[Tooltip("Cocher pour générer les arbres")]
	[SerializeField] private bool generateTrees = true;

	[Header("Seed aléatoire")]
	[SerializeField] private int randomSeed = 0; // 0 = non forcé

	private System.Random prng;

	private void Awake()
	{
		if (timeManager == null)
		{
			timeManager = FindObjectOfType<TimeManager>();
		}

		if (randomSeed == 0)
		{
			prng = new System.Random(Guid.NewGuid().GetHashCode());
		}
		else
		{
			prng = new System.Random(randomSeed);
		}

		if (timeManager != null)
		{
			timeManager.OnSeasonStarted += HandleSeasonStarted;
		}
	}

	private void OnDestroy()
	{
		if (timeManager != null)
		{
			timeManager.OnSeasonStarted -= HandleSeasonStarted;
		}
	}

	private void Start()
	{
		GenerateRocks();
		if (generateTrees)
		{
			GenerateTrees();
		}
	}

	private void HandleSeasonStarted(int seasonId, string seasonName)
	{
		GenerateRocks();
		if (generateTrees)
		{
			GenerateTrees();
		}
	}

	public void GenerateRocks()
	{
		if (rocksTilemap == null || terrainTilemap == null)
		{
			Debug.LogError("CollectiblesGenerator: rocksTilemap ou terrainTilemap non assignée");
			return;
		}
		if (rockTiles == null || rockTiles.Length == 0)
		{
			Debug.LogError("CollectiblesGenerator: aucune tile de rocher fournie");
			return;
		}

		// Obtenir les bornes de la tilemap de terrain
		terrainTilemap.CompressBounds();
		BoundsInt bounds = terrainTilemap.cellBounds;

		int yMin = bounds.yMin;
		int yMax = bounds.yMax;

		int rocksPlaced = 0;
		for (int x = bounds.xMin; x <= bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y <= bounds.yMax; y++)
			{
				Vector3Int cell = new Vector3Int(x, y, 0);

				// Vérifier si le sol est autorisé
				if (!IsGroundAllowed(cell)) continue;

				// Vérifier si la position est libre
				if (rocksTilemap.GetTile(cell) != null) continue;

				// Calculer la probabilité selon la configuration
				float currentProbability = rockProbability;
				if (useRockGradient)
				{
					// Calculer la probabilité selon la position Y (gradient vertical)
					float t = (yMax == yMin) ? 1f : Mathf.InverseLerp(yMin, yMax, y);
					t = Mathf.Pow(t, rockGradientCurve);
					currentProbability = Mathf.Lerp(minRockProbability, maxRockProbability, t);
				}

				// Générer selon la probabilité calculée
				if (prng.NextDouble() <= currentProbability)
				{
					// Choisir une tile de rocher aléatoire
					TileBase rockTile = rockTiles[prng.Next(rockTiles.Length)];
					rocksTilemap.SetTile(cell, rockTile);
					rocksPlaced++;
				}
			}
		}

		Debug.Log($"CollectiblesGenerator: {rocksPlaced} rochers placés");
	}

	public void GenerateTrees()
	{
		if (treesTilemap == null || terrainTilemap == null)
		{
			Debug.LogError("CollectiblesGenerator: treesTilemap ou terrainTilemap non assignée");
			return;
		}
		if (treeTiles == null || treeTiles.Length == 0)
		{
			Debug.Log("CollectiblesGenerator: aucune tile d'arbre fournie");
			return;
		}

		// Obtenir les bornes de la tilemap de terrain
		terrainTilemap.CompressBounds();
		BoundsInt bounds = terrainTilemap.cellBounds;

		int yMin = bounds.yMin;
		int yMax = bounds.yMax;

		int treesPlaced = 0;
		for (int x = bounds.xMin; x <= bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y <= bounds.yMax; y++)
			{
				Vector3Int cell = new Vector3Int(x, y, 0);

				// Vérifier si le sol est autorisé
				if (!IsGroundAllowed(cell)) continue;

				// Vérifier si la position est libre
				if (treesTilemap.GetTile(cell) != null) continue;

				// Calculer la probabilité selon la configuration
				float currentProbability = treeProbability;
				if (useTreeGradient)
				{
					// Calculer la probabilité selon la position Y (gradient vertical)
					float t = (yMax == yMin) ? 1f : Mathf.InverseLerp(yMin, yMax, y);
					t = Mathf.Pow(t, treeGradientCurve);
					currentProbability = Mathf.Lerp(minTreeProbability, maxTreeProbability, t);
				}

				// Générer selon la probabilité calculée
				if (prng.NextDouble() <= currentProbability)
				{
					// Choisir une tile d'arbre aléatoire
					TileBase treeTile = treeTiles[prng.Next(treeTiles.Length)];
					treesTilemap.SetTile(cell, treeTile);
					treesPlaced++;
				}
			}
		}

		Debug.Log($"CollectiblesGenerator: {treesPlaced} arbres placés");
	}

	private bool IsGroundAllowed(Vector3Int cell)
	{
		if (terrainTilemap == null) return false;

		TileBase groundTile = terrainTilemap.GetTile(cell);
		if (groundTile == null) return false;

		// Si aucune restriction, autoriser tous les sols
		if (allowedGroundTiles == null || allowedGroundTiles.Length == 0) return true;

		// Vérifier si le sol est dans la liste autorisée
		for (int i = 0; i < allowedGroundTiles.Length; i++)
		{
			if (allowedGroundTiles[i] == groundTile) return true;
		}

		return false;
	}
}