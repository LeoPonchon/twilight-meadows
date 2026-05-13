using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Farming/Crop Data")]
public class CropData : ItemData
{
    [Header("Données de culture")]
    [Tooltip("Temps de croissance en jours")]
    public int growthDays = 4;
    
    [Tooltip("Saisons où cette culture peut être plantée")]
    public List<Season> validSeasons = new List<Season> { Season.Spring };
    
    [Tooltip("Sprites de croissance (0 = graine, dernier = mature)")]
    public List<Sprite> growthSprites = new List<Sprite>();
    
    [Tooltip("Sprite de culture flétrie")]
    public Sprite witheredSprite;
    
    [Header("Récolte")]
    [Tooltip("Item produit lors de la récolte")]
    public ItemData harvestItem;
    
    [Tooltip("Quantité min/max de récolte")]
    public int minHarvest = 1;
    public int maxHarvest = 1;
    
    [Tooltip("Chance de donner des graines supplémentaires")]
    [Range(0f, 1f)]
    public float seedDropChance = 0.1f;
    
    [Tooltip("Quantité de graines supplémentaires")]
    public int extraSeedsAmount = 1;
    
    [Header("Arrosage")]
    [Tooltip("Jours sans arrosage avant flétrissure")]
    public int daysWithoutWater = 2;
    
    [Tooltip("Besoin d'arrosage quotidien")]
    public bool needsDailyWatering = true;
}

public enum Season
{
    Spring = 1,
    Summer = 2,
    Autumn = 3,
    Winter = 4
}
