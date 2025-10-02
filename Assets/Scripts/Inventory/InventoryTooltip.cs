using TMPro;
using UnityEngine;

/// <summary>
/// Gère l'affichage des tooltips pour les objets de l'inventaire
/// </summary>
public class InventoryTooltip : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private void Awake()
    {
        if (tooltipPanel == null || tooltipText == null)
        {
            Debug.LogError("Tooltip elements not assigned in InventoryTooltip!", this);
        }

        HideTooltip();
    }

    public void ShowTooltip(string itemName, string itemDescription, Vector3 position)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        tooltipPanel.SetActive(true);
        tooltipText.text = $"<b>{itemName}</b>\n{itemDescription}";
        tooltipPanel.transform.position = position + new Vector3(0, -150f, 0f);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}