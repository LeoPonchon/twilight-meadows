using System.Collections.Generic;

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

