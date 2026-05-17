using UnityEngine;

[DefaultExecutionOrder(-450)]
public sealed class SleepController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private KeyCode debugSleepKey = KeyCode.O;

    [Header("Rules")]
    [SerializeField] private int faintHour = 2;
    [SerializeField] private int faintMinute = 0;
    [SerializeField] private int wakeFromSleepHour = 6;
    [SerializeField] private int wakeFromSleepMinute = 0;
    [SerializeField] private int wakeFromFaintHour = 10;
    [SerializeField] private int wakeFromFaintMinute = 0;

    [Header("Bed")]
    [SerializeField] private Transform bedSpawnPoint;

    [Header("References (optional)")]
    [SerializeField] private SceneContext sceneContext;

    private TimeManager timeManager;
    private ISaveGameService saveService;

    private bool sleptThisDay;
    private int lastFaintDay = -1;

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
            Debug.LogError("SleepController: Missing SceneContext in scene.", this);
            enabled = false;
            return;
        }

        timeManager = sceneContext.GetRequired<TimeManager>(this, nameof(timeManager));
        saveService = sceneContext.Get<ISaveGameService>();

        if (timeManager != null)
        {
            timeManager.OnDayChanged += OnDayChanged;
        }
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnDayChanged -= OnDayChanged;
        }
    }

    private void Update()
    {
        if (timeManager == null) return;

        if (Input.GetKeyDown(debugSleepKey))
        {
            GoToBed();
            return;
        }

        TryFaintIfNeeded();
    }

    private void OnDayChanged(int day)
    {
        sleptThisDay = false;
        lastFaintDay = -1;
    }

    private void TryFaintIfNeeded()
    {
        if (sleptThisDay) return;

        int day = timeManager.GetCurrentDay();
        if (lastFaintDay == day) return;

        int h = timeManager.GetCurrentHour();
        int m = timeManager.GetCurrentMins();

        if (h == faintHour && m == faintMinute)
        {
            lastFaintDay = day;
            Faint();
        }
    }

    private void GoToBed()
    {
        sleptThisDay = true;

        bool shouldAdvanceDay = ShouldAdvanceDayForWake(wakeFromSleepHour, wakeFromSleepMinute);
        WakeAt(wakeFromSleepHour, wakeFromSleepMinute, shouldAdvanceDay);
    }

    private void Faint()
    {
        sleptThisDay = true;
        WakeAt(wakeFromFaintHour, wakeFromFaintMinute, advanceDay: false);
    }

    private bool ShouldAdvanceDayForWake(int wakeHour, int wakeMinute)
    {
        int h = timeManager.GetCurrentHour();
        int m = timeManager.GetCurrentMins();

        if (h > wakeHour) return true;
        if (h < wakeHour) return false;
        return m > wakeMinute;
    }

    private void WakeAt(int hour, int minute, bool advanceDay)
    {
        if (advanceDay)
        {
            timeManager.AdvanceDays(1);
        }

        timeManager.SetTimeOfDay(hour, minute);

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && bedSpawnPoint != null)
        {
            player.transform.position = bedSpawnPoint.position;
        }

        saveService?.SaveNow();
    }
}
