using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Gestionnaire d'économie simple pour l'or du joueur.
/// - Singleton accessible via Instance
/// - Evénement OnGoldChanged pour mettre à jour un HUD éventuel
/// - Méthodes AddGold / Spend / CanSpend
/// </summary>
public class EconomyManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Or initial du joueur")]
    [SerializeField] private int startingGold = 0;

    [Header("UI (optionnel)")]
    [Tooltip("Affichage texte de l'or (facultatif)")]
    [SerializeField] private TextMeshProUGUI goldText;

    /// <summary>
    /// Or actuel du joueur
    /// </summary>
    public int Gold => wallet != null ? wallet.Gold : 0;

    /// <summary>
    /// Ev��nement déclenché à chaque changement d'or (passe la valeur actuelle)
    /// </summary>
    public event Action<int> OnGoldChanged;

    private GoldWallet wallet;

    private void Awake()
    {
        wallet = new GoldWallet(startingGold);
        wallet.GoldChanged += OnWalletGoldChanged;
        UpdateUI(wallet.Gold);
    }

    private void OnDestroy()
    {
        if (wallet != null)
        {
            wallet.GoldChanged -= OnWalletGoldChanged;
        }
    }

    private void OnWalletGoldChanged(int gold)
    {
        OnGoldChanged?.Invoke(gold);
        UpdateUI(gold);
    }

    /// <summary>
    /// Ajoute de l'or
    /// </summary>
    public void AddGold(int amount)
    {
        wallet?.Add(amount);
    }

    /// <summary>
    /// Indique si on peut dépenser une certaine somme
    /// </summary>
    public bool CanSpend(int amount)
    {
        return wallet != null && wallet.CanSpend(amount);
    }

    /// <summary>
    /// Dépense de l'or si possible
    /// </summary>
    public bool Spend(int amount)
    {
        return wallet != null && wallet.Spend(amount);
    }

    /// <summary>
    /// Fixe directement la valeur d'or (>=0)
    /// </summary>
    public void SetGold(int amount)
    {
        wallet?.Set(amount);
    }

    /// <summary>
    /// Affecte un label pour afficher l'or (facultatif)
    /// </summary>
    public void BindGoldUI(TextMeshProUGUI ui)
    {
        goldText = ui;
        UpdateUI(Gold);
    }

    private void UpdateUI(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"{gold}g";
        }
    }
}
