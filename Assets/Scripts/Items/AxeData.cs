using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Tools/Axe")]
public class AxeData : ItemData
{
    [Header("Configuration Hache")]
    [Tooltip("Durabilité maximale de la hache")]
    public int maxDurability = 100;
    
    [Tooltip("Durabilité actuelle de la hache")]
    public int currentDurability = 100;
    
    [Tooltip("Coût en stamina par utilisation")]
    public int staminaCost = 2;
    
    [Tooltip("Dégâts infligés aux arbres")]
    public int damage = 10;
    
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
