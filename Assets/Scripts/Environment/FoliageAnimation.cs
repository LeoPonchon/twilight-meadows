using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class FoliageAnimation : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Tilemap foliageTilemap;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float rotationAngle = 8f; // Angle de rotation en degrés (petit coup)
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // Dictionnaire pour stocker les tiles en cours d'animation
    private Dictionary<Vector3Int, Coroutine> animatingTiles = new Dictionary<Vector3Int, Coroutine>();
    
    // Set pour stocker les tiles qui ont déjà été animées dans cette session de contact
    private HashSet<Vector3Int> animatedTiles = new HashSet<Vector3Int>();
    
    // Position actuelle du joueur pour détecter les changements de tile
    private Vector3Int lastPlayerCell = Vector3Int.zero;
    
    // Matrices de transformation originales pour chaque tile
    private Dictionary<Vector3Int, Matrix4x4> originalTransforms = new Dictionary<Vector3Int, Matrix4x4>();
    
    private void Awake()
    {
        // Si la tilemap n'est pas assignée, essayer de la trouver automatiquement
        if (foliageTilemap == null)
        {
            foliageTilemap = GetComponent<Tilemap>();
        }
        
        // Stocker les transformations originales de toutes les tiles
        StoreOriginalTransforms();
    }
    
    private void StoreOriginalTransforms()
    {
        if (foliageTilemap == null) return;
        
        BoundsInt bounds = foliageTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = foliageTilemap.GetTile(cell);
                
                if (tile != null)
                {
                    // Stocker la transformation actuelle comme transformation originale
                    Matrix4x4 currentTransform = foliageTilemap.GetTransformMatrix(cell);
                    originalTransforms[cell] = currentTransform;
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Trouver la tile exacte sous le joueur
            Vector3 playerPosition = other.transform.position;
            Vector3Int playerCell = foliageTilemap.WorldToCell(playerPosition);
            
            // Animer uniquement la tile directement sous le joueur
            AnimateDirectTile(playerCell);
            lastPlayerCell = playerCell;
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Trouver la tile exacte sous le joueur
            Vector3 playerPosition = other.transform.position;
            Vector3Int playerCell = foliageTilemap.WorldToCell(playerPosition);
            
            // Si le joueur a changé de tile, animer la nouvelle tile
            if (playerCell != lastPlayerCell)
            {
                AnimateDirectTile(playerCell);
                lastPlayerCell = playerCell;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Quand le joueur quitte la zone, réinitialiser les tiles animées
            // pour permettre de rejouer l'animation si on repasse dessus plus tard
            animatedTiles.Clear();
        }
    }
    
    private void AnimateDirectTile(Vector3Int cell)
    {
        // Vérifier si cette tile existe, contient du foliage, et n'a pas déjà été animée
        TileBase tile = foliageTilemap.GetTile(cell);
        if (tile != null && !animatingTiles.ContainsKey(cell) && !animatedTiles.Contains(cell))
        {
            StartTileAnimation(cell, 0f); // Pas de délai pour un contact direct
        }
    }
    
    private void StartTileAnimation(Vector3Int cell, float delay)
    {
        // Si cette tile est déjà en cours d'animation, ne pas la relancer
        if (animatingTiles.ContainsKey(cell))
            return;
        
        // Lancer la coroutine d'animation
        Coroutine animationCoroutine = StartCoroutine(AnimateTile(cell, delay));
        animatingTiles[cell] = animationCoroutine;
    }
    
    private IEnumerator AnimateTile(Vector3Int cell, float delay)
    {
        // Attendre le délai si nécessaire
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        
        // Récupérer la transformation originale
        Matrix4x4 originalTransform = originalTransforms.ContainsKey(cell) 
            ? originalTransforms[cell] 
            : Matrix4x4.identity;
        
        // Phase 1: Petit coup vers la gauche (rapide)
        SetTileRotation(cell, originalTransform, -rotationAngle);
        yield return new WaitForSeconds(animationDuration / 6);
        
        // Phase 2: Retour au centre (rapide)
        SetTileRotation(cell, originalTransform, 0f);
        yield return new WaitForSeconds(animationDuration / 6);
        
        // Phase 3: Petit coup vers la droite (rapide)
        SetTileRotation(cell, originalTransform, rotationAngle);
        yield return new WaitForSeconds(animationDuration / 6);
        
        // Phase 4: Retour définitif au centre
        SetTileRotation(cell, originalTransform, 0f);
        
        // Marquer cette tile comme ayant été animée
        animatedTiles.Add(cell);
        
        // Nettoyer la référence de l'animation
        animatingTiles.Remove(cell);
    }
    
    private void SetTileRotation(Vector3Int cell, Matrix4x4 originalTransform, float angle)
    {
        // Appliquer directement la rotation sans interpolation
        Matrix4x4 newTransform = Matrix4x4.TRS(
            originalTransform.GetColumn(3), // Position originale
            Quaternion.Euler(0f, 0f, angle), // Rotation directe
            Vector3.one // Scale original
        );
        
        foliageTilemap.SetTransformMatrix(cell, newTransform);
    }
    
    private float GetCurrentRotationAngle(Vector3Int cell)
    {
        Matrix4x4 currentTransform = foliageTilemap.GetTransformMatrix(cell);
        Quaternion rotation = Quaternion.LookRotation(
            currentTransform.GetColumn(2),
            currentTransform.GetColumn(1)
        );
        
        return rotation.eulerAngles.z;
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || foliageTilemap == null) return;
        
        // Dessiner la zone de détection
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Dessiner les tiles en cours d'animation
        Gizmos.color = Color.red;
        foreach (var cell in animatingTiles.Keys)
        {
            Vector3 worldPos = foliageTilemap.CellToWorld(cell);
            Gizmos.DrawWireCube(worldPos + Vector3.one * 0.5f, Vector3.one);
        }
    }
    
    // Méthode publique pour forcer l'animation d'une tile spécifique (utile pour les tests)
    public void AnimateSpecificTile(Vector3Int cell)
    {
        if (foliageTilemap.GetTile(cell) != null && !animatingTiles.ContainsKey(cell))
        {
            StartTileAnimation(cell, 0f);
        }
    }
    
    // Méthode publique pour animer la tile directement sous une position donnée
    public void AnimateTileAtPosition(Vector3 worldPosition)
    {
        Vector3Int cell = foliageTilemap.WorldToCell(worldPosition);
        AnimateDirectTile(cell);
    }
    
    // Méthode publique pour arrêter toutes les animations
    public void StopAllAnimations()
    {
        foreach (var coroutine in animatingTiles.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        animatingTiles.Clear();
        
        // Remettre toutes les tiles à leur position originale
        RestoreAllOriginalTransforms();
    }
    
    // Méthode publique pour réinitialiser les tiles animées (utile pour les tests)
    public void ResetAnimatedTiles()
    {
        animatedTiles.Clear();
    }
    
    private void RestoreAllOriginalTransforms()
    {
        foreach (var kvp in originalTransforms)
        {
            foliageTilemap.SetTransformMatrix(kvp.Key, kvp.Value);
        }
    }
}