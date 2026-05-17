using System.Collections.Generic;

public sealed class SoilWorld
{
    private readonly HashSet<GridPos> soilCells = new HashSet<GridPos>();
    private readonly HashSet<GridPos> wetSoilCells = new HashSet<GridPos>();

    public IReadOnlyCollection<GridPos> SoilCells => soilCells;
    public IReadOnlyCollection<GridPos> WetSoilCells => wetSoilCells;

    public void RegisterSoil(GridPos pos, bool isWet)
    {
        soilCells.Add(pos);
        if (isWet) wetSoilCells.Add(pos);
        else wetSoilCells.Remove(pos);
    }

    public void Unregister(GridPos pos)
    {
        soilCells.Remove(pos);
        wetSoilCells.Remove(pos);
    }

    public void MarkWet(GridPos pos)
    {
        if (!soilCells.Contains(pos)) return;
        wetSoilCells.Add(pos);
    }

    public void MarkDry(GridPos pos)
    {
        wetSoilCells.Remove(pos);
    }

    public List<GridPos> SnapshotSoils()
    {
        return new List<GridPos>(soilCells);
    }

    public List<GridPos> SnapshotWetSoils()
    {
        return new List<GridPos>(wetSoilCells);
    }
}

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

public sealed class FarmingWorld
{
    public readonly struct CropUpdate
    {
        public readonly GridPos Pos;
        public readonly bool StageChanged;
        public readonly bool WitherChanged;
        public readonly bool FruitsChanged;

        public CropUpdate(GridPos pos, bool stageChanged, bool witherChanged, bool fruitsChanged)
        {
            Pos = pos;
            StageChanged = stageChanged;
            WitherChanged = witherChanged;
            FruitsChanged = fruitsChanged;
        }
    }

    public sealed class CropState
    {
        public CropBlueprint Blueprint;
        public int CurrentStage;
        public int DayPlanted;
        public int LastWateredDay;
        public bool IsWithered;

        public int LastProductionDay;
        public bool HasFruits;

        public CropState(CropBlueprint blueprint, int currentStage, int dayPlanted, int lastWateredDay)
        {
            Blueprint = blueprint;
            CurrentStage = currentStage;
            DayPlanted = dayPlanted;
            LastWateredDay = lastWateredDay;
        }
    }

    private readonly Dictionary<GridPos, CropState> crops = new Dictionary<GridPos, CropState>();

    public IReadOnlyDictionary<GridPos, CropState> Crops => crops;

    public bool HasCrop(GridPos pos) => crops.ContainsKey(pos);

    public bool TryGetCrop(GridPos pos, out CropState crop) => crops.TryGetValue(pos, out crop);

    public bool Plant(GridPos pos, CropBlueprint blueprint, int currentDay)
    {
        if (crops.ContainsKey(pos)) return false;
        crops[pos] = new CropState(blueprint, currentStage: 0, dayPlanted: currentDay, lastWateredDay: currentDay);
        return true;
    }

    public bool Remove(GridPos pos) => crops.Remove(pos);

    public List<CropUpdate> TickDay(int currentDay, int currentSeasonId)
    {
        var updates = new List<CropUpdate>();
        foreach (var kvp in crops)
        {
            GridPos pos = kvp.Key;
            CropState crop = kvp.Value;

            bool stageChanged = false;
            bool witherChanged = false;
            bool fruitsChanged = false;

            if (FarmingCropLogic.ShouldGrow(
                    crop.IsWithered,
                    crop.CurrentStage,
                    crop.Blueprint.GrowthStagesCount,
                    currentDay,
                    crop.DayPlanted))
            {
                crop.CurrentStage++;
                stageChanged = true;
            }

            bool isMature = FarmingCropLogic.IsMature(crop.CurrentStage, crop.Blueprint.GrowthStagesCount);

            if (crop.Blueprint.IsPerennial && isMature)
            {
                bool changed = FarmingCropLogic.TryUpdatePerennialProduction(
                    currentDay,
                    crop.Blueprint.ProductionIntervalDays,
                    ref crop.LastProductionDay,
                    ref crop.HasFruits);
                if (changed) fruitsChanged = true;
            }

            bool isInSeason = crop.Blueprint.GrowthSeasonId == currentSeasonId;
            bool shouldWither = FarmingCropLogic.ShouldWither(
                crop.IsWithered,
                isInSeason,
                isMature,
                currentDay,
                crop.LastWateredDay,
                crop.Blueprint.WitherTimeDays);
            if (shouldWither)
            {
                crop.IsWithered = true;
                witherChanged = true;
            }

            if (stageChanged || witherChanged || fruitsChanged)
            {
                updates.Add(new CropUpdate(pos, stageChanged, witherChanged, fruitsChanged));
            }
        }
        return updates;
    }

    public void WaterAllCrops(int currentDay)
    {
        foreach (var kvp in crops)
        {
            kvp.Value.LastWateredDay = currentDay;
        }
    }

    public void DryPerennialFruits()
    {
        foreach (var kvp in crops)
        {
            if (!kvp.Value.Blueprint.IsPerennial) continue;
            kvp.Value.HasFruits = false;
        }
    }
}

public sealed class CropService
{
    private readonly FarmingWorld farmingWorld = new FarmingWorld();

    public IReadOnlyDictionary<GridPos, FarmingWorld.CropState> Crops => farmingWorld.Crops;

    public bool TryGetCrop(GridPos pos, out FarmingWorld.CropState crop) => farmingWorld.TryGetCrop(pos, out crop);

    public bool Plant(GridPos pos, CropBlueprint blueprint, int currentDay) => farmingWorld.Plant(pos, blueprint, currentDay);

    public void RestoreCrop(
        GridPos pos,
        CropBlueprint blueprint,
        int currentStage,
        int dayPlanted,
        int lastWateredDay,
        bool isWithered,
        int lastProductionDay,
        bool hasFruits)
    {
        // Ensure crop exists then patch its state.
        if (!farmingWorld.HasCrop(pos))
        {
            int plantedDay = dayPlanted > 0 ? dayPlanted : 1;
            farmingWorld.Plant(pos, blueprint, plantedDay);
        }

        if (farmingWorld.TryGetCrop(pos, out var crop) && crop != null)
        {
            crop.Blueprint = blueprint;
            crop.CurrentStage = currentStage;
            crop.DayPlanted = dayPlanted;
            crop.LastWateredDay = lastWateredDay;
            crop.IsWithered = isWithered;
            crop.LastProductionDay = lastProductionDay;
            crop.HasFruits = hasFruits;
        }
    }

    public bool Remove(GridPos pos) => farmingWorld.Remove(pos);

    public List<FarmingWorld.CropUpdate> TickDay(int currentDay, int currentSeasonId) => farmingWorld.TickDay(currentDay, currentSeasonId);

    public void WaterAllCrops(int currentDay) => farmingWorld.WaterAllCrops(currentDay);

    public void DryPerennialFruits() => farmingWorld.DryPerennialFruits();
}
