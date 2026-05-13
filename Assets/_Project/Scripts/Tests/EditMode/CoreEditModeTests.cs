using NUnit.Framework;

public sealed class GameClockTests
{
    [Test]
    public void TickFixed_AdvancesTime_AndDay()
    {
        var c = new GameClock(startingHours: 23, startingDay: 1, startingSeasonId: 1, startingYear: 1);
        c.TickFixed(60f, 1f);
        Assert.AreEqual(0, c.Hours);
        Assert.AreEqual(2, c.Day);
    }
}

public sealed class MovementRulesTests
{
    [Test]
    public void Speed_UsesSprintBeforeWalk()
    {
        Assert.AreEqual(10f, MovementRules.Speed(5f, 2f, 0.5f, sprinting: true, walking: true));
    }

    [Test]
    public void Speed_ReturnsWalkSpeed_WhenWalking()
    {
        Assert.AreEqual(2.5f, MovementRules.Speed(5f, 2f, 0.5f, sprinting: false, walking: true));
    }
}

public sealed class WeatherSystemTests
{
    [Test]
    public void GenerateForDay_Day1_IsAlwaysSunny()
    {
        var sys = new WeatherSystem(1f, 1f);
        Assert.AreEqual(WeatherType.Sunny, sys.GenerateForDay(1));
    }
}

public sealed class GoldWalletTests
{
    [Test]
    public void Spend_ReturnsFalse_WhenNotEnoughGold()
    {
        var w = new GoldWallet(startingGold: 0);
        Assert.IsFalse(w.Spend(1));
    }
}

public sealed class InventoryCoreTests
{
    private sealed class Item
    {
        public Item(string id, bool stackable)
        {
            Id = id;
            Stackable = stackable;
        }

        public string Id { get; }
        public bool Stackable { get; }
    }

    private sealed class Stack
    {
        public Stack(Item item, int qty)
        {
            Item = item;
            Quantity = qty;
        }

        public Item Item { get; }
        public int Quantity { get; set; }
    }

    [Test]
    public void Add_Stacks_WhenItemIsStackable()
    {
        var item = new Item("apple", stackable: true);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 2,
            inventorySlots: 2,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 1);
        inv.Add(item, 2);

        Assert.AreEqual(1, inv.Slots.Count);
        Assert.AreEqual(3, inv.GetQuantityTotal(item));
    }

    [Test]
    public void Add_PrefersInventoryRange_BeforeHotbarFallback()
    {
        var item = new Item("stone", stackable: false);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 2,
            inventorySlots: 2,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 1);
        Assert.IsNotNull(inv.GetInSlot(2));
    }

    [Test]
    public void Remove_RemovesSlot_WhenQuantityReachesZero()
    {
        var item = new Item("apple", stackable: true);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 1,
            inventorySlots: 1,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 2);
        inv.Remove(item, 2);

        Assert.AreEqual(0, inv.Slots.Count);
    }

    [Test]
    public void Remove_DecrementsQuantity_WhenQuantityRemains()
    {
        var item = new Item("apple", stackable: true);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 1,
            inventorySlots: 1,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 5);
        inv.Remove(item, 2);

        Assert.AreEqual(1, inv.Slots.Count);
        Assert.AreEqual(3, inv.GetQuantityTotal(item));
    }
}

public sealed class FarmingCropLogicTests
{
    [Test]
    public void ShouldGrow_False_WhenWithered()
    {
        Assert.IsFalse(FarmingCropLogic.ShouldGrow(true, 0, 3, currentDay: 2, dayPlanted: 1));
    }

    [Test]
    public void ShouldGrow_True_WhenEnoughDaysPassed()
    {
        Assert.IsTrue(FarmingCropLogic.ShouldGrow(false, currentStage: 0, growthStagesCount: 3, currentDay: 2, dayPlanted: 1));
    }

    [Test]
    public void ShouldWither_True_WhenMatureAndNotWateredLongEnough()
    {
        Assert.IsTrue(FarmingCropLogic.ShouldWither(
            isWithered: false,
            isInGrowthSeason: true,
            isMature: true,
            currentDay: 10,
            lastWateredDay: 7,
            witherTimeDays: 3));
    }

    [Test]
    public void TryUpdatePerennialProduction_FirstTime_SetsFruits()
    {
        int lastProd = 0;
        bool hasFruits = false;
        bool changed = FarmingCropLogic.TryUpdatePerennialProduction(5, 2, ref lastProd, ref hasFruits);
        Assert.IsTrue(changed);
        Assert.AreEqual(5, lastProd);
        Assert.IsTrue(hasFruits);
    }
}

public sealed class FarmingWorldTests
{
    [Test]
    public void Plant_AddsCrop()
    {
        var world = new FarmingWorld();
        var bp = new CropBlueprint(growthStagesCount: 3, growthSeasonId: 1, witherTimeDays: 2, isPerennial: false, productionIntervalDays: 0);
        Assert.IsTrue(world.Plant(new GridPos(1, 2), bp, currentDay: 1));
        Assert.IsTrue(world.HasCrop(new GridPos(1, 2)));
    }

    [Test]
    public void TickDay_GrowsStage_WhenEnoughDaysPassed()
    {
        var world = new FarmingWorld();
        var bp = new CropBlueprint(growthStagesCount: 3, growthSeasonId: 1, witherTimeDays: 2, isPerennial: false, productionIntervalDays: 0);
        world.Plant(new GridPos(0, 0), bp, currentDay: 1);

        var updates = world.TickDay(currentDay: 2, currentSeasonId: 1);
        Assert.AreEqual(1, updates.Count);
        Assert.IsTrue(world.TryGetCrop(new GridPos(0, 0), out var crop));
        Assert.AreEqual(1, crop.CurrentStage);
    }

    [Test]
    public void WaterAllCrops_UpdatesLastWateredDay()
    {
        var world = new FarmingWorld();
        var bp = new CropBlueprint(growthStagesCount: 2, growthSeasonId: 1, witherTimeDays: 2, isPerennial: false, productionIntervalDays: 0);
        world.Plant(new GridPos(0, 0), bp, currentDay: 1);
        world.WaterAllCrops(currentDay: 5);
        Assert.IsTrue(world.TryGetCrop(new GridPos(0, 0), out var crop));
        Assert.AreEqual(5, crop.LastWateredDay);
    }
}

public sealed class SoilWorldTests
{
    [Test]
    public void RegisterSoil_AddsToSoilCells()
    {
        var w = new SoilWorld();
        w.RegisterSoil(new GridPos(1, 1), isWet: false);
        Assert.AreEqual(1, w.SoilCells.Count);
    }
}

public sealed class ShopRulesTests
{
    [Test]
    public void CanBuy_False_WhenNotEnoughGold()
    {
        Assert.IsFalse(ShopRules.CanBuy(playerGold: 5, buyPrice: 10, hasStock: true, inventoryCanAdd: true));
    }

    [Test]
    public void CanBuy_True_WhenAllConditionsMet()
    {
        Assert.IsTrue(ShopRules.CanBuy(playerGold: 10, buyPrice: 10, hasStock: true, inventoryCanAdd: true));
    }
}
