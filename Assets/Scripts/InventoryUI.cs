using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory playerInventory;
    public GameObject itemSlotPrefab;
    public Transform inventoryPanel;
    public Transform hotbarPanel;
    public GameObject tooltip; // Référence à l'infobulle dans le Canvas
    public TextMeshProUGUI tooltipText; // Texte de l'infobulle


    private Dictionary<int, GameObject> slots = new Dictionary<int, GameObject>();

    private int? selectedSlotID = null;
    private GameObject floatingItem;

    public GameObject player;
    private PlayerInput playerInput;


    private void Awake()
    {
        playerInventory.OnInventoryChanged += UpdateInventoryUI;
        CreateInventorySlots();
        UpdateInventoryUI();
        playerInput = player.GetComponent<PlayerInput>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        tooltip.SetActive(false); // Désactiver l’infobulle au début
    }

    private void Update()
    {
        if (playerInput.actions["OpenInventory"].triggered) // Si on ferme l'inventaire
        {
            HideTooltip();
            // Gestion de la fermeture de l'inventaire
            if (floatingItem != null && selectedSlotID.HasValue)
            {
                // Remettre l'objet dans le slot sélectionné
                Debug.Log($"Remettre l'objet flottant dans le slot {selectedSlotID.Value}");

                Image slotImage = slots[selectedSlotID.Value].transform.GetChild(0).GetComponent<Image>();
                Image floatingImage = floatingItem.GetComponent<Image>();

                slotImage.sprite = floatingImage.sprite;
                slotImage.enabled = true;

                // Détruire l'objet flottant
                Destroy(floatingItem);
                floatingItem = null;
            }

            // Réinitialiser la sélection
            selectedSlotID = null;
        }
        else if (floatingItem != null)
        {
            // Mettre à jour la position de l'objet flottant avec la souris
            Vector3 mousePosition = Input.mousePosition;
            RectTransform floatingRect = floatingItem.GetComponent<RectTransform>();
            floatingRect.position = new Vector3(mousePosition.x - 50f, mousePosition.y - 50f, 0);
        }
    }

    private void ShowTooltip(string itemName, string itemDescription, Vector3 position)
    {
        if (gameObject.activeSelf)
        {
            tooltip.SetActive(true);
            tooltipText.text = $"<b>{itemName}</b>\n{itemDescription}";
            tooltip.transform.position = position + new Vector3(0, -150f, 0f);
        }
    }

    private void HideTooltip()
    {
        if (gameObject.activeSelf)
        {
            tooltip.SetActive(false);
        }
    }

    private void CreateInventorySlots()
    {
        int slotID = 0;

        for (int i = 0; i < playerInventory.maxHotbarSlots; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, hotbarPanel);
            int currentSlotID = slotID;
            slot.GetComponent<Button>().onClick.AddListener(() => OnSlotClicked(currentSlotID));

            slots.Add(slotID, slot);
            slot.name = $"Hotbar Slot {slotID}";
            ResetSlotUI(slot);
            slotID++;
        }

        for (int i = 0; i < playerInventory.maxSlots; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, inventoryPanel);
            int currentSlotID = slotID;
            slot.GetComponent<Button>().onClick.AddListener(() => OnSlotClicked(currentSlotID));

            EventTrigger trigger = slot.AddComponent<EventTrigger>();

            AddEventTrigger(trigger, EventTriggerType.PointerEnter, eventData =>
            {
                ItemStack item = playerInventory.GetItemInSlot(currentSlotID);
                if (item != null)
                {
                    Vector3 mousePosition = Input.mousePosition;
                    ShowTooltip(item.itemData.itemName, item.itemData.description, slot.transform.position);
                }
            });

            AddEventTrigger(trigger, EventTriggerType.PointerExit, eventData =>
            {
                HideTooltip();
            });

            slots.Add(slotID, slot);
            slot.name = $"Inventory Slot {slotID}";
            ResetSlotUI(slot);
            slotID++;
        }
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }



    private void ResetSlotUI(GameObject slot)
    {
        slot.transform.GetChild(0).GetComponent<Image>().sprite = null;
        slot.transform.GetChild(0).GetComponent<Image>().enabled = false;
        slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
    }

    public void UpdateInventoryUI()
    {
        foreach (var slot in slots.Values)
        {
            ResetSlotUI(slot);
        }

        foreach (var item in playerInventory.GetAllItemsWithIDs())
        {
            int slotID = item.Key;
            ItemStack stack = item.Value;

            if (slots.TryGetValue(slotID, out GameObject slot))
            {
                Image icon = slot.transform.GetChild(0).GetComponent<Image>();
                TextMeshProUGUI quantityText = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

                icon.sprite = stack.itemData.icon;
                icon.enabled = true;
                quantityText.text = stack.quantity > 1 ? stack.quantity.ToString() : "";
            }
        }
    }
    public void OnSlotClicked(int slotID)
    {
        Debug.Log(slotID);

        if (selectedSlotID == null)
        {
            // Début du déplacement
            if (playerInventory.GetItemInSlot(slotID) != null)
            {
                selectedSlotID = slotID;

                // Récupération des données visuelles de l'item
                Image slotImage = slots[slotID].transform.GetChild(0).GetComponent<Image>();
                Sprite itemSprite = slotImage.sprite;

                slotImage.sprite = null;
                slotImage.enabled = false;

                // Création dynamique de l'objet "floating item"
                floatingItem = new GameObject("Floating Item");
                floatingItem.transform.SetParent(transform.parent); // Ajout à la hiérarchie du canvas
                floatingItem.AddComponent<CanvasRenderer>();

                RectTransform floatingRect = floatingItem.AddComponent<RectTransform>();
                floatingRect.sizeDelta = new Vector2(slots[slotID].transform.GetChild(0).GetComponent<RectTransform>().rect.width, slots[slotID].transform.GetChild(0).GetComponent<RectTransform>().rect.height); // Taille de l'item visuel
                floatingRect.anchorMin = new Vector2(0.5f, 0.5f);
                floatingRect.anchorMax = new Vector2(0.5f, 0.5f);

                Image floatingImage = floatingItem.AddComponent<Image>();
                floatingImage.sprite = itemSprite;
                floatingImage.raycastTarget = false; // Empêche l'item de bloquer les clics
            }
        }
        else
        {
            // Fin du déplacement
            if (playerInventory.GetItemInSlot(slotID) == null || slotID == selectedSlotID)
            {
                playerInventory.MoveItem((int)selectedSlotID, slotID);
                selectedSlotID = null;

                // Suppression de l'objet flottant
                if (floatingItem != null)
                {
                    Destroy(floatingItem);
                    floatingItem = null;
                }

                UpdateInventoryUI();
            }
        }
    }
}
