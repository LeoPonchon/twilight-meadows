using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le système de drag and drop pour les objets de l'inventaire
/// </summary>
public class InventoryDragAndDrop : MonoBehaviour
{
    [SerializeField] private InventoryContainer playerInventory;
    [SerializeField] private InventoryUI inventoryUI;

    private ItemData floatingItemData;
    private int floatingItemQuantity = 0;
    private GameObject floatingItem;
    private int? selectedSlotID = null;

    private void Update()
    {
        if (floatingItem != null)
        {
            UpdateFloatingItemPosition();
        }
    }

    public void HandleFloatingItem()
    {
        if (floatingItem != null && selectedSlotID.HasValue)
        {
            // Récupérer le slot depuis le manager
            var slotObj = inventoryUI.GetSlotFromManager(selectedSlotID.Value);
            if (slotObj != null)
            {
                inventoryUI.SetSlotImage(slotObj, floatingItem.GetComponent<Image>().sprite);
            }
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

    public void HandleLeftClick(int slotID)
    {
        if (floatingItem == null)
        {
            StartFloatingItem(slotID);
        }
        else
        {
            CompleteFloatingItem(slotID);
        }
    }

    public void HandleRightClick(int slotID)
    {
        if (floatingItem == null)
        {
            StartFloatingItem(slotID, true);
        }
        else
        {
            CompleteFloatingItem(slotID, true);
        }
    }

    private void StartFloatingItem(int slotID, bool isRightClick = false)
    {
        if (playerInventory == null) return;

        var item = playerInventory.GetItemInSlot(slotID);
        if (item == null) return;

        floatingItemQuantity = isRightClick ? Mathf.CeilToInt(item.quantity / 2f) : item.quantity;
        selectedSlotID = slotID;
        floatingItemData = item.itemData;

        var slotObj = inventoryUI.GetSlotFromManager(slotID);
        if (slotObj == null) return;

        floatingItem = CreateFloatingItem(
            slotObj.transform.GetChild(0).GetComponent<Image>().sprite,
            slotObj.transform.GetChild(0).GetComponent<RectTransform>().rect.size
        );

        inventoryUI.SetSlotImage(slotObj, null);

        playerInventory.RemoveItemFromSlot(slotID, floatingItemQuantity);
        inventoryUI.UpdateInventoryUI();
    }

    private void CompleteFloatingItem(int slotID, bool isRightClick = false)
    {
        if (floatingItem == null || floatingItemData == null || playerInventory == null) return;

        var slotItem = playerInventory.GetItemInSlot(slotID);

        // Si le slot cible est vide ou contient un item similaire
        if (slotItem == null || (slotItem.itemData == floatingItemData && floatingItemData.isStackable))
        {
            int quantityToMove = isRightClick ? 1 : floatingItemQuantity;
            quantityToMove = Mathf.Min(quantityToMove, floatingItemQuantity);

            playerInventory.AddItemToSlot(slotID, floatingItemData, quantityToMove);
            floatingItemQuantity -= quantityToMove;

            if (floatingItemQuantity <= 0)
            {
                Destroy(floatingItem);
                floatingItem = null;
                floatingItemData = null;
                selectedSlotID = null;
            }

            inventoryUI.UpdateInventoryUI();
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
