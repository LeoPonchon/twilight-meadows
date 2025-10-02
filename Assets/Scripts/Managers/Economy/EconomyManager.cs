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
    public static EconomyManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Or initial du joueur")]
    [SerializeField] private int startingGold = 0;

    [Header("UI (optionnel)")]
    [Tooltip("Affichage texte de l'or (facultatif)")]
    [SerializeField] private TextMeshProUGUI goldText;

    /// <summary>
    /// Or actuel du joueur
    /// </summary>
    public int Gold { get; private set; }

    /// <summary>
    /// Ev��nement déclenché à chaque changement d'or (passe la valeur actuelle)
    /// </summary>
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        Gold = Mathf.Max(0, startingGold);
        UpdateUI();
    }

    /// <summary>
    /// Ajoute de l'or
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
        UpdateUI();
    }

    /// <summary>
    /// Indique si on peut dépenser une certaine somme
    /// </summary>
    public bool CanSpend(int amount)
    {
        return amount >= 0 && Gold >= amount;
    }

    /// <summary>
    /// Dépense de l'or si possible
    /// </summary>
    public bool Spend(int amount)
    {
        if (!CanSpend(amount)) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        UpdateUI();
        return true;
    }

    /// <summary>
    /// Fixe directement la valeur d'or (>=0)
    /// </summary>
    public void SetGold(int amount)
    {
        Gold = Mathf.Max(0, amount);
        OnGoldChanged?.Invoke(Gold);
        UpdateUI();
    }

    /// <summary>
    /// Affecte un label pour afficher l'or (facultatif)
    /// </summary>
    public void BindGoldUI(TextMeshProUGUI ui)
    {
        goldText = ui;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = $"{Gold}g";
        }
    }
}
