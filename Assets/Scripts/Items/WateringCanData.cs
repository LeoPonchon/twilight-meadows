using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewWateringCan", menuName = "Tools/Watering Can")]
public class WateringCanData : ItemData
{
    [Header("Configuration Arrosoir")]
    [Tooltip("Capacité maximale de l'arrosoir")]
    public int maxCapacity = 10;
    
    [Tooltip("Capacité actuelle de l'arrosoir")]
    public int currentCapacity = 10;
    
    [Tooltip("Portée d'arrosage (nombre de cases)")]
    public int wateringRange = 1;
    
    [Tooltip("Coût en stamina par utilisation")]
    public int staminaCost = 1;
    
    [Header("Rechargement")]
    [Tooltip("Sources d'eau pour recharger l'arrosoir")]
    public TileBase[] waterSourceTiles;
    
    [Tooltip("Temps de rechargement en secondes")]
    public float refillTime = 2f;
}
