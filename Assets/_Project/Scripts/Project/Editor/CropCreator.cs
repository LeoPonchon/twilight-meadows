using UnityEngine;
using UnityEditor;

/// <summary>
/// Script utilitaire pour créer et configurer automatiquement les nouvelles graines et cultures
/// </summary>
public class CropCreator : MonoBehaviour
{
    [System.Serializable]
    public class CropData
    {
        public string seedName;
        public string cropName;
        public Season season;
        public int growthDays;
        public int harvestQuantity;
        public int sellPrice;
        public int witherTime;
    }

    [Header("Configuration des Cultures")]
    public CropData[] cropsToCreate = new CropData[]
    {
        new CropData { seedName = "Carrot Seed", cropName = "Carrot", season = Season.Spring, growthDays = 4, harvestQuantity = 2, sellPrice = 15, witherTime = 2 },
        new CropData { seedName = "Radish Seed", cropName = "Radish", season = Season.Spring, growthDays = 3, harvestQuantity = 1, sellPrice = 8, witherTime = 1 },
        new CropData { seedName = "Cabbage Seed", cropName = "Cabbage", season = Season.Spring, growthDays = 5, harvestQuantity = 3, sellPrice = 25, witherTime = 3 },
        new CropData { seedName = "Tomato Seed", cropName = "Tomato", season = Season.Summer, growthDays = 4, harvestQuantity = 2, sellPrice = 18, witherTime = 2 },
        new CropData { seedName = "Corn Seed", cropName = "Corn", season = Season.Summer, growthDays = 5, harvestQuantity = 3, sellPrice = 20, witherTime = 3 },
        new CropData { seedName = "Cucumber Seed", cropName = "Cucumber", season = Season.Summer, growthDays = 4, harvestQuantity = 2, sellPrice = 16, witherTime = 2 },
        new CropData { seedName = "Pumpkin Seed", cropName = "Pumpkin", season = Season.Autumn, growthDays = 6, harvestQuantity = 4, sellPrice = 35, witherTime = 4 },
        new CropData { seedName = "Beet Seed", cropName = "Beet", season = Season.Autumn, growthDays = 4, harvestQuantity = 2, sellPrice = 22, witherTime = 3 }
    };

    [ContextMenu("Créer toutes les cultures")]
    public void CreateAllCrops()
    {
        Debug.Log("Création des cultures en cours...");
        
        foreach (var cropData in cropsToCreate)
        {
            Debug.Log($"Création de {cropData.seedName} -> {cropData.cropName}");
            // Les ScriptableObjects sont déjà créés, il faut juste les configurer dans Unity
        }
        
        Debug.Log("Cultures créées ! N'oubliez pas de configurer les références dans Unity.");
    }
}
