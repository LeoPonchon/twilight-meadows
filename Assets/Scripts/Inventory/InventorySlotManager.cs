using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Gère la création et la mise à jour des slots d'inventaire visuels
/// </summary>
public class InventorySlotManager : MonoBehaviour, IInventoryView
{
    #region Variables publiques

    [Header("Références")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private Transform hotbarPanel;

    #endregion

    #region Variables privées

    // Dictionnaire des slots : clé = ID de slot, valeur = GameObject du slot
    private Dictionary<int, GameObject> slots = new Dictionary<int, GameObject>();

    // Suivi des slots créés
    private bool hotbarSlotsCreated = false;
    private bool inventorySlotsCreated = false;

    #endregion

    #region Événements

    // Événements pour les interactions avec les slots
    public delegate void SlotEventHandler(int slotID, GameObject slot, bool isHotbar);
    public event SlotEventHandler OnSlotHover;
    public event SlotEventHandler OnSlotExit;
    public event System.Action<int> OnSlotLeftClick;
    public event System.Action<int> OnSlotRightClick;

    #endregion

    #region Méthodes Unity

    private void Awake()
    {
        ValidateReferences();
    }

    #endregion

    #region Validation des références

    /// <summary>
    /// Valide que toutes les références nécessaires sont assignées
    /// </summary>
    private void ValidateReferences()
    {
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory is not assigned in InventorySlotManager!", this);
        }

        if (itemSlotPrefab == null)
        {
            Debug.LogError("itemSlotPrefab is not assigned in InventorySlotManager!", this);
        }

        if (inventoryPanel == null)
        {
            Debug.LogError("inventoryPanel is not assigned in InventorySlotManager!", this);
        }

        if (hotbarPanel == null)
        {
            Debug.LogError("hotbarPanel is not assigned in InventorySlotManager!", this);
        }
    }

    #endregion

    #region Méthodes publiques - Interface IInventoryView

    /// <summary>
    /// Met à jour la vue de l'inventaire
    /// </summary>
    public void UpdateView()
    {
        UpdateInventoryUI();
    }

    /// <summary>
    /// Crée les slots visuels
    /// </summary>
    public void CreateSlots()
    {
        CreateInventorySlots();
    }

    /// <summary>
    /// Nettoie les slots visuels
    /// </summary>
    public void ClearSlots()
    {
        foreach (var slot in slots.Values)
        {
            if (slot != null)
            {
                Destroy(slot);
            }
        }
        slots.Clear();
        hotbarSlotsCreated = false;
        inventorySlotsCreated = false;
    }

    /// <summary>
    /// Change l'image d'un slot spécifique
    /// </summary>
    public void SetSlotImage(int slotID, Sprite sprite)
    {
        if (slots.TryGetValue(slotID, out GameObject slot))
        {
            SetSlotImage(slot, sprite);
        }
    }

    /// <summary>
    /// Obtient l'objet GameObject d'un slot spécifique
    /// </summary>
    public GameObject GetSlot(int slotID)
    {
        if (slots.TryGetValue(slotID, out GameObject slot))
            return slot;
        return null;
    }

    #endregion

    #region Création des slots

    /// <summary>
    /// Crée tous les slots (hotbar et inventaire)
    /// </summary>
    public void CreateInventorySlots()
    {
        // Appeler les deux méthodes pour créer les deux types de slots
        CreateHotbarSlots();
        CreateMainInventorySlots();

        Debug.Log($"Nombre total de slots créés: {slots.Count}");
    }

    /// <summary>
    /// Crée uniquement les slots de la hotbar
    /// </summary>
    public void CreateHotbarSlots()
    {
        if (playerInventory == null)
        {
            Debug.LogError("Cannot create hotbar slots: playerInventory is null", this);
            return;
        }

        if (hotbarSlotsCreated)
        {
            Debug.Log("Les slots de hotbar sont déjà créés");
            return;
        }

        // Nettoyer les slots hotbar existants
        CleanSlotsInRange(0, playerInventory.maxHotbarSlots);

        if (hotbarPanel != null)
        {
            Debug.Log($"Création de {playerInventory.maxHotbarSlots} slots pour la hotbar");
            CreateSlots(hotbarPanel, playerInventory.maxHotbarSlots, true);
            hotbarSlotsCreated = true;
        }
        else
        {
            Debug.LogError("hotbarPanel is not assigned in InventorySlotManager!", this);
        }
    }

    /// <summary>
    /// Crée uniquement les slots de l'inventaire principal
    /// </summary>
    public void CreateMainInventorySlots()
    {
        if (playerInventory == null)
        {
            Debug.LogError("Cannot create inventory slots: playerInventory is null", this);
            return;
        }

        if (inventorySlotsCreated)
        {
            Debug.Log("Les slots d'inventaire sont déjà créés");
            return;
        }

        // Nettoyer les slots d'inventaire existants
        CleanSlotsInRange(playerInventory.maxHotbarSlots, playerInventory.maxHotbarSlots + playerInventory.maxSlots);

        if (inventoryPanel != null)
        {
            Debug.Log($"Création de {playerInventory.maxSlots} slots pour l'inventaire principal");
            CreateSlots(inventoryPanel, playerInventory.maxSlots, false);
            inventorySlotsCreated = true;
        }
        else
        {
            Debug.LogError("inventoryPanel is not assigned in InventorySlotManager!", this);
        }
    }

    /// <summary>
    /// Nettoie les slots dans une plage d'IDs
    /// </summary>
    private void CleanSlotsInRange(int startId, int endId)
    {
        List<int> keysToRemove = new List<int>();

        foreach (var kvp in slots)
        {
            if (kvp.Key >= startId && kvp.Key < endId)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            slots.Remove(key);
        }
    }

    /// <summary>
    /// Crée les slots pour un panel donné
    /// </summary>
    private void CreateSlots(Transform panel, int count, bool isHotbar)
    {
        if (itemSlotPrefab == null)
        {
            Debug.LogError("Cannot create slots: itemSlotPrefab is null", this);
            return;
        }

        // Effacer tous les enfants existants du panel
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, panel);

            // Calculer le bon ID de slot en fonction de si c'est un slot hotbar ou normal
            int slotID = isHotbar ? i : playerInventory.maxHotbarSlots + i;

            slots.Add(slotID, slot);

            slot.name = isHotbar ? $"Hotbar Slot {i}" : $"Inventory Slot {i}";

            ConfigureSlotLayout(slot);
            AddSlotEvents(slot, slotID, isHotbar);
            ResetSlotUI(slot);

            // Style visuel différent pour les slots de hotbar
            if (isHotbar)
            {
                StyleHotbarSlot(slot);
            }
        }

        // Force la mise à jour du layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Configure le layout d'un slot
    /// </summary>
    private void ConfigureSlotLayout(GameObject slot)
    {
        RectTransform rectTransform = slot.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Pour un GridLayoutGroup, ces paramètres n'affecteront pas le positionnement
            // mais assureront que le slot ne perturbe pas le layout par des interactions
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    /// <summary>
    /// Applique un style visuel distinct pour les slots de hotbar
    /// </summary>
    private void StyleHotbarSlot(GameObject slot)
    {
        // Ajouter un indicateur visuel pour les slots de hotbar
        var image = slot.transform.GetChild(0).GetComponent<Image>();
        if (image != null)
        {
            // Ajouter une bordure ou un effet de surbrillance pour distinguer les slots de hotbar
            image.color = new Color(1f, 1f, 0.8f); // Teinte légèrement jaune
        }
    }

    #endregion

    #region Gestion des événements

    /// <summary>
    /// Ajoute les gestionnaires d'événements à un slot
    /// </summary>
    private void AddSlotEvents(GameObject slot, int slotID, bool isHotbar)
    {
        // Utiliser l'EventTrigger existant ou en ajouter un nouveau
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.AddComponent<EventTrigger>();
        }
        else
        {
            // Nettoyer les événements existants pour éviter les doublons
            trigger.triggers.Clear();
        }

        AddEventTrigger(trigger, EventTriggerType.PointerEnter, _ => HandleSlotHover(slotID, slot, isHotbar));
        AddEventTrigger(trigger, EventTriggerType.PointerExit, _ => HandleSlotExit(slotID));

        // Utiliser le UIClickHandler existant ou en ajouter un nouveau
        UIClickHandler clickHandler = slot.GetComponent<UIClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = slot.AddComponent<UIClickHandler>();
        }
        else
        {
            // Nettoyer les événements existants
            clickHandler.onLeftClick.RemoveAllListeners();
            clickHandler.onRightClick.RemoveAllListeners();
        }

        clickHandler.onLeftClick.AddListener(() => OnSlotLeftClick?.Invoke(slotID));
        clickHandler.onRightClick.AddListener(() => OnSlotRightClick?.Invoke(slotID));
    }

    /// <summary>
    /// Ajoute un gestionnaire d'événement à un trigger
    /// </summary>
    private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// Gère l'événement de survol d'un slot
    /// </summary>
    private void HandleSlotHover(int slotID, GameObject slot, bool isHotbar)
    {
        OnSlotHover?.Invoke(slotID, slot, isHotbar);
    }

    /// <summary>
    /// Gère l'événement de sortie d'un slot
    /// </summary>
    private void HandleSlotExit(int slotID)
    {
        OnSlotExit?.Invoke(slotID, null, false);
    }

    #endregion

    #region Mise à jour de l'interface

    /// <summary>
    /// Réinitialise l'UI d'un slot
    /// </summary>
    public void ResetSlotUI(GameObject slot)
    {
        SetSlotImage(slot, null);
        SetSlotQuantity(slot, "");
    }

    /// <summary>
    /// Définit l'image d'un slot
    /// </summary>
    public void SetSlotImage(GameObject slot, Sprite sprite)
    {
        var image = slot.transform.GetChild(0).GetComponent<Image>();
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    /// <summary>
    /// Définit le texte de quantité d'un slot
    /// </summary>
    private void SetSlotQuantity(GameObject slot, string text)
    {
        slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = text;
    }

    /// <summary>
    /// Met à jour l'UI de l'inventaire
    /// </summary>
    public void UpdateInventoryUI()
    {
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory est null dans UpdateInventoryUI", this);
            return;
        }

        // S'assurer que les slots sont créés avant de mettre à jour l'UI
        if (!hotbarSlotsCreated || !inventorySlotsCreated)
        {
            Debug.Log("Création des slots avant la mise à jour de l'UI");
            CreateInventorySlots();
        }

        // Réinitialiser tous les slots
        foreach (var slot in slots.Values) ResetSlotUI(slot);

        // Récupérer les objets de l'inventaire
        var items = playerInventory.GetAllItemsWithIDs();
        Debug.Log($"Mise à jour de l'UI avec {items.Count} items dans l'inventaire");

        // Mettre à jour l'UI pour chaque objet
        foreach (var (slotID, stack) in items)
        {
            Debug.Log($"Item dans le slot {slotID}: {stack.itemData.itemName} x{stack.quantity}");

            if (slots.TryGetValue(slotID, out GameObject slot))
            {
                SetSlotImage(slot, stack.itemData.icon);
                SetSlotQuantity(slot, stack.quantity > 1 ? stack.quantity.ToString() : "");
            }
            else
            {
                Debug.LogWarning($"Slot {slotID} non trouvé dans le dictionnaire de slots", this);
            }
        }
    }

    #endregion

    #region Méthodes utilitaires

    /// <summary>
    /// Renvoie tous les slots
    /// </summary>
    public Dictionary<int, GameObject> GetAllSlots()
    {
        return slots;
    }

    /// <summary>
    /// Vérifie si les slots sont déjà créés
    /// </summary>
    public bool AreSlotsCreated()
    {
        return hotbarSlotsCreated && inventorySlotsCreated;
    }

    /// <summary>
    /// Réinitialise les flags de création pour forcer une recréation des slots
    /// </summary>
    public void ResetCreationFlags()
    {
        hotbarSlotsCreated = false;
        inventorySlotsCreated = false;
    }

    #endregion
}