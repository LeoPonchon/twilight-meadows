using UnityEngine;

[CreateAssetMenu(fileName = "NewSeed", menuName = "Farming/Seed")]
public class SeedData : ItemData
{
    [Header("Configuration Graine")]
    [Tooltip("Sprites de croissance (chaque sprite = 1 jour)")]
    public Sprite[] growthSprites;
    
    [Tooltip("Sprite de culture flétrie")]
    public Sprite witheredSprite;
    
    [Tooltip("Saison de croissance de cette culture")]
    public Season growthSeason = Season.Spring;
    
    [Tooltip("Produit récolté")]
    public ItemData harvestProduct;
    
    [Tooltip("Quantité de produit récolté")]
    public int harvestQuantity = 1;
    
    [Tooltip("Temps avant flétrissement si non arrosé")]
    public int witherTime = 2;
    
    [Header("Culture Pérenne")]
    [Tooltip("Si cette culture produit régulièrement (comme les raisins)")]
    public bool isPerennial = false;
    
    [Tooltip("Intervalle de production en jours (ex: tous les 2 jours)")]
    public int productionInterval = 2;
    
    [Tooltip("Sprite avec fruits pour les cultures pérennes")]
    public Sprite fruitSprite;
    
    public bool CanPlant()
    {
        // Vérifier la saison actuelle
        TimeManager timeManager = FindObjectOfType<TimeManager>();
        if (timeManager == null) return false;
        
        int currentSeasonId = timeManager.GetCurrentSeasonId();
        int requiredSeasonId = (int)growthSeason;
        
        // La graine ne peut être plantée que pendant sa saison de croissance
        return currentSeasonId == requiredSeasonId;
    }
}