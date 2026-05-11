using UnityEngine;
using System;

/// <summary>
/// Gestionnaire météo qui contrôle la pluie et les orages
/// </summary>
public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("Configuration Météo")]
    [Tooltip("Probabilité de pluie par jour (0-1)")]
    [Range(0f, 1f)]
    public float rainProbability = 0.3f;
    
    [Tooltip("Probabilité d'orage par jour (0-1)")]
    [Range(0f, 1f)]
    public float stormProbability = 0.1f;

    [Header("Effets Visuels")]
    [Tooltip("GameObject contenant le système de particules pour la pluie")]
    public GameObject rainParticleSystem;

    /// <summary>
    /// État météo actuel
    /// </summary>
    public WeatherType CurrentWeather { get; private set; } = WeatherType.Sunny;


    /// <summary>
    /// Événement déclenché quand la météo change
    /// </summary>
    public event Action<WeatherType> OnWeatherChanged;

    private TimeManager timeManager;
    private System.Random weatherRandom;
    private int lastCheckedDay = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        
        // Initialiser le générateur aléatoire avec une seed basée sur la date
        weatherRandom = new System.Random();
    }

    private void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            timeManager.OnDayChanged += OnDayChanged;
        }
        
        // Initialiser les particules (les arrêter au démarrage)
        InitializeParticleEffects();
        
        // Générer la météo pour le jour actuel
        GenerateWeatherForDay(timeManager != null ? timeManager.GetCurrentDay() : 1);
    }

    /// <summary>
    /// Initialise les effets de particules au démarrage
    /// </summary>
    private void InitializeParticleEffects()
    {
        // S'assurer que le GameObject de pluie est désactivé au démarrage
        if (rainParticleSystem != null)
        {
            rainParticleSystem.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnDayChanged -= OnDayChanged;
        }
    }


    private void OnDayChanged(int day)
    {
        if (day != lastCheckedDay)
        {
            lastCheckedDay = day;
            GenerateWeatherForDay(day);
        }
    }

    /// <summary>
    /// Génère la météo pour un jour donné
    /// </summary>
    private void GenerateWeatherForDay(int day)
    {
        // Le premier jour est toujours ensoleillé pour éviter les problèmes d'initialisation
        if (day == 1)
        {
            SetWeather(WeatherType.Sunny);
            return;
        }
        
        // Utiliser le jour comme seed pour avoir une météo cohérente
        weatherRandom = new System.Random(day);
        
        float rainRoll = (float)weatherRandom.NextDouble();
        float stormRoll = (float)weatherRandom.NextDouble();
        
        WeatherType newWeather = WeatherType.Sunny;
        
        // Priorité : Orage > Pluie > Ensoleillé
        if (stormRoll < stormProbability)
        {
            newWeather = WeatherType.Stormy;
        }
        else if (rainRoll < rainProbability)
        {
            newWeather = WeatherType.Rainy;
        }
        
        // La météo dure toute la journée
        SetWeather(newWeather);
    }

    /// <summary>
    /// Définit la météo actuelle
    /// </summary>
    private void SetWeather(WeatherType newWeather)
    {
        if (CurrentWeather != newWeather)
        {
            CurrentWeather = newWeather;
            OnWeatherChanged?.Invoke(newWeather);
            
            // Contrôler les particules selon la météo
            UpdateParticleEffects();
        }
    }

    /// <summary>
    /// Met à jour les effets de particules selon la météo actuelle
    /// </summary>
    private void UpdateParticleEffects()
    {
        // Désactiver le GameObject de pluie d'abord
        if (rainParticleSystem != null)
        {
            rainParticleSystem.SetActive(false);
        }

        // Activer le GameObject selon la météo
        switch (CurrentWeather)
        {
            case WeatherType.Rainy:
            case WeatherType.Stormy:
                if (rainParticleSystem != null)
                {
                    rainParticleSystem.SetActive(true);
                }
                break;
                
            case WeatherType.Sunny:
            default:
                // Aucune particule pour le soleil
                break;
        }
    }

    /// <summary>
    /// Vérifie si la météo actuelle arrose automatiquement les cultures
    /// </summary>
    public bool IsAutoWatering()
    {
        return CurrentWeather == WeatherType.Rainy || CurrentWeather == WeatherType.Stormy;
    }

    /// <summary>
    /// Obtient le bonus de croissance selon la météo
    /// </summary>
    public float GetGrowthBonus()
    {
        // Pas de bonus de croissance, juste l'arrosage automatique
        return 0f;
    }

    /// <summary>
    /// Force un type de météo (pour les tests)
    /// </summary>
    public void ForceWeather(WeatherType weather)
    {
        SetWeather(weather);
    }

    /// <summary>
    /// Obtient l'état actuel de la météo
    /// </summary>
    public WeatherType GetCurrentWeather()
    {
        return CurrentWeather;
    }
}

/// <summary>
/// Types de météo disponibles
/// </summary>
public enum WeatherType
{
    Sunny,      // Ensoleillé
    Rainy,      // Pluvieux
    Stormy      // Orageux
}
