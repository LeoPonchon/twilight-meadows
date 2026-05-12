using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Point de câblage de scène pour réduire les FindObjectOfType dans le code.
/// Assigne ces références dans la scène (recommandé), les scripts peuvent s'y brancher.
/// </summary>
public class SceneContext : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private bool strictWiring = true;

    [Header("Core")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Canvas uiCanvas;

    [Header("Gameplay (typed)")]
    [SerializeField] private MonoBehaviour player;
    [SerializeField] private MonoBehaviour playerInventory;
    [SerializeField] private MonoBehaviour inventoryManager;
    [SerializeField] private MonoBehaviour inventoryUI;

    [Header("Systems (typed)")]
    [SerializeField] private MonoBehaviour economyManager;
    [SerializeField] private MonoBehaviour timeManager;
    [SerializeField] private MonoBehaviour weatherManager;

    [Header("Gameplay (legacy migration)")]
    [SerializeField] private UnityEngine.Object legacyPlayer;
    [SerializeField] private UnityEngine.Object legacyPlayerInventory;
    [SerializeField] private UnityEngine.Object legacyInventoryManager;
    [SerializeField] private UnityEngine.Object legacyInventoryUI;

    [Header("Systems (legacy migration)")]
    [SerializeField] private UnityEngine.Object legacyEconomyManager;
    [SerializeField] private UnityEngine.Object legacyTimeManager;
    [SerializeField] private UnityEngine.Object legacyWeatherManager;

    public PlayerInput PlayerInput => playerInput;
    public Canvas UiCanvas => uiCanvas;

    private void Awake()
    {
        MigrateLegacyReferences();

        if (!strictWiring) return;

        if (playerInput == null)
        {
            Debug.LogError("SceneContext: PlayerInput is not assigned.", this);
        }

        if (uiCanvas == null)
        {
            Debug.LogError("SceneContext: UiCanvas is not assigned.", this);
        }

        if (player == null) Debug.LogError("SceneContext: Player is not assigned.", this);
        if (playerInventory == null) Debug.LogError("SceneContext: PlayerInventory is not assigned.", this);
        if (inventoryManager == null) Debug.LogError("SceneContext: InventoryManager is not assigned.", this);
        if (inventoryUI == null) Debug.LogError("SceneContext: InventoryUI is not assigned.", this);
        if (economyManager == null) Debug.LogError("SceneContext: EconomyManager is not assigned.", this);
        if (timeManager == null) Debug.LogError("SceneContext: TimeManager is not assigned.", this);
        if (weatherManager == null) Debug.LogError("SceneContext: WeatherManager is not assigned.", this);
    }

    public bool TryGet<T>(out T value) where T : class
    {
        value = Get<T>();
        return value != null;
    }

    public T Get<T>() where T : class
    {
        if (TryResolveFromLegacy(player, out T a)) return a;
        if (TryResolveFromLegacy(playerInventory, out T b)) return b;
        if (TryResolveFromLegacy(inventoryManager, out T c)) return c;
        if (TryResolveFromLegacy(inventoryUI, out T d)) return d;
        if (TryResolveFromLegacy(economyManager, out T e)) return e;
        if (TryResolveFromLegacy(timeManager, out T f)) return f;
        if (TryResolveFromLegacy(weatherManager, out T g)) return g;
        return null;
    }

    public T GetRequired<T>(Component requester, string label = null) where T : class
    {
        var value = Get<T>();
        if (value != null) return value;

        string typeName = typeof(T).Name;
        string who = requester != null ? requester.GetType().Name : "UnknownRequester";
        string extra = string.IsNullOrEmpty(label) ? string.Empty : $" ({label})";
        Debug.LogError($"SceneContext: Missing required reference for {typeName}{extra} requested by {who}.", this);
        return null;
    }

    private void MigrateLegacyReferences()
    {
        if (player == null) TryResolveFromLegacy(legacyPlayer, out player);
        if (playerInventory == null) TryResolveFromLegacy(legacyPlayerInventory, out playerInventory);
        if (inventoryManager == null) TryResolveFromLegacy(legacyInventoryManager, out inventoryManager);
        if (inventoryUI == null) TryResolveFromLegacy(legacyInventoryUI, out inventoryUI);
        if (economyManager == null) TryResolveFromLegacy(legacyEconomyManager, out economyManager);
        if (timeManager == null) TryResolveFromLegacy(legacyTimeManager, out timeManager);
        if (weatherManager == null) TryResolveFromLegacy(legacyWeatherManager, out weatherManager);
    }

    private static void TryResolveFromLegacy<T>(UnityEngine.Object obj, out T value) where T : class
    {
        if (obj == null)
        {
            value = null;
            return;
        }

        if (obj is T direct)
        {
            value = direct;
            return;
        }

        if (obj is GameObject go)
        {
            value = go.GetComponent(typeof(T)) as T;
            return;
        }

        if (obj is Component c)
        {
            value = c.GetComponent(typeof(T)) as T;
            return;
        }

        value = null;
    }
}
