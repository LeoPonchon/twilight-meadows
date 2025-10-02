using UnityEngine;

[CreateAssetMenu(fileName = "NewShovel", menuName = "Tools/Shovel")]
public class ShovelData : ItemData
{
    [Header("Configuration Pelle")]
    [Tooltip("Durabilité maximale de la pelle")]
    public int maxDurability = 100;
    
    [Tooltip("Durabilité actuelle de la pelle")]
    public int currentDurability = 100;
    
    [Tooltip("Coût en stamina par utilisation")]
    public int staminaCost = 1;
    
    public bool CanUse()
    {
        return currentDurability > 0;
    }
    
    public bool Use()
    {
        if (currentDurability <= 0) return false;
        
        currentDurability--;
        return true;
    }
    
    public void Repair()
    {
        currentDurability = maxDurability;
    }
    
    public float GetDurabilityPercentage()
    {
        return (float)currentDurability / maxDurability;
    }
}
