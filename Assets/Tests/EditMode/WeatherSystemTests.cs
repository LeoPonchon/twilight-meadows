using NUnit.Framework;

public class WeatherSystemTests
{
    [Test]
    public void GenerateForDay_IsDeterministic_ForSameDay()
    {
        var system = new WeatherSystem(rainProbability: 0.3f, stormProbability: 0.1f);
        var a = system.GenerateForDay(10);
        var b = system.GenerateForDay(10);
        Assert.AreEqual(a, b);
    }

    [Test]
    public void Day1_IsAlwaysSunny()
    {
        var system = new WeatherSystem(rainProbability: 1f, stormProbability: 1f);
        Assert.AreEqual(WeatherType.Sunny, system.GenerateForDay(1));
    }
}

