public readonly struct CropBlueprint
{
    public readonly int GrowthStagesCount;
    public readonly int GrowthSeasonId;
    public readonly int WitherTimeDays;
    public readonly bool IsPerennial;
    public readonly int ProductionIntervalDays;

    public CropBlueprint(int growthStagesCount, int growthSeasonId, int witherTimeDays, bool isPerennial, int productionIntervalDays)
    {
        GrowthStagesCount = growthStagesCount;
        GrowthSeasonId = growthSeasonId;
        WitherTimeDays = witherTimeDays;
        IsPerennial = isPerennial;
        ProductionIntervalDays = productionIntervalDays;
    }
}

