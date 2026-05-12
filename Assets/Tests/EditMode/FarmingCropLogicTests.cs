using NUnit.Framework;

public class FarmingCropLogicTests
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

