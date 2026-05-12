using NUnit.Framework;

public class FarmingWorldTests
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

