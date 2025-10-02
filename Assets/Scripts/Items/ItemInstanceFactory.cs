using UnityEngine;

/// <summary>
/// Factory pour créer des instances d'items à partir des ScriptableObject
/// </summary>
public static class ItemInstanceFactory
{
    /// <summary>
    /// Crée une instance d'item à partir d'un ItemData
    /// </summary>
    public static ItemInstance CreateInstance(ItemData itemData)
    {
        if (itemData == null) return null;

        // Créer l'instance appropriée selon le type
        if (itemData is WateringCanData wateringCanData)
        {
            return new WateringCanInstance(wateringCanData);
        }
        else if (itemData is ShovelData shovelData)
        {
            return new ShovelInstance(shovelData);
        }
        else if (itemData is AxeData axeData)
        {
            return new AxeInstance(axeData);
        }
        else if (itemData is PickaxeData pickaxeData)
        {
            return new PickaxeInstance(pickaxeData);
        }
        else if (itemData is SpearData spearData)
        {
            return new SpearInstance(spearData);
        }
        else if (itemData is HoeData hoeData)
        {
            return new HoeInstance(hoeData);
        }
        else if (itemData is SeedData seedData)
        {
            return new SeedInstance(seedData);
        }
        else
        {
            // Item générique
            return new ItemInstance(itemData);
        }
    }

    /// <summary>
    /// Vérifie si un ItemData est un outil
    /// </summary>
    public static bool IsTool(ItemData itemData)
    {
        return itemData is WateringCanData || 
               itemData is ShovelData || 
               itemData is AxeData || 
               itemData is PickaxeData || 
               itemData is SpearData || 
               itemData is HoeData;
    }

    /// <summary>
    /// Obtient le ToolKind d'un ItemData
    /// </summary>
    public static ToolKind GetToolKind(ItemData itemData)
    {
        if (itemData == null) return ToolKind.None;
        
        if (itemData is WateringCanData) return ToolKind.WateringCan;
        if (itemData is ShovelData) return ToolKind.Shovel;
        if (itemData is AxeData) return ToolKind.Axe;
        if (itemData is PickaxeData) return ToolKind.Pickaxe;
        if (itemData is SpearData) return ToolKind.Spear;
        if (itemData is HoeData) return ToolKind.Hoe;
        if (itemData is SeedData) return ToolKind.None; // Les graines ne sont pas des outils
        
        return ToolKind.None;
    }
}
