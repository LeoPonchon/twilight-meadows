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

