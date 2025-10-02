using UnityEngine;

/// <summary>
/// Structure pour gérer les cultures plantées
/// </summary>
[System.Serializable]
public class PlantedCrop
{
    public SeedData originalSeedData; // Référence vers le ScriptableObject original
    public SeedInstance seedInstance; // Instance de la graine
    public int currentStage = 0; // 0=graine, 1=jeune, 2=mature, 3=mûr
    public int dayPlanted; // Jour de plantation (utilise le système de jours du jeu)
    public bool isWithered = false;
    
    // Propriétés pour les cultures pérennes
    public bool isPerennial = false; // Si c'est une culture pérenne (comme les raisins)
    public int lastProductionDay = 0; // Dernier jour de production
    public bool hasFruits = false; // Si la culture a actuellement des fruits à récolter
    
    // Propriétés pour l'arrosage
    public int lastWateredDay = 0; // Dernier jour d'arrosage
}

