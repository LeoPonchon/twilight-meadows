using UnityEngine;

[CreateAssetMenu(fileName = "NewTool", menuName = "Tools/Tool")]
public abstract class ToolData : ItemData
{
    [Header("Configuration Outil")]
    [Tooltip("Durabilité maximale de l'outil")]
    public int maxDurability = 100;
    
    [Tooltip("Durabilité actuelle de l'outil")]
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
