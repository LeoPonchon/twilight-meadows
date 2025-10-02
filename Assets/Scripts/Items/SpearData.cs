using UnityEngine;

[CreateAssetMenu(fileName = "NewSpear", menuName = "Tools/Spear")]
public class SpearData : ItemData
{
    [Header("Configuration Lance")]
    [Tooltip("Durabilité maximale de la lance")]
    public int maxDurability = 100;
    
    [Tooltip("Durabilité actuelle de la lance")]
    public int currentDurability = 100;
    
    [Tooltip("Coût en stamina par utilisation")]
    public int staminaCost = 1;
    
    [Tooltip("Dégâts infligés aux ennemis")]
    public int damage = 20;
    
    [Tooltip("Portée d'attaque")]
    public float attackRange = 2f;
    
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
