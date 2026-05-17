using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)]
public sealed class AutoSaveOnDayChange : MonoBehaviour, ISaveGameService
{
    [Header("Behavior")]
    [SerializeField] private bool loadOnStart = true;

    [Header("References (optional)")]
    [SerializeField] private SceneContext sceneContext;

    private TimeManager timeManager;
    private EconomyManager economyManager;
    private IWorldSaveService worldSaveService;

    private void Awake()
    {
        if (sceneContext == null)
        {
            sceneContext = FindObjectOfType<SceneContext>();
        }
    }

    private void Start()
    {
        SaveLoadState.ResetForNewSession();

        if (sceneContext == null)
        {
            Debug.LogError("AutoSaveOnDayChange: Missing SceneContext in scene.", this);
            enabled = false;
            return;
        }

        timeManager = sceneContext.GetRequired<TimeManager>(this, "TimeManager");
        economyManager = sceneContext.Get<EconomyManager>();
        worldSaveService = sceneContext.Get<IWorldSaveService>();
        if (worldSaveService == null)
        {
            // Fallback when SceneContext isn't wired (dev-friendly).
            var behaviours = FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IWorldSaveService svc)
                {
                    worldSaveService = svc;
                    break;
                }
            }
        }

        if (timeManager == null)
        {
            enabled = false;
            return;
        }

        if (loadOnStart)
        {
            TryLoadActiveSlot();
        }
    }

    private void OnDestroy()
    {
        // Intentionally no autosave subscription: saving is triggered only on new game creation and on sleep/faint.
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
        bool canApplyPosition = data.hasPlayerPosition || data.playerPosition != Vector3.zero;
        if (player != null && canApplyPosition)
        {
            player.transform.position = data.playerPosition;
        }

        if (economyManager != null)
        {
            economyManager.SetGold(data.gold);
        }

        if (data.day <= 0 || data.seasonId <= 0 || data.year <= 0)
        {
            return;
        }

        timeManager.SetTimeState(
            hour: data.hour,
            minute: data.minute,
            day: data.day,
            seasonId: data.seasonId,
            year: data.year);

        if (worldSaveService != null && data.world != null)
        {
            worldSaveService.ApplyWorld(data.world);
            SaveLoadState.HasLoadedWorldState = true;
        }
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
            hasPlayerPosition = player != null,
            gold = economyManager != null ? economyManager.Gold : 0,
            day = timeManager.GetCurrentDay(),
            seasonId = timeManager.GetCurrentSeasonId(),
            year = timeManager.GetCurrentYear(),
            hour = timeManager.GetCurrentHour(),
            minute = timeManager.GetCurrentMins(),
            world = worldSaveService != null ? worldSaveService.CaptureWorld() : null,
        };

        try
        {
            SaveSlots.EnsureSaveDirectoryExists();
            File.WriteAllText(active.FilePath, JsonUtility.ToJson(data, prettyPrint: true));
            SaveSlots.TouchLastPlayed(active.SlotId);
            if (data.world == null)
            {
                Debug.LogWarning("AutoSaveOnDayChange: Saved without world state (missing IWorldSaveService).", this);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"AutoSaveOnDayChange: Failed to save '{active.FilePath}': {ex.Message}", this);
        }
    }

    
}
