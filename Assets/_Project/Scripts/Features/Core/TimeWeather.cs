using System;

public sealed class GameClock
{
    public int Minutes { get; private set; }
    public int Hours { get; private set; }
    public int Day { get; private set; }
    public int SeasonId { get; private set; }
    public int Year { get; private set; }

    public event Action<int> DayChanged;
    public event Action<int, string> SeasonStarted;

    private float seconds;

    public GameClock(int startingHours = 9, int startingDay = 1, int startingSeasonId = 1, int startingYear = 1)
    {
        Hours = Clamp(startingHours, 0, 23);
        Day = Math.Max(1, startingDay);
        SeasonId = Clamp(startingSeasonId, 1, 4);
        Year = Math.Max(1, startingYear);
    }

    public void SetState(int hours, int minutes, int day, int seasonId, int year)
    {
        Hours = Clamp(hours, 0, 23);
        Minutes = Clamp(minutes, 0, 50);
        Minutes = (Minutes / 10) * 10;
        Day = Math.Max(1, day);
        SeasonId = Clamp(seasonId, 1, 4);
        Year = Math.Max(1, year);
        seconds = 0f;
    }

    public void TickFixed(float fixedDeltaTimeSeconds, float tickMultiplier)
    {
        if (fixedDeltaTimeSeconds <= 0f || tickMultiplier <= 0f) return;

        seconds += fixedDeltaTimeSeconds * tickMultiplier;

        if (seconds < 60f) return;

        seconds = 0f;
        Minutes += 10;

        if (Minutes < 60) return;

        Minutes = 0;
        Hours += 1;

        if (Hours < 24) return;

        Hours = 0;
        AdvanceDay();
    }

    public void AdvanceDays(int days)
    {
        if (days <= 0) return;
        for (int i = 0; i < days; i++)
        {
            AdvanceDay();
        }
    }

    private void AdvanceDay()
    {
        Day += 1;
        bool seasonChanged = false;

        if (Day > 28)
        {
            Day = 1;
            SeasonId += 1;
            seasonChanged = true;
        }

        if (SeasonId > 4)
        {
            SeasonId = 1;
            Year += 1;
        }

        DayChanged?.Invoke(Day);
        if (seasonChanged)
        {
            SeasonStarted?.Invoke(SeasonId, GetSeasonNameById(SeasonId));
        }
    }

    public static string GetSeasonNameById(int id)
    {
        return id switch
        {
            1 => "Spring",
            2 => "Summer",
            3 => "Autumn",
            4 => "Winter",
            _ => string.Empty
        };
    }

    private static int Clamp(int v, int min, int max)
    {
        if (v < min) return min;
        if (v > max) return max;
        return v;
    }
}

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
