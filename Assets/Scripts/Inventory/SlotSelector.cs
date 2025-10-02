using UnityEngine;

/// <summary>
/// Gère le sélecteur visuel surbrillant les slots de l'inventaire
/// </summary>
public class SlotSelector : MonoBehaviour
{
    [SerializeField] private GameObject selectorPrefab;

    private GameObject selectorInstance;

    private void Awake()
    {
        if (selectorPrefab == null)
        {
            Debug.LogError("selectorPrefab is not assigned in SlotSelector!", this);
            return;
        }

        // Créer le sélecteur comme enfant direct du canvas ou d'une couche UI dédiée
        // plutôt que comme enfant des slots eux-mêmes
        selectorInstance = Instantiate(selectorPrefab, transform);

        // Configurer le sélecteur pour qu'il n'affecte pas le layout
        RectTransform rectTransform = selectorInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // S'assurer qu'il ne prend pas de place dans le layout
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        HideSelector();
    }

    public void ShowSelector(GameObject slot)
    {
        if (selectorInstance != null && slot != null)
        {
            selectorInstance.SetActive(true);

            // Utiliser la position du slot comme référence
            // sans modifier sa hiérarchie
            selectorInstance.transform.position = slot.transform.position;

            // S'assurer que le sélecteur est affiché par-dessus les slots
            selectorInstance.transform.SetAsLastSibling();
        }
    }

    public void HideSelector()
    {
        if (selectorInstance != null)
            selectorInstance.SetActive(false);
    }
}