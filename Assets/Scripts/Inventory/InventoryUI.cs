using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface utilisateur pour l'inventaire - gère uniquement l'affichage UI
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform inventoryPanel;
    
    private InventoryDragAndDrop dragAndDrop;
    private InventoryTooltip tooltip;
    private GameObject slotSelectorInstance;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        SetupEventListeners();
        InitializeSlotSelector();
    }

    private void OnDisable()
    {
        HideTooltipAndSelector();
    }

    private void InitializeComponents()
    {
        dragAndDrop = GetComponent<InventoryDragAndDrop>();
        tooltip = GetComponent<InventoryTooltip>();
    }

    private void SetupEventListeners()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged += UpdateInventory;

        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryStateChanged += OnInventoryStateChanged;
        }
    }

    public ItemStack GetCurrentHotbarItem()
    {
        return inventoryManager != null ? inventoryManager.GetCurrentHotbarItem() : null;
    }
    
    public bool UseCurrentHotbarItem()
    {
        return inventoryManager != null ? inventoryManager.UseCurrentHotbarItem() : false;
    }
    
    public int GetCurrentHotbarSlot()
    {
        return inventoryManager != null ? inventoryManager.GetCurrentHotbarSlot() : 0;
    }

    private void UpdateInventory()
    {
        UpdateInventoryUI();
    }

    public void HideTooltipAndSelector()
    {
        if (tooltip != null)
            tooltip.HideTooltip();
        HideSlotSelector();
    }
    
    private void OnInventoryStateChanged(bool isOpen)
    {
        if (inventoryPanel != null)
            inventoryPanel.gameObject.SetActive(isOpen);
            
        if (!isOpen)
            HideTooltipAndSelector();
    }

    public void HandleLeftClick(int slotID)
    {
        if (dragAndDrop != null)
            dragAndDrop.HandleLeftClick(slotID);
    }

    public void HandleRightClick(int slotID)
    {
        if (dragAndDrop != null)
            dragAndDrop.HandleRightClick(slotID);
    }
    
    public void ShowTooltip(string itemName, string description, Vector3 position)
    {
        if (tooltip != null)
            tooltip.ShowTooltip(itemName, description, position);
    }


    public void UpdateView()
    {
        UpdateInventoryUI();
    }
    
    /// <summary>
    /// Obtient un slot depuis le manager
    /// </summary>
    public GameObject GetSlotFromManager(int slotID)
    {
        return inventoryManager != null ? inventoryManager.GetSlot(slotID) : null;
    }

    /// Réinitialise l'UI d'un slot
    public void ResetSlotUI(GameObject slot)
    {
        SetSlotImage(slot, null);
        SetSlotQuantity(slot, "");
    }

    /// Définit l'image d'un slot
    public void SetSlotImage(GameObject slot, Sprite sprite)
    {
        var image = slot.transform.GetChild(0).GetComponent<Image>();
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    /// Définit le texte de quantité d'un slot
    private void SetSlotQuantity(GameObject slot, string text)
    {
        slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = text;
    }

    /// Met à jour l'UI de l'inventaire
    public void UpdateInventoryUI()
    {
        if (playerInventory == null || inventoryManager == null) return;

        // Récupérer tous les slots depuis le manager
        var slots = inventoryManager.GetAllSlots();
        
        // Réinitialiser tous les slots
        foreach (var slot in slots.Values) 
        {
            ResetSlotUI(slot);
        }

        // Récupérer les objets de l'inventaire
        var items = playerInventory.GetAllItemsWithIDs();

        foreach (var (slotID, stack) in items)
        {
            if (slots.TryGetValue(slotID, out GameObject slot))
            {
                SetSlotImage(slot, stack.itemData.icon);
                SetSlotQuantity(slot, stack.quantity > 1 ? stack.quantity.ToString() : "");
            }
        }
    }

    /// Initialise le sélecteur de slot
    private void InitializeSlotSelector()
    {
        if (inventoryManager == null) return;
        
        // Le sélecteur est maintenant géré par le manager
        slotSelectorInstance = inventoryManager.GetSlotSelectorInstance();
        
        if (slotSelectorInstance != null)
        {
            HideSlotSelector();
        }
    }
    
    /// Affiche le sélecteur sur un slot
    public void ShowSlotSelector(GameObject slot)
    {
        if (inventoryManager != null)
        {
            inventoryManager.ShowSlotSelector(slot);
        }
    }
    
    /// Cache le sélecteur de slot
    public void HideSlotSelector()
    {
        if (inventoryManager != null)
        {
            inventoryManager.HideSlotSelector();
        }
    }

}
