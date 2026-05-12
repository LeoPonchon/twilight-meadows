using System;

public sealed class WeatherSystem
{
    private readonly float rainProbability;
    private readonly float stormProbability;

    public WeatherSystem(float rainProbability, float stormProbability)
    {
        this.rainProbability = Clamp01(rainProbability);
        this.stormProbability = Clamp01(stormProbability);
    }

    public WeatherType GenerateForDay(int day)
    {
        if (day <= 1) return WeatherType.Sunny;

        var rng = new Random(day);
        float rainRoll = (float)rng.NextDouble();
        float stormRoll = (float)rng.NextDouble();

        if (stormRoll < stormProbability) return WeatherType.Stormy;
        if (rainRoll < rainProbability) return WeatherType.Rainy;
        return WeatherType.Sunny;
    }

    private static float Clamp01(float v)
    {
        if (v < 0f) return 0f;
        if (v > 1f) return 1f;
        return v;
    }
}

