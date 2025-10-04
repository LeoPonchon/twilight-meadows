using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class ItemInstance
{
    public ItemData originalData;
    public string itemName;
    public string description;
    public Sprite icon;
    public bool isStackable;

    public ItemInstance(ItemData originalData)
    {
        this.originalData = originalData;
        this.itemName = originalData.itemName;
        this.description = originalData.description;
        this.icon = originalData.icon;
        this.isStackable = originalData.isStackable;
    }

    public virtual bool CanUse() { return true; }
    public virtual bool Use() { return true; }
}

[System.Serializable]
public class WateringCanInstance : ItemInstance
{
    public int maxCapacity;
    public int currentCapacity;
    public int wateringRange;
    public int staminaCost;
    public TileBase[] waterSourceTiles;
    public float refillTime;

    public WateringCanInstance(WateringCanData originalData) : base(originalData)
    {
        this.maxCapacity = originalData.maxCapacity;
        this.currentCapacity = originalData.currentCapacity;
        this.wateringRange = originalData.wateringRange;
        this.staminaCost = originalData.staminaCost;
        this.waterSourceTiles = new TileBase[originalData.waterSourceTiles.Length];
        System.Array.Copy(originalData.waterSourceTiles, this.waterSourceTiles, originalData.waterSourceTiles.Length);
        this.refillTime = originalData.refillTime;
    }

    public override bool CanUse()
    {
        return currentCapacity > 0;
    }

    public override bool Use()
    {
        if (currentCapacity <= 0) return false;
        currentCapacity--;
        return true;
    }

    public bool CanWater()
    {
        return currentCapacity > 0;
    }

    public bool UseWater()
    {
        if (currentCapacity <= 0) return false;
        currentCapacity--;
        return true;
    }

    public void Refill()
    {
        currentCapacity = maxCapacity;
    }

    public float GetCapacityPercentage()
    {
        return (float)currentCapacity / maxCapacity;
    }
}

[System.Serializable]
public class ToolInstance : ItemInstance
{
    public int maxDurability;
    public int currentDurability;
    public int staminaCost;
    public int damage;

    public ToolInstance(ItemData originalData, int maxDurability, int staminaCost, int damage = 0) : base(originalData)
    {
        this.maxDurability = maxDurability;
        this.currentDurability = maxDurability;
        this.staminaCost = staminaCost;
        this.damage = damage;
    }

    public override bool CanUse()
    {
        return currentDurability > 0;
    }

    public override bool Use()
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

[System.Serializable]
public class ShovelInstance : ToolInstance
{
    public ShovelInstance(ShovelData originalData) : base(originalData, originalData.maxDurability, originalData.staminaCost)
    {
    }
}

[System.Serializable]
public class AxeInstance : ToolInstance
{
    public AxeInstance(AxeData originalData) : base(originalData, originalData.maxDurability, originalData.staminaCost, originalData.damage)
    {
    }
}

[System.Serializable]
public class PickaxeInstance : ToolInstance
{
    public PickaxeInstance(PickaxeData originalData) : base(originalData, originalData.maxDurability, originalData.staminaCost, originalData.damage)
    {
    }
}

[System.Serializable]
public class SpearInstance : ToolInstance
{
    public float attackRange;

    public SpearInstance(SpearData originalData) : base(originalData, originalData.maxDurability, originalData.staminaCost, originalData.damage)
    {
        this.attackRange = originalData.attackRange;
    }
}

[System.Serializable]
public class SwordInstance : ToolInstance
{
    public float attackRange;
    public float attackRadius;

    public SwordInstance(SwordData originalData) : base(originalData, originalData.maxDurability, originalData.staminaCost, originalData.damage)
    {
        this.attackRange = originalData.attackRange;
        this.attackRadius = originalData.attackRadius;
    }
}

[System.Serializable]
public class BowInstance : ToolInstance
{
    public float attackRange;
    public float arrowSpeed;
    public float accuracy;

    public BowInstance(BowData originalData) : base(originalData, originalData.maxDurability, originalData.staminaCost, originalData.damage)
    {
        this.attackRange = originalData.attackRange;
        this.arrowSpeed = originalData.arrowSpeed;
        this.accuracy = originalData.accuracy;
    }
}

[System.Serializable]
public class HoeInstance : ToolInstance
{
    public HoeInstance(HoeData originalData) : base(originalData, originalData.maxDurability, originalData.staminaCost)
    {
    }
}

[System.Serializable]
public class SeedInstance : ItemInstance
{
    public Sprite[] growthSprites;
    public Sprite witheredSprite;
    public Season growthSeason;
    public ItemData harvestProduct;
    public int harvestQuantity;
    public int witherTime;
    
    // Propriétés pour les cultures pérennes
    public bool isPerennial;
    public int productionInterval;
    public Sprite fruitSprite;

    public SeedInstance(SeedData originalData) : base(originalData)
    {
        this.growthSprites = new Sprite[originalData.growthSprites.Length];
        System.Array.Copy(originalData.growthSprites, this.growthSprites, originalData.growthSprites.Length);
        this.witheredSprite = originalData.witheredSprite;
        this.growthSeason = originalData.growthSeason;
        this.harvestProduct = originalData.harvestProduct;
        this.harvestQuantity = originalData.harvestQuantity;
        this.witherTime = originalData.witherTime;
        
        // Propriétés pérennes
        this.isPerennial = originalData.isPerennial;
        this.productionInterval = originalData.productionInterval;
        this.fruitSprite = originalData.fruitSprite;
    }

    public override bool CanUse()
    {
        return true;
    }
}
