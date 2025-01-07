using TMPro;
using UnityEngine.Rendering;
using UnityEngine;
using System;

public class DayNightCycle : MonoBehaviour
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
    [SerializeField] private int dayNameId = 1;
    [SerializeField] private int seasonId = 1;
    [SerializeField] private int year = 1;

    [SerializeField] private bool activateLights;
    [SerializeField] private GameObject[] lights;

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

        if (hours >= 24)
        {
            hours = 0;
            days += 1;
            dayNameId += 1;
        }
        if (dayNameId > 7) dayNameId = 1;
        if (days >= 28)
        {
            days = 1;
            seasonId += 1;
        }

        if (seasonId >= 4)
        {
            year += 1;
            seasonId = 1;
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
        string dayNameToDisplay = dayNameId switch { 1 => "Mon", 2 => "Tue", 3 => "Wed", 4 => "Thu", 5 => "Fri", 6 => "Sat", 7 => "Sun", _ => "" };
        string seasonToDisplay = seasonId switch { 1 => "Spring", 2 => "Summer", 3 => "Autumn", 4 => "Winter", _ => "" };

        timeDisplay.text = string.Format("{0:00}:{1:00}", hours, mins);
        dayDisplay.text = $"{dayNameToDisplay} {dayNameId}";
        seasonDisplay.text = seasonToDisplay;
        yearDisplay.text = "Year " + year;
    }

    public int GetCurrentHour()
    {
        return hours;
    }

    internal int GetCurrentMins()
    {
        return mins;
    }
}
