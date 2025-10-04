using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Composant pour gérer les slots de magasin avec achat/vente
/// </summary>
public class ShopSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuration")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int buyPrice = 10;
    [SerializeField] private int sellPrice = 5;
    [SerializeField] private int stockQuantity = 99; // Quantité disponible en magasin (-1 = illimité)
    
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI priceText;
    
    [Header("Références Système")]
    [SerializeField] private Inventory playerInventory;
    
    private int currentStock;
    
    private void Awake()
    {
        InitializeReferences();
    }
    
    private void Start()
    {
        InitializeStock();
        UpdateUI();
    }
    
    private void InitializeReferences()
    {
        // Trouver automatiquement les références si elles ne sont pas assignées
        if (playerInventory == null)
            playerInventory = FindObjectOfType<Inventory>();
    }
    
    private void InitializeStock()
    {
        currentStock = stockQuantity;
    }
    
    // Interface IPointerClickHandler pour détecter les clics directement sur le slot
    public void OnPointerClick(PointerEventData eventData)
    {
        // Détecter le type de clic
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Clic gauche : Achat
            TryBuyItem();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Clic droit : Vente
            TrySellItem();
        }
    }
    
    private void TryBuyItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("ShopSlot: Aucun item configuré pour l'achat");
            return;
        }
        
        if (EconomyManager.Instance == null)
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
        if (!EconomyManager.Instance.CanSpend(buyPrice))
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
        
        if (EconomyManager.Instance == null)
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
        EconomyManager.Instance.Spend(buyPrice);
        
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
        EconomyManager.Instance.AddGold(sellPrice);
        
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
        if (quantityText != null)
        {
            if (itemData != null)
            {
                if (stockQuantity == -1)
                {
                    quantityText.text = "∞";
                }
                else
                {
                    quantityText.text = currentStock.ToString();
                }
            }
            else
            {
                quantityText.text = "";
            }
        }
        
        // Mettre à jour le prix
        if (priceText != null)
        {
            if (itemData != null)
            {
                priceText.text = $"A: {buyPrice} | V: {sellPrice}";
            }
            else
            {
                priceText.text = "";
            }
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
    
    // Méthodes pour obtenir les informations
    public ItemData GetItemData() => itemData;
    public int GetBuyPrice() => buyPrice;
    public int GetSellPrice() => sellPrice;
    public int GetCurrentStock() => currentStock;
    public bool HasStock() => stockQuantity == -1 || currentStock > 0;
}
