using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Point de câblage de scène pour réduire les FindObjectOfType dans le code.
/// Assigne ces références dans la scène (recommandé), les scripts peuvent s'y brancher.
/// </summary>
public class SceneContext : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Canvas uiCanvas;

    [Header("Gameplay")]
    [SerializeField] private UnityEngine.Object player;
    [SerializeField] private UnityEngine.Object playerInventory;
    [SerializeField] private UnityEngine.Object inventoryManager;
    [SerializeField] private UnityEngine.Object inventoryUI;

    [Header("Systems")]
    [SerializeField] private UnityEngine.Object economyManager;
    [SerializeField] private UnityEngine.Object timeManager;
    [SerializeField] private UnityEngine.Object weatherManager;

    public PlayerInput PlayerInput => playerInput;
    public Canvas UiCanvas => uiCanvas;

    public T Get<T>() where T : class
    {
        if (TryResolve(player, out T a)) return a;
        if (TryResolve(playerInventory, out T b)) return b;
        if (TryResolve(inventoryManager, out T c)) return c;
        if (TryResolve(inventoryUI, out T d)) return d;
        if (TryResolve(economyManager, out T e)) return e;
        if (TryResolve(timeManager, out T f)) return f;
        if (TryResolve(weatherManager, out T g)) return g;
        return null;
    }

    private static bool TryResolve<T>(UnityEngine.Object obj, out T value) where T : class
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
