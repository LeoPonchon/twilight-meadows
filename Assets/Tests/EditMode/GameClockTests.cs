using NUnit.Framework;

public class GameClockTests
{
    [Test]
    public void TickFixed_AdvancesTime_In10MinuteSteps()
    {
        var clock = new GameClock(startingHours: 9, startingDay: 1, startingSeasonId: 1, startingYear: 1);

        // 60 seconds of accumulated fixed time should advance +10 minutes once.
        for (int i = 0; i < 60; i++)
        {
            clock.TickFixed(1f, 1f);
        }

        Assert.AreEqual(9, clock.Hours);
        Assert.AreEqual(10, clock.Minutes);
    }

    [Test]
    public void DayChange_FiresEvent_WhenWrapping24Hours()
    {
        var clock = new GameClock(startingHours: 23, startingDay: 1, startingSeasonId: 1, startingYear: 1);
        int dayEvent = -1;
        clock.DayChanged += d => dayEvent = d;

        // Need 6 increments of +10 minutes to go from 23:00 to 24:00 and wrap.
        for (int step = 0; step < 6; step++)
        {
            for (int i = 0; i < 60; i++) clock.TickFixed(1f, 1f);
        }

        Assert.AreEqual(0, clock.Hours);
        Assert.AreEqual(0, clock.Minutes);
        Assert.AreEqual(2, clock.Day);
        Assert.AreEqual(2, dayEvent);
    }
}

