using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)]
public sealed class AutoSaveOnDayChange : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool autoSaveOnDayChanged = true;

    [Header("References (optional)")]
    [SerializeField] private SceneContext sceneContext;

    private TimeManager timeManager;
    private EconomyManager economyManager;

    private void Awake()
    {
        if (sceneContext == null)
        {
            sceneContext = FindObjectOfType<SceneContext>();
        }
    }

    private void Start()
    {
        if (sceneContext == null)
        {
            Debug.LogError("AutoSaveOnDayChange: Missing SceneContext in scene.", this);
            enabled = false;
            return;
        }

        timeManager = sceneContext.GetRequired<TimeManager>(this, "TimeManager");
        economyManager = sceneContext.Get<EconomyManager>();

        if (timeManager == null)
        {
            enabled = false;
            return;
        }

        if (loadOnStart)
        {
            TryLoadActiveSlot();
        }

        if (autoSaveOnDayChanged)
        {
            timeManager.OnDayChanged += HandleDayChanged;
        }
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnDayChanged -= HandleDayChanged;
        }
    }

    private void HandleDayChanged(int day)
    {
        SaveNow();
    }

    private void TryLoadActiveSlot()
    {
        if (!SaveSlots.TryGetActive(out var active))
        {
            return;
        }

        if (!File.Exists(active.FilePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(active.FilePath);
            if (string.IsNullOrWhiteSpace(json)) return;

            var data = JsonUtility.FromJson<GameSaveData>(json);
            if (data == null) return;

            ApplyLoadedData(data);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"AutoSaveOnDayChange: Failed to load save '{active.FilePath}': {ex.Message}", this);
        }
    }

    private void ApplyLoadedData(GameSaveData data)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition;
        }

        if (economyManager != null)
        {
            economyManager.SetGold(data.gold);
        }

        timeManager.SetTimeState(
            hour: data.hour,
            minute: data.minute,
            day: data.day,
            seasonId: data.seasonId,
            year: data.year);
    }

    public void SaveNow()
    {
        var active = SaveSlots.GetOrCreateActive();

        var player = GameObject.FindGameObjectWithTag("Player");
        var playerPos = player != null ? player.transform.position : Vector3.zero;

        var data = new GameSaveData
        {
            slotId = active.SlotId,
            savedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            sceneName = SceneManager.GetActiveScene().name,
            playerPosition = playerPos,
            gold = economyManager != null ? economyManager.Gold : 0,
            day = timeManager.GetCurrentDay(),
            seasonId = timeManager.GetCurrentSeasonId(),
            year = timeManager.GetCurrentYear(),
            hour = timeManager.GetCurrentHour(),
            minute = timeManager.GetCurrentMins(),
        };

        try
        {
            SaveSlots.EnsureSaveDirectoryExists();
            File.WriteAllText(active.FilePath, JsonUtility.ToJson(data, prettyPrint: true));
            SaveSlots.TouchLastPlayed(active.SlotId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"AutoSaveOnDayChange: Failed to save '{active.FilePath}': {ex.Message}", this);
        }
    }

    
}
