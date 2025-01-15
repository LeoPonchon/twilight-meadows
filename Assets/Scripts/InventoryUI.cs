using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory playerInventory;
    public GameObject itemSlotPrefab;
    public GameObject selectorPrefab;
    public Transform inventoryPanel;
    public Transform hotbarPanel;
    public GameObject tooltip;
    public TextMeshProUGUI tooltipText;

    private ItemData floatingItemData;
    private Dictionary<int, GameObject> slots = new Dictionary<int, GameObject>();
    private GameObject selectorInstance;
    private int? hoveredSlotID = null;
    private int? selectedSlotID = null;
    private GameObject floatingItem;

    [SerializeField]
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInventory.OnInventoryChanged += UpdateInventoryUI;
        UpdateInventoryUI();

        selectorInstance = Instantiate(selectorPrefab, inventoryPanel.parent);
        selectorInstance.SetActive(false);
    }

    private void OnEnable() => playerInput.SwitchCurrentActionMap("UI");
    private void OnDisable() {
        playerInput.SwitchCurrentActionMap("Game");
        HideTooltip();
        HideSelector();
    }

    private void Update()
    {
        if (playerInput.actions["OpenInventory"].triggered)
        {
            HandleFloatingItem();
            HideTooltip();
        }
        else if (floatingItem != null)
        {
            UpdateFloatingItemPosition();
        }
    }

    private void HandleFloatingItem()
    {
        if (floatingItem != null && selectedSlotID.HasValue)
        {
            SetSlotImage(selectedSlotID.Value, floatingItem.GetComponent<Image>().sprite);
            Destroy(floatingItem);
            floatingItem = null;
        }

        selectedSlotID = null;
    }

    private void UpdateFloatingItemPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        floatingItem.GetComponent<RectTransform>().position = mousePosition + new Vector3(-50f, -50f, 0);
    }

    private void ShowTooltip(string itemName, string itemDescription, Vector3 position)
    {
        tooltip.SetActive(true);
        tooltipText.text = $"<b>{itemName}</b>\n{itemDescription}";
        tooltip.transform.position = position + new Vector3(0, -150f, 0f);
    }

    private void HideTooltip() => tooltip.SetActive(false);

    public void CreateInventorySlots()
    {
        CreateSlots(hotbarPanel, playerInventory.maxHotbarSlots, true);
        CreateSlots(inventoryPanel, playerInventory.maxSlots, false);
    }

    private void CreateSlots(Transform panel, int count, bool isHotbar)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, panel);
            int slotID = slots.Count;
            slots.Add(slotID, slot);

            slot.name = isHotbar ? $"Hotbar Slot {slotID}" : $"Inventory Slot {slotID}";
            AddSlotEvents(slot, slotID, isHotbar);
            ResetSlotUI(slot);
        }
    }

    private void AddSlotEvents(GameObject slot, int slotID, bool isHotbar)
    {
        EventTrigger trigger = slot.AddComponent<EventTrigger>();

        AddEventTrigger(trigger, EventTriggerType.PointerEnter, _ => OnSlotHover(slotID, slot, isHotbar));
        AddEventTrigger(trigger, EventTriggerType.PointerExit, _ => OnSlotExit(slotID));

        slot.GetComponent<UIClickHandler>().onLeftClick.AddListener(() => HandleLeftClick(slotID));
        slot.GetComponent<UIClickHandler>().onRightClick.AddListener(() => HandleRightClick(slotID));
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    private void OnSlotHover(int slotID, GameObject slot, bool isHotbar)
    {
        if (!isHotbar)
        {
            var item = playerInventory.GetItemInSlot(slotID);
            if (item != null) ShowTooltip(item.itemData.itemName, item.itemData.description, slot.transform.position);
        }

        hoveredSlotID = slotID;
        ShowSelector(slot);
    }

    private void OnSlotExit(int slotID)
    {
        HideTooltip();
        HideSelector();
        hoveredSlotID = null;
    }

    private void ShowSelector(GameObject slot)
    {
        selectorInstance.SetActive(true);
        selectorInstance.transform.position = slot.transform.position;
    }

    private void HideSelector() => selectorInstance.SetActive(false);

    private void ResetSlotUI(GameObject slot)
    {
        SetSlotImage(slot, null);
        slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
    }

    private void SetSlotImage(GameObject slot, Sprite sprite)
    {
        var image = slot.transform.GetChild(0).GetComponent<Image>();
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private void SetSlotImage(int slotID, Sprite sprite)
    {
        if (slots.TryGetValue(slotID, out GameObject slot)) SetSlotImage(slot, sprite);
    }

    public void UpdateInventoryUI()
    {
        foreach (var slot in slots.Values) ResetSlotUI(slot);

        foreach (var (slotID, stack) in playerInventory.GetAllItemsWithIDs())
        {
            if (slots.TryGetValue(slotID, out GameObject slot))
            {
                SetSlotImage(slot, stack.itemData.icon);
                slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = stack.quantity > 1 ? stack.quantity.ToString() : "";
            }
        }
    }
        
    private int floatingItemQuantity = 0;

    private void HandleLeftClick(int slotID)
    {
        if (floatingItem == null)
        {
            StartFloatingItem(slotID);
        }
        else if (floatingItem != null)
        {
            CompleteFloatingItem(slotID);
        }
    }

    private void HandleRightClick(int slotID)
    {
        if (floatingItem == null)
        {
            StartFloatingItem(slotID, true);
        }
        else if (floatingItem != null)
        {
            CompleteFloatingItem(slotID, true);
        }
    }

    private void StartFloatingItem(int slotID, bool isRightClick = false)
    {
        var item = playerInventory.GetItemInSlot(slotID);
        if (item == null) return;

        // Si on est en mode clic droit, diviser le stack en deux
        floatingItemQuantity = isRightClick ? Mathf.CeilToInt(item.quantity / 2f) : item.quantity;
        selectedSlotID = slotID;
        floatingItemData = item.itemData;

        floatingItem = CreateFloatingItem(slots[slotID].transform.GetChild(0).GetComponent<Image>().sprite, slots[slotID].transform.GetChild(0).GetComponent<RectTransform>().rect.size);
        SetSlotImage(slotID, null);

        playerInventory.RemoveItemFromSlot(slotID, floatingItemQuantity);
        UpdateInventoryUI();
    }

    private void CompleteFloatingItem(int slotID, bool isRightClick = false)
    {
        if (floatingItem == null || floatingItemData == null) return;

        var slotItem = playerInventory.GetItemInSlot(slotID);

        // Si le slot cible est vide ou contient un item similaire
        if (slotItem == null || (slotItem.itemData == floatingItemData && floatingItemData.isStackable))
        {
            int quantityToMove = isRightClick ? 1: floatingItemQuantity;
            quantityToMove = Mathf.Min(quantityToMove, floatingItemQuantity); // Ne pas dépasser la quantité restante

            playerInventory.AddItemToSlot(slotID, floatingItemData, quantityToMove);
            floatingItemQuantity -= quantityToMove;

            if (floatingItemQuantity <= 0)
            {
                Destroy(floatingItem);
                floatingItem = null;
                floatingItemData = null;
                selectedSlotID = null;
            }

            UpdateInventoryUI();
        }
        else
        {
            Debug.LogWarning("Cannot place item in this slot.");
        }
    }



    private GameObject CreateFloatingItem(Sprite sprite, Vector2 size)
    {
        GameObject floating = new GameObject("Floating Item", typeof(CanvasRenderer), typeof(RectTransform), typeof(Image));
        floating.transform.SetParent(transform.parent);

        var rectTransform = floating.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        var image = floating.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;

        return floating;
    }
}
