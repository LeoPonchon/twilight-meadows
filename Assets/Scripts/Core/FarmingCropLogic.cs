public static class FarmingCropLogic
{
    public static bool IsMature(int currentStage, int growthStagesCount)
    {
        return growthStagesCount > 0 && currentStage >= growthStagesCount - 1;
    }

    public static bool ShouldGrow(bool isWithered, int currentStage, int growthStagesCount, int currentDay, int dayPlanted)
    {
        if (isWithered) return false;
        if (growthStagesCount <= 0) return false;
        if (currentStage >= growthStagesCount - 1) return false;

        int daysSincePlanted = currentDay - dayPlanted;
        int requiredDays = currentStage + 1;
        return daysSincePlanted >= requiredDays;
    }

    public static bool ShouldWither(
        bool isWithered,
        bool isInGrowthSeason,
        bool isMature,
        int currentDay,
        int lastWateredDay,
        int witherTimeDays)
    {
        if (isWithered) return false;
        if (!isInGrowthSeason) return false;
        if (!isMature) return false;
        if (witherTimeDays <= 0) return false;

        int daysSinceLastWatered = currentDay - lastWateredDay;
        return daysSinceLastWatered >= witherTimeDays;
    }

    public static bool TryUpdatePerennialProduction(
        int currentDay,
        int productionIntervalDays,
        ref int lastProductionDay,
        ref bool hasFruits)
    {
        if (productionIntervalDays <= 0) return false;

        if (lastProductionDay == 0)
        {
            lastProductionDay = currentDay;
            hasFruits = true;
            return true;
        }

        int daysSinceLastProduction = currentDay - lastProductionDay;
        if (daysSinceLastProduction >= productionIntervalDays && !hasFruits)
        {
            lastProductionDay = currentDay;
            hasFruits = true;
            return true;
        }

        return false;
    }
}

