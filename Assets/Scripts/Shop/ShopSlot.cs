using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Composant pour gérer les slots de magasin avec achat/vente
/// </summary>
public class ShopSlot : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int buyPrice = 10;
    [SerializeField] private int sellPrice = 5;
    [SerializeField] private int stockQuantity = 99; // Quantité disponible en magasin (-1 = illimité)
    
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemQuantity;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemValueBuy;
    [SerializeField] private TextMeshProUGUI itemValueSell;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;
    
    [Header("Références Système")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private SceneContext sceneContext;
    
    private int currentStock;
    
    private void Awake()
    {
        InitializeReferences();
    }
    
    private void Start()
    {
        InitializeStock();
        UpdateUI();
        SetupButton();
    }
    
    private void InitializeReferences()
    {
        if (sceneContext == null)
            sceneContext = FindObjectOfType<SceneContext>();
        if (sceneContext == null)
        {
            Debug.LogError("ShopSlot: Missing SceneContext in scene.", this);
            enabled = false;
            return;
        }
        // Trouver automatiquement les références si elles ne sont pas assignées
        if (playerInventory == null)
            playerInventory = sceneContext.GetRequired<Inventory>(this, nameof(playerInventory));

        if (economyManager == null)
            economyManager = sceneContext.GetRequired<EconomyManager>(this, nameof(economyManager));
            
            
        // Trouver automatiquement les boutons s'ils ne sont pas assignés
        if (buyButton == null)
            buyButton = transform.Find("BuyButton")?.GetComponent<Button>();
            
        if (sellButton == null)
            sellButton = transform.Find("SellButton")?.GetComponent<Button>();
            
        // Trouver automatiquement les textes s'ils ne sont pas assignés
        if (itemQuantity == null)
            itemQuantity = transform.Find("item_quantity")?.GetComponent<TextMeshProUGUI>();
            
        if (itemName == null)
            itemName = transform.Find("item_name")?.GetComponent<TextMeshProUGUI>();
            
        if (itemValueBuy == null)
            itemValueBuy = transform.Find("item_value_buy")?.GetComponent<TextMeshProUGUI>();
            
        if (itemValueSell == null)
            itemValueSell = transform.Find("item_value_sell")?.GetComponent<TextMeshProUGUI>();
            
        if (itemIcon == null)
            itemIcon = transform.Find("item_icon")?.GetComponent<Image>();
    }
    
    private void InitializeStock()
    {
        currentStock = stockQuantity;
    }
    
    private void SetupButton()
    {
        // Configurer les boutons BUY et SELL séparés
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => TryBuyItem());
        }
        
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => TrySellItem());
        }
    }
    
    private void TryBuyItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("ShopSlot: Aucun item configuré pour l'achat");
            return;
        }
        
        if (economyManager == null)
        {
            Debug.LogError("ShopSlot: EconomyManager non trouvé");
            return;
        }
        
        if (playerInventory == null)
        {
            Debug.LogError("ShopSlot: Inventory non trouvé");
            return;
        }
        
        // Vérifier si le joueur a assez d'argent
        if (!economyManager.CanSpend(buyPrice))
        {
            Debug.Log($"Pas assez d'argent pour acheter {itemData.itemName}. Coût: {buyPrice}");
            return;
        }
        
        // Vérifier le stock
        if (currentStock <= 0 && stockQuantity != -1)
        {
            Debug.Log($"{itemData.itemName} en rupture de stock");
            return;
        }
        
        // Vérifier si l'inventaire a de la place
        if (!playerInventory.CanAddItem(itemData, 1))
        {
            Debug.Log("Inventaire plein, impossible d'acheter");
            return;
        }
        
        // Effectuer l'achat
        PerformBuy();
    }
    
    private void TrySellItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("ShopSlot: Aucun item configuré pour la vente");
            return;
        }
        
        if (economyManager == null)
        {
            Debug.LogError("ShopSlot: EconomyManager non trouvé");
            return;
        }
        
        if (playerInventory == null)
        {
            Debug.LogError("ShopSlot: Inventory non trouvé");
            return;
        }
        
        // Vérifier si le joueur possède cet item
        if (!playerInventory.HasItem(itemData, 1))
        {
            Debug.Log($"Vous ne possédez pas {itemData.itemName}");
            return;
        }
        
        // Effectuer la vente
        PerformSell();
    }
    
    private void PerformBuy()
    {
        // Retirer l'argent
        economyManager.Spend(buyPrice);
        
        // Ajouter l'item à l'inventaire
        playerInventory.AddItem(itemData, 1);
        
        // Réduire le stock si limité
        if (stockQuantity != -1)
        {
            currentStock--;
        }
        
        UpdateUI();
        Debug.Log($"Achat réussi: {itemData.itemName} pour {buyPrice} pièces");
    }
    
    private void PerformSell()
    {
        // Retirer l'item de l'inventaire
        playerInventory.RemoveItem(itemData, 1);
        
        // Ajouter l'argent
        economyManager.AddGold(sellPrice);
        
        UpdateUI();
        Debug.Log($"Vente réussie: {itemData.itemName} pour {sellPrice} pièces");
    }
    
    private void UpdateUI()
    {
        // Mettre à jour l'icône
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData != null ? itemData.icon : null;
            itemIcon.enabled = itemData != null;
        }
        
        // Mettre à jour la quantité
        if (itemQuantity != null)
        {
            if (itemData != null)
            {
                if (stockQuantity == -1)
                {
                    itemQuantity.text = "∞";
                }
                else
                {
                    itemQuantity.text = currentStock.ToString();
                }
            }
            else
            {
                itemQuantity.text = "";
            }
        }
        
        // Mettre à jour le nom de l'item
        if (itemName != null)
        {
            if (itemData != null)
            {
                itemName.text = itemData.itemName;
            }
            else
            {
                itemName.text = "";
            }
        }
        
        // Mettre à jour les prix d'achat et de vente
        if (itemValueBuy != null)
        {
            if (itemData != null)
            {
                itemValueBuy.text = $"{buyPrice}G";
            }
            else
            {
                itemValueBuy.text = "";
            }
        }
        
        if (itemValueSell != null)
        {
            if (itemData != null)
            {
                itemValueSell.text = $"{sellPrice}G";
            }
            else
            {
                itemValueSell.text = "";
            }
        }
        
        // Mettre à jour l'état des boutons
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        bool hasStock = stockQuantity == -1 || currentStock > 0;
        bool canAdd = playerInventory != null && itemData != null && playerInventory.CanAddItem(itemData, 1);
        bool canBuy = itemData != null &&
                      economyManager != null &&
                      ShopRules.CanBuy(economyManager.Gold, buyPrice, hasStock, canAdd);
                     
        bool canSell = itemData != null &&
                       playerInventory != null &&
                       ShopRules.CanSell(playerInventory.HasItem(itemData));
        
        if (buyButton != null)
        {
            buyButton.interactable = canBuy;
        }
        
        if (sellButton != null)
        {
            sellButton.interactable = canSell;
        }
    }
    
    // Méthodes publiques pour configuration
    public void SetItemData(ItemData newItemData)
    {
        itemData = newItemData;
        UpdateUI();
    }
    
    public void SetPrices(int newBuyPrice, int newSellPrice)
    {
        buyPrice = newBuyPrice;
        sellPrice = newSellPrice;
        UpdateUI();
    }
    
    public void SetStock(int newStock)
    {
        stockQuantity = newStock;
        currentStock = newStock;
        UpdateUI();
    }
    
    /// <summary>
    /// Méthode publique pour forcer la mise à jour des boutons (utile depuis l'extérieur)
    /// </summary>
    public void RefreshButtonStates()
    {
        UpdateButtonStates();
    }
    
    
    // Méthodes pour obtenir les informations
    public ItemData GetItemData() => itemData;
    public int GetBuyPrice() => buyPrice;
    public int GetSellPrice() => sellPrice;
    public int GetCurrentStock() => currentStock;
    public bool HasStock() => stockQuantity == -1 || currentStock > 0;
}
