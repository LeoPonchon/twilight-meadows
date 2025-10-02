using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère la barre d'outils (hotbar) du joueur et la sélection des slots
/// </summary>
public class HotbarController : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private GameObject selectorPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private int currentSlot = 0;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private InventorySlotManager slotManager;

    private RectTransform[] slots;
    private GameObject selectorInstance;

    private void Start()
    {
        InitializeSlots();
        InitializeSelector();

        if (slotManager != null)
        {
            // Créer uniquement les slots de la hotbar, pas l'inventaire complet
            if (!slotManager.AreSlotsCreated())
            {
                slotManager.CreateHotbarSlots();
                Debug.Log("Slots de hotbar créés par HotbarController");
            }
            else
            {
                Debug.Log("Les slots sont déjà créés, pas besoin de les recréer");
            }
        }
        else if (inventoryUI != null)
        {
            // Fallback sur InventoryUI si slotManager n'est pas directement assigné
            Debug.Log("Création des slots d'inventaire via InventoryUI");
            inventoryUI.CreateInventorySlots();
        }
        else
        {
            Debug.LogError("Ni slotManager ni inventoryUI n'est assigné dans HotbarController!", this);
        }
    }

    private void InitializeSlots()
    {
        if (slotsParent == null)
        {
            Debug.LogError("slotsParent is not assigned in HotbarController!", this);
            return;
        }

        // Si aucun enfant n'existe encore, on attend que les slots soient créés
        if (slotsParent.childCount == 0)
        {
            Debug.LogWarning("Aucun slot trouvé dans le GridLayout. Les slots seront initialisés plus tard.");
            // On va tester à nouveau après 0.2 secondes
            Invoke("InitializeSlots", 0.2f);
            return;
        }

        slots = new RectTransform[slotsParent.childCount];
        for (int i = 0; i < slotsParent.childCount; i++)
        {
            slots[i] = slotsParent.GetChild(i).GetComponent<RectTransform>();
        }
    }

    private void InitializeSelector()
    {
        if (selectorPrefab == null)
        {
            Debug.LogError("selectorPrefab is not assigned in HotbarController!", this);
            return;
        }

        if (slotsParent == null) return;

        selectorInstance = Instantiate(selectorPrefab, slotsParent.parent);
        selectorInstance.GetComponent<Image>().raycastTarget = false;
        UpdateSelectorPosition();
    }

    private void Update()
    {
        if (slots == null || slots.Length == 0 || selectorInstance == null)
            return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0f)
        {
            if (scrollInput < 0)
            {
                currentSlot = (currentSlot + 1) % slots.Length;
            }
            else if (scrollInput > 0)
            {
                currentSlot = (currentSlot - 1 + slots.Length) % slots.Length;
            }
            UpdateSelectorPosition();
        }
    }

    private void UpdateSelectorPosition()
    {
        if (selectorInstance == null || slots == null || currentSlot >= slots.Length)
            return;

        selectorInstance.transform.position = slots[currentSlot].position;
        Debug.Log($"Slot actif : {currentSlot}");
    }

    public int GetCurrentSlot()
    {
        return currentSlot;
    }
}