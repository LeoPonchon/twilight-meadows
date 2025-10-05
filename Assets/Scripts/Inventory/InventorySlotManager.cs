using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Gère la création et la gestion des slots d'inventaire
/// </summary>
public class InventorySlotManager : MonoBehaviour
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject itemSlotPrefab;
    
    [Header("UI Panels")]
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private Transform hotbarPanel;
    
    // Variables pour la gestion des slots
    private Dictionary<int, GameObject> hotbarSlots = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> allSlots = new Dictionary<int, GameObject>();
    
    // Variables pour gérer l'état de création
    private bool hotbarSlotsCreated = false;
    private bool inventorySlotsCreated = false;
    
    // Références
    private Inventory playerInventory;
    private InventoryUI inventoryUI;
    
    public Dictionary<int, GameObject> AllSlots => allSlots;
    public Dictionary<int, GameObject> HotbarSlots => hotbarSlots;
    public bool HotbarSlotsCreated => hotbarSlotsCreated;
    public bool InventorySlotsCreated => inventorySlotsCreated;
    
    public void Initialize(Inventory inventory, InventoryUI ui)
    {
        playerInventory = inventory;
        inventoryUI = ui;
    }
    
    public void CreateInventorySlots()
    {
        CreateHotbarSlots();
        CreateMainInventorySlots();
    }
    
    public void CreateHotbarSlots()
    {
        if (playerInventory == null || hotbarSlotsCreated) return;
        
        CleanSlotsInRange(0, playerInventory.maxHotbarSlots);
        
        if (hotbarPanel != null)
        {
            CreateSlots(hotbarPanel, playerInventory.maxHotbarSlots, true);
            hotbarSlotsCreated = true;
        }
    }
    
    public void CreateMainInventorySlots()
    {
        if (playerInventory == null || inventorySlotsCreated) return;
        
        CleanSlotsInRange(playerInventory.maxHotbarSlots, playerInventory.maxHotbarSlots + playerInventory.maxSlots);
        
        if (inventoryPanel != null)
        {
            CreateSlots(inventoryPanel, playerInventory.maxSlots, false);
            inventorySlotsCreated = true;
        }
    }
    
    private void CleanSlotsInRange(int startId, int endId)
    {
        List<int> keysToRemove = new List<int>();
        
        foreach (var kvp in allSlots)
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
            allSlots.Remove(key);
            hotbarSlots.Remove(key);
        }
    }
    
    private void CreateSlots(Transform panel, int count, bool isHotbar)
    {
        if (itemSlotPrefab == null) return;
        
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
            
            allSlots.Add(slotID, slot);
            
            slot.name = isHotbar ? $"Hotbar Slot {i}" : $"Inventory Slot {i}";
            
            ConfigureSlotLayout(slot);
            AddSlotEvents(slot, slotID, isHotbar);
            
            // Style visuel différent pour les slots de hotbar
            if (isHotbar)
            {
                StyleHotbarSlot(slot);
                hotbarSlots[i] = slot;
            }
        }
        
        // Force la mise à jour du layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
    }
    
    private void ConfigureSlotLayout(GameObject slot)
    {
        RectTransform rectTransform = slot.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }
    
    private void StyleHotbarSlot(GameObject slot)
    {
        var image = slot.transform.GetChild(0).GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(1f, 1f, 0.8f); // Teinte légèrement jaune
        }
    }
    
    private void AddSlotEvents(GameObject slot, int slotID, bool isHotbar)
    {
        // Ajouter le composant Button si il n'existe pas
        Button button = slot.GetComponent<Button>();
        if (button == null)
        {
            button = slot.AddComponent<Button>();
        }
        
        // Configurer le bouton pour être transparent
        button.transition = Selectable.Transition.None;
        
        // Nettoyer les anciens listeners
        button.onClick.RemoveAllListeners();
        
        // Ajouter le listener pour le clic gauche
        button.onClick.AddListener(() => HandleSlotLeftClick(slotID));
        
        // Ajouter les EventTriggers pour le hover (Button ne gère que les clics)
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.AddComponent<EventTrigger>();
        }
        else
        {
            trigger.triggers.Clear();
        }
        
        AddEventTrigger(trigger, EventTriggerType.PointerEnter, _ => HandleSlotHover(slotID, slot, isHotbar));
        AddEventTrigger(trigger, EventTriggerType.PointerExit, _ => HandleSlotExit(slotID, slot, isHotbar));
        
        // Garder UIClickHandler pour les clics droits
        UIClickHandler clickHandler = slot.GetComponent<UIClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = slot.AddComponent<UIClickHandler>();
        }
        else
        {
            clickHandler.onLeftClick.RemoveAllListeners();
            clickHandler.onRightClick.RemoveAllListeners();
        }
        
        // Seulement le clic droit via UIClickHandler
        clickHandler.onRightClick.AddListener(() => HandleSlotRightClick(slotID));
    }
    
    private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }
    
    private void HandleSlotHover(int slotID, GameObject slot, bool isHotbar)
    {
        if (!isHotbar && playerInventory != null)
        {
            var item = playerInventory.GetItemInSlot(slotID);
            if (item != null && inventoryUI != null)
            {
                inventoryUI.ShowTooltip(item.itemData.itemName, item.itemData.description, slot.transform.position);
            }
        }
        if (inventoryUI != null)
        {
            inventoryUI.ShowSlotSelector(slot);
        }
    }
    
    private void HandleSlotExit(int slotID, GameObject slot, bool isHotbar)
    {
        if (inventoryUI != null)
        {
            inventoryUI.HideTooltipAndSelector();
        }
    }
    
    private void HandleSlotLeftClick(int slotID)
    {
        if (inventoryUI != null)
        {
            inventoryUI.HandleLeftClick(slotID);
        }
    }
    
    private void HandleSlotRightClick(int slotID)
    {
        if (inventoryUI != null)
        {
            inventoryUI.HandleRightClick(slotID);
        }
    }
    
    public GameObject GetSlot(int slotID)
    {
        return allSlots.TryGetValue(slotID, out GameObject slot) ? slot : null;
    }
    
    public void RegisterHotbarSlot(int slotIndex, GameObject slotObject)
    {
        if (slotIndex >= 0 && slotIndex < playerInventory.maxHotbarSlots)
        {
            hotbarSlots[slotIndex] = slotObject;
            allSlots[slotIndex] = slotObject;
        }
    }
    
    public void RegisterInventorySlot(int slotIndex, GameObject slotObject)
    {
        allSlots[slotIndex] = slotObject;
    }
}
