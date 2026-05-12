using System.Collections.Generic;

public sealed class CropService
{
    private readonly FarmingWorld farmingWorld = new FarmingWorld();

    public IReadOnlyDictionary<GridPos, FarmingWorld.CropState> Crops => farmingWorld.Crops;

    public bool TryGetCrop(GridPos pos, out FarmingWorld.CropState crop) => farmingWorld.TryGetCrop(pos, out crop);

    public bool Plant(GridPos pos, CropBlueprint blueprint, int currentDay) => farmingWorld.Plant(pos, blueprint, currentDay);

    public bool Remove(GridPos pos) => farmingWorld.Remove(pos);

    public List<FarmingWorld.CropUpdate> TickDay(int currentDay, int currentSeasonId) => farmingWorld.TickDay(currentDay, currentSeasonId);

    public void WaterAllCrops(int currentDay) => farmingWorld.WaterAllCrops(currentDay);

    public void DryPerennialFruits() => farmingWorld.DryPerennialFruits();
}

