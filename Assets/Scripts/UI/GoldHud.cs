using TMPro;
using UnityEngine;

public sealed class GoldHud : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private SceneContext sceneContext;
    [SerializeField] private TextMeshProUGUI goldText;

    private void Awake()
    {
        if (sceneContext == null)
        {
            sceneContext = FindObjectOfType<SceneContext>();
        }
        if (sceneContext == null)
        {
            Debug.LogError("GoldHud: Missing SceneContext in scene.", this);
            enabled = false;
            return;
        }

        if (economyManager == null)
        {
            economyManager = sceneContext.GetRequired<EconomyManager>(this, nameof(economyManager));
        }
    }

    private void OnEnable()
    {
        if (economyManager == null) return;
        economyManager.OnGoldChanged += OnGoldChanged;
        economyManager.NotifyCurrentGold();
    }

    private void OnDisable()
    {
        if (economyManager == null) return;
        economyManager.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged(int gold)
    {
        if (goldText == null) return;
        goldText.text = $"{gold}g";
    }
}

