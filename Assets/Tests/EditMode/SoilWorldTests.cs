using NUnit.Framework;

public class SoilWorldTests
{
    [Test]
    public void RegisterSoil_AddsToSoils()
    {
        var world = new SoilWorld();
        world.RegisterSoil(new GridPos(1, 2), isWet: false);
        Assert.AreEqual(1, world.SoilCells.Count);
        Assert.AreEqual(0, world.WetSoilCells.Count);
    }

    [Test]
    public void MarkWet_OnlyWorksForRegisteredSoil()
    {
        var world = new SoilWorld();
        world.MarkWet(new GridPos(1, 2));
        Assert.AreEqual(0, world.WetSoilCells.Count);

        world.RegisterSoil(new GridPos(1, 2), isWet: false);
        world.MarkWet(new GridPos(1, 2));
        Assert.AreEqual(1, world.WetSoilCells.Count);
    }

    [Test]
    public void Unregister_RemovesFromBothSets()
    {
        var world = new SoilWorld();
        var pos = new GridPos(1, 2);
        world.RegisterSoil(pos, isWet: true);
        world.Unregister(pos);
        Assert.AreEqual(0, world.SoilCells.Count);
        Assert.AreEqual(0, world.WetSoilCells.Count);
    }
}

