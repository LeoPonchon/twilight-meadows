using System;
using UnityEngine;

/// <summary>
/// Gestionnaire météo qui contrôle la pluie et les orages.
/// Dépend d'un TimeManager (référence explicite ou fallback).
/// </summary>
public class WeatherManager : MonoBehaviour
{
    [Header("Configuration Météo")]
    [Tooltip("Probabilité de pluie par jour (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float rainProbability = 0.3f;

    [Tooltip("Probabilité d'orage par jour (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float stormProbability = 0.1f;

    [Header("Effets Visuels")]
    [Tooltip("GameObject contenant le système de particules pour la pluie")]
    [SerializeField] private GameObject rainParticleSystem;

    [Header("References")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private SceneContext sceneContext;

    public WeatherType CurrentWeather { get; private set; } = WeatherType.Sunny;

    public event Action<WeatherType> OnWeatherChanged;

    private WeatherSystem weatherSystem;
    private int lastCheckedDay = -1;

    private void Awake()
    {
        weatherSystem = new WeatherSystem(rainProbability, stormProbability);
    }

    private void Start()
    {
        if (sceneContext == null)
        {
            sceneContext = FindObjectOfType<SceneContext>();
        }
        if (sceneContext == null)
        {
            Debug.LogError("WeatherManager: Missing SceneContext in scene.", this);
            enabled = false;
            return;
        }

        if (timeManager == null)
        {
            timeManager = sceneContext.GetRequired<TimeManager>(this, nameof(timeManager));
        }

        if (timeManager != null)
        {
            timeManager.OnDayChanged += OnDayChanged;
        }

        InitializeParticleEffects();
        GenerateWeatherForDay(timeManager != null ? timeManager.GetCurrentDay() : 1);
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnDayChanged -= OnDayChanged;
        }
    }

    private void InitializeParticleEffects()
    {
        if (rainParticleSystem != null)
        {
            rainParticleSystem.SetActive(false);
        }
    }

    private void OnDayChanged(int day)
    {
        if (day == lastCheckedDay) return;
        lastCheckedDay = day;
        GenerateWeatherForDay(day);
    }

    private void GenerateWeatherForDay(int day)
    {
        SetWeather(weatherSystem != null ? weatherSystem.GenerateForDay(day) : WeatherType.Sunny);
    }

    private void SetWeather(WeatherType newWeather)
    {
        if (CurrentWeather == newWeather) return;
        CurrentWeather = newWeather;
        OnWeatherChanged?.Invoke(newWeather);
        UpdateParticleEffects();
    }

    private void UpdateParticleEffects()
    {
        if (rainParticleSystem != null)
        {
            rainParticleSystem.SetActive(false);
        }

        switch (CurrentWeather)
        {
            case WeatherType.Rainy:
            case WeatherType.Stormy:
                if (rainParticleSystem != null) rainParticleSystem.SetActive(true);
                break;
            case WeatherType.Sunny:
            default:
                break;
        }
    }

    public bool IsAutoWatering()
    {
        return CurrentWeather == WeatherType.Rainy || CurrentWeather == WeatherType.Stormy;
    }

    public void ForceWeather(WeatherType weather)
    {
        SetWeather(weather);
    }
}
