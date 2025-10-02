using UnityEngine;

[CreateAssetMenu(fileName = "NewPickaxe", menuName = "Tools/Pickaxe")]
public class PickaxeData : ItemData
{
    [Header("Configuration Pioche")]
    [Tooltip("Durabilité maximale de la pioche")]
    public int maxDurability = 100;
    
    [Tooltip("Durabilité actuelle de la pioche")]
    public int currentDurability = 100;
    
    [Tooltip("Coût en stamina par utilisation")]
    public int staminaCost = 2;
    
    [Tooltip("Dégâts infligés aux roches")]
    public int damage = 15;
    
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
