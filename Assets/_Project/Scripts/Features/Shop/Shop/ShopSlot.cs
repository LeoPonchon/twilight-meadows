using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int buyPrice = 10;
    [SerializeField] private int sellPrice = 5;
    [SerializeField] private int stockQuantity = 99;

    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemQuantity;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemValueBuy;
    [SerializeField] private TextMeshProUGUI itemValueSell;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    [Header("Services")]
    [SerializeField] private InventoryContainer playerInventory;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private SceneContext sceneContext;

    private int stock;

    private bool InfiniteStock => stockQuantity < 0;
    private bool HasStockAvailable => InfiniteStock || stock > 0;

    private void Awake()
    {
        ResolveReferences();
        stock = stockQuantity;
    }

    private void OnEnable()
    {
        if (buyButton != null) buyButton.onClick.AddListener(TryBuy);
        if (sellButton != null) sellButton.onClick.AddListener(TrySell);
        Refresh();
    }

    private void OnDisable()
    {
        if (buyButton != null) buyButton.onClick.RemoveListener(TryBuy);
        if (sellButton != null) sellButton.onClick.RemoveListener(TrySell);
    }

    public void SetItemData(ItemData newItemData)
    {
        itemData = newItemData;
        Refresh();
    }

    public void SetPrices(int newBuyPrice, int newSellPrice)
    {
        buyPrice = newBuyPrice;
        sellPrice = newSellPrice;
        Refresh();
    }

    public void SetStock(int newStock)
    {
        stockQuantity = newStock;
        stock = newStock;
        Refresh();
    }

    public void RefreshButtonStates() => Refresh();

    public ItemData GetItemData() => itemData;
    public int GetBuyPrice() => buyPrice;
    public int GetSellPrice() => sellPrice;
    public int GetCurrentStock() => stock;
    public bool HasStock() => HasStockAvailable;

    private void TryBuy()
    {
        if (itemData == null || playerInventory == null || economyManager == null) return;
        bool canAdd = playerInventory.CanAddItem(itemData, 1);
        if (!ShopRules.CanBuy(economyManager.Gold, buyPrice, HasStockAvailable, canAdd)) return;
        if (!economyManager.Spend(buyPrice)) return;

        playerInventory.AddItem(itemData, 1);
        if (!InfiniteStock) stock--;
        Refresh();
    }

    private void TrySell()
    {
        if (itemData == null || playerInventory == null || economyManager == null) return;
        if (!ShopRules.CanSell(playerInventory.HasItem(itemData, 1))) return;

        playerInventory.RemoveItem(itemData, 1);
        economyManager.AddGold(sellPrice);
        Refresh();
    }

    private void Refresh()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData != null ? itemData.icon : null;
            itemIcon.enabled = itemData != null;
        }

        if (itemName != null) itemName.text = itemData != null ? itemData.itemName : "";
        if (itemQuantity != null) itemQuantity.text = itemData == null ? "" : InfiniteStock ? "inf" : stock.ToString();
        if (itemValueBuy != null) itemValueBuy.text = itemData != null ? $"{buyPrice}G" : "";
        if (itemValueSell != null) itemValueSell.text = itemData != null ? $"{sellPrice}G" : "";

        bool canAdd = playerInventory != null && itemData != null && playerInventory.CanAddItem(itemData, 1);
        bool canBuy = itemData != null && economyManager != null && ShopRules.CanBuy(economyManager.Gold, buyPrice, HasStockAvailable, canAdd);
        bool canSell = itemData != null && playerInventory != null && ShopRules.CanSell(playerInventory.HasItem(itemData, 1));

        if (buyButton != null) buyButton.interactable = canBuy;
        if (sellButton != null) sellButton.interactable = canSell;
    }

    private void ResolveReferences()
    {
        if (sceneContext == null) sceneContext = FindObjectOfType<SceneContext>();
        if (playerInventory == null) playerInventory = sceneContext != null ? sceneContext.Get<InventoryContainer>() : null;
        if (economyManager == null) economyManager = sceneContext != null ? sceneContext.Get<EconomyManager>() : null;
    }
}
