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

