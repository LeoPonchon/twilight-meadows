using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le système de drag and drop pour les objets de l'inventaire
/// </summary>
public class InventoryDragAndDrop : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private InventorySlotManager slotManager;

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
            slotManager.SetSlotImage(selectedSlotID.Value, floatingItem.GetComponent<Image>().sprite);
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

        var slotObj = slotManager.GetSlot(slotID);
        if (slotObj == null) return;

        floatingItem = CreateFloatingItem(
            slotObj.transform.GetChild(0).GetComponent<Image>().sprite,
            slotObj.transform.GetChild(0).GetComponent<RectTransform>().rect.size
        );

        slotManager.SetSlotImage(slotID, null);

        playerInventory.RemoveItemFromSlot(slotID, floatingItemQuantity);
        slotManager.UpdateInventoryUI();
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

            slotManager.UpdateInventoryUI();
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