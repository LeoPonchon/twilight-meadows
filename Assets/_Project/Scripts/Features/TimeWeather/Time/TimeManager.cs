using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TimeManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private TextMeshProUGUI dayDisplay;
    [SerializeField] private TextMeshProUGUI seasonDisplay;
    [SerializeField] private TextMeshProUGUI yearDisplay;

    [Header("Time Settings")]
    [SerializeField] private float tick = 1f;
    [SerializeField] private int startingHours = 9;
    [SerializeField] private int startingDay = 1;
    [SerializeField] private int startingSeasonId = 1;
    [SerializeField] private int startingYear = 1;

    [Header("Lighting")]
    [SerializeField] private bool activateLights;
    [SerializeField] private GameObject[] lights;

    [Header("References")]
    [SerializeField] private SceneContext sceneContext;

    public event Action<int, string> OnSeasonStarted;
    public event Action<int> OnDayChanged;

    private Volume ppv;
    private GameClock clock;

    private void Start()
    {
        ppv = GetComponent<Volume>();
        if (sceneContext == null)
        {
            sceneContext = FindObjectOfType<SceneContext>();
        }
        if (sceneContext == null)
        {
            Debug.LogError("TimeManager: Missing SceneContext in scene.", this);
            enabled = false;
            return;
        }
        clock = new GameClock(startingHours, startingDay, startingSeasonId, startingYear);
        clock.DayChanged += HandleDayChanged;
        clock.SeasonStarted += HandleSeasonStarted;
    }

    private void OnDestroy()
    {
        if (clock != null)
        {
            clock.DayChanged -= HandleDayChanged;
            clock.SeasonStarted -= HandleSeasonStarted;
        }
    }

    private void FixedUpdate()
    {
        if (clock == null) return;
        clock.TickFixed(Time.fixedDeltaTime, tick);
        ControlPPV();
        DisplayTime();
    }

    private void HandleDayChanged(int day)
    {
        OnDayChanged?.Invoke(day);
    }

    private void HandleSeasonStarted(int seasonId, string seasonName)
    {
        OnSeasonStarted?.Invoke(seasonId, seasonName);
    }

    public void ControlPPV()
    {
        if (ppv == null) return;

        float baseDayWeight = 0f;

        int hours = clock.Hours;
        int mins = clock.Minutes;

        if (hours >= 18 && hours < 24)
        {
            float progress = ((hours - 18) * 60 + mins) / (6f * 60f);
            ppv.weight = Mathf.Lerp(baseDayWeight, 1f, progress);
        }
        else if (hours >= 0 && hours < 6)
        {
            ppv.weight = 1f;
        }
        else if (hours >= 6 && hours < 12)
        {
            float progress = ((hours - 6) * 60 + mins) / (6f * 60f);
            ppv.weight = Mathf.Lerp(1f, baseDayWeight, progress);
        }
        else
        {
            ppv.weight = baseDayWeight;
        }

        ControlLights(hours, mins);
    }

    private void ControlLights(int hours, int mins)
    {
        if (hours >= 21 && hours <= 22 && !activateLights && mins > 45)
        {
            foreach (var light in lights) light.SetActive(true);
            activateLights = true;
        }
        if (hours >= 6 && hours < 7 && activateLights && mins > 45)
        {
            foreach (var light in lights) light.SetActive(false);
            activateLights = false;
        }
    }

    public void DisplayTime()
    {
        if (timeDisplay == null || dayDisplay == null || seasonDisplay == null || yearDisplay == null) return;

        int hours = clock.Hours;
        int mins = clock.Minutes;
        int days = clock.Day;
        int seasonId = clock.SeasonId;
        int year = clock.Year;

        int absoluteDayIndex = ((year - 1) * 112) + ((seasonId - 1) * 28) + (days - 1);
        string dayNameToDisplay = (absoluteDayIndex % 7) switch { 0 => "Mon", 1 => "Tue", 2 => "Wed", 3 => "Thu", 4 => "Fri", 5 => "Sat", 6 => "Sun", _ => "" };
        string seasonToDisplay = seasonId switch { 1 => "Spring", 2 => "Summer", 3 => "Autumn", 4 => "Winter", _ => "" };

        timeDisplay.text = string.Format("{0:00}:{1:00}", hours, mins);
        dayDisplay.text = $"{dayNameToDisplay} {days}";
        seasonDisplay.text = seasonToDisplay;
        yearDisplay.text = "Year " + year;
    }

    public int GetCurrentHour() => clock != null ? clock.Hours : startingHours;

    public int GetCurrentMins() => clock != null ? clock.Minutes : 0;

    public int GetCurrentDay() => clock != null ? clock.Day : startingDay;

    public string GetCurrentSeason() => seasonDisplay != null ? seasonDisplay.text : GameClock.GetSeasonNameById(startingSeasonId);

    public int GetCurrentSeasonId() => clock != null ? clock.SeasonId : startingSeasonId;

    public int GetCurrentYear() => clock != null ? clock.Year : startingYear;

    public void SetTimeState(int hour, int minute, int day, int seasonId, int year)
    {
        if (clock == null)
        {
            clock = new GameClock(startingHours, startingDay, startingSeasonId, startingYear);
            clock.DayChanged += HandleDayChanged;
            clock.SeasonStarted += HandleSeasonStarted;
        }

        clock.SetState(hour, minute, day, seasonId, year);
        DisplayTime();
    }

    public void AdvanceDays(int days)
    {
        if (days <= 0) return;

        if (clock == null)
        {
            clock = new GameClock(startingHours, startingDay, startingSeasonId, startingYear);
            clock.DayChanged += HandleDayChanged;
            clock.SeasonStarted += HandleSeasonStarted;
        }

        clock.AdvanceDays(days);
        DisplayTime();
    }

    public void SetTimeOfDay(int hour, int minute)
    {
        if (clock == null)
        {
            clock = new GameClock(startingHours, startingDay, startingSeasonId, startingYear);
            clock.DayChanged += HandleDayChanged;
            clock.SeasonStarted += HandleSeasonStarted;
        }

        clock.SetState(hour, minute, clock.Day, clock.SeasonId, clock.Year);
        DisplayTime();
    }
}
