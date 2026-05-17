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

    [Header("Systems (typed)")]
    [SerializeField] private MonoBehaviour economyManager;
    [SerializeField] private MonoBehaviour timeManager;
    [SerializeField] private MonoBehaviour weatherManager;
    [SerializeField] private MonoBehaviour saveGameService;
    [SerializeField] private MonoBehaviour worldSaveService;

    public PlayerInput PlayerInput => playerInput;
    public Canvas UiCanvas => uiCanvas;

    private void Awake()
    {
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
        if (economyManager == null) Debug.LogError("SceneContext: EconomyManager is not assigned.", this);
        if (timeManager == null) Debug.LogError("SceneContext: TimeManager is not assigned.", this);
        if (weatherManager == null) Debug.LogError("SceneContext: WeatherManager is not assigned.", this);
        if (saveGameService == null) Debug.LogWarning("SceneContext: SaveGameService is not assigned.", this);
        if (worldSaveService == null) Debug.LogWarning("SceneContext: WorldSaveService is not assigned.", this);
    }

    public bool TryGet<T>(out T value) where T : class
    {
        value = Get<T>();
        return value != null;
    }

    public T Get<T>() where T : class
    {
        if (TryResolveFromObject(player, out T a)) return a;
        if (TryResolveFromObject(playerInventory, out T b)) return b;
        if (TryResolveFromObject(inventoryManager, out T c)) return c;
        if (TryResolveFromObject(economyManager, out T d)) return d;
        if (TryResolveFromObject(timeManager, out T e)) return e;
        if (TryResolveFromObject(weatherManager, out T f)) return f;
        if (TryResolveFromObject(saveGameService, out T g)) return g;
        if (TryResolveFromObject(worldSaveService, out T h)) return h;
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

    private static bool TryResolveFromObject<T>(UnityEngine.Object obj, out T value) where T : class
    {
        if (obj == null)
        {
            value = null;
            return false;
        }

        if (obj is T direct)
        {
            value = direct;
            return true;
        }

        if (obj is GameObject go)
        {
            value = go.GetComponent(typeof(T)) as T;
            return value != null;
        }

        if (obj is Component c)
        {
            value = c.GetComponent(typeof(T)) as T;
            return value != null;
        }

        value = null;
        return false;
    }
}
