using UnityEngine;

[System.Serializable]
public class PlantedCrop
{
    public SeedData originalSeedData;
    public SeedInstance seedInstance;
    public int currentStage = 0;
    public int dayPlanted;
    public bool isWithered = false;
    
    public bool isPerennial = false;
    public int lastProductionDay = 0;
    public bool hasFruits = false;
    
    public int lastWateredDay = 0;
}

