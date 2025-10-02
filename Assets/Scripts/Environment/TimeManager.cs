using TMPro;
using UnityEngine.Rendering;
using UnityEngine;
using System;

public class TimeManager     : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private TextMeshProUGUI dayDisplay;
    [SerializeField] private TextMeshProUGUI seasonDisplay;
    [SerializeField] private TextMeshProUGUI yearDisplay;
    private Volume ppv;

    [SerializeField] private float tick;
    [SerializeField] private float seconds;
    [SerializeField] private int mins;
    [SerializeField] private int hours = 9;
    [SerializeField] private int days = 1;
    [SerializeField] private int seasonId = 1;
    [SerializeField] private int year = 1;

    [SerializeField] private bool activateLights;
    [SerializeField] private GameObject[] lights;

    public event Action<int, string> OnSeasonStarted;
    public event Action<int> OnDayChanged;

    void Start() => ppv = gameObject.GetComponent<Volume>();

    void FixedUpdate()
    {
        CalcTime();
        DisplayTime();
    }

    public void CalcTime()
    {
        seconds += Time.fixedDeltaTime * tick;

        if (seconds >= 60)
        {
            seconds = 0;
            mins += 10;
        }

        if (mins >= 60)
        {
            mins = 0;
            hours += 1;
        }

        bool dayChanged = false;
        if (hours >= 24)
        {
            hours = 0;
            days += 1; // Incr�menter les jours sans retourner � 0
            dayChanged = true;
        }

        bool seasonChanged = false;
        if (days > 28)
        {
            days = 1; // R�initialiser les jours du mois apr�s 28
            seasonId += 1; // Passer � la saison suivante
            seasonChanged = true;
        }

        if (seasonId > 4)
        {
            seasonId = 1; // Retourner au printemps apr�s l'hiver
            year += 1; // Incr�menter l'ann�e
        }

        if (dayChanged)
        {
            OnDayChanged?.Invoke(days);
        }

        if (seasonChanged)
        {
            OnSeasonStarted?.Invoke(seasonId, GetSeasonNameById(seasonId));
        }

        ControlPPV();
    }


    public void ControlPPV()
    {
        if (hours >= 21 && hours < 22) // dusk at 21:00 / 9pm    -   until 22:00 / 10pm
        {
            ppv.weight = (float)mins / 60; // since dusk is 1 hr, we just divide the mins by 60 which will slowly increase from 0 - 1 
        }
        else if (hours >= 6 && hours < 7) // Dawn at 6:00 / 6am    -   until 7:00 / 7am
        {
            ppv.weight = 1 - (float)mins / 60; // we minus 1 because we want it to go from 1 - 0
        }
        ControlLights();
    }

    void ControlLights()
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
        string dayNameToDisplay = (days % 7) switch { 1 => "Mon", 2 => "Tue", 3 => "Wed", 4 => "Thu", 5 => "Fri", 6 => "Sat", 0 => "Sun", _ => "" };
        string seasonToDisplay = seasonId switch { 1 => "Spring", 2 => "Summer", 3 => "Autumn", 4 => "Winter", _ => "" };

        timeDisplay.text = string.Format("{0:00}:{1:00}", hours, mins);
        dayDisplay.text = $"{dayNameToDisplay} {days}";
        seasonDisplay.text = seasonToDisplay;
        yearDisplay.text = "Year " + year;
    }

    private string GetSeasonNameById(int id)
    {
        switch (id)
        {
            case 1: return "Spring";
            case 2: return "Summer";
            case 3: return "Autumn";
            case 4: return "Winter";
            default: return string.Empty;
        }
    }

    public int GetCurrentHour()
    {
        return hours;
    }

    internal int GetCurrentMins()
    {
        return mins;
    }

    internal int GetCurrentDay()
    {
        return days;
    }

    internal string GetCurrentSeason()
    {
        return seasonDisplay.text;
    }

    internal int GetCurrentSeasonId()
    {
        return seasonId;
    }
}
