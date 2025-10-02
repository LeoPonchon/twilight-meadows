using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Coordonne tous les aspects de l'UI d'inventaire
/// </summary>
public class InventoryUI : MonoBehaviour
{
    #region Variables

    [Header("Références")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private PlayerInput playerInput;

    // Références aux composants de l'inventaire
    [SerializeField] private InventorySlotManager slotManager;
    private InventoryDragAndDrop dragAndDrop;
    private InventoryTooltip tooltip;
    private SlotSelector selector;

    #endregion

    #region Méthodes Unity

    /// <summary>
    /// Initialisation des composants
    /// </summary>
    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary>
    /// Création des slots et mise à jour de l'UI
    /// </summary>
    private void Start()
    {
        // Assurons-nous de créer les slots au démarrage
        if (slotManager != null)
        {
            if (slotManager == null)
            {
                Debug.LogError("slotManager n'est pas assigné à InventoryUI", this);
                return;
            }

            // Créer tous les slots d'inventaire (principaux et hotbar si nécessaire)
            CreateInventorySlots();

            // Configuration des écouteurs d'événements APRÈS la création des slots
            SetupEventListeners();

            // Forcer une mise à jour de l'UI
            if (playerInventory != null)
            {
                Debug.Log("Initialisation de l'inventaire UI - Items: " + playerInventory.GetAllItemsWithIDs().Count);
                UpdateInventory();
            }
        }
    }

    /// <summary>
    /// Activation du mode UI
    /// </summary>
    private void OnEnable()
    {
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI");
    }

    /// <summary>
    /// Retour au mode jeu
    /// </summary>
    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Game");

        HideTooltipAndSelector();
    }

    /// <summary>
    /// Gestion des inputs
    /// </summary>
    private void Update()
    {
        if (playerInput == null) return;

        if (playerInput.actions["OpenInventory"].triggered)
        {
            if (dragAndDrop != null)
                dragAndDrop.HandleFloatingItem();

            HideTooltipAndSelector();
        }
    }

    #endregion

    #region Initialisation

    /// <summary>
    /// Initialisation des composants de l'inventaire
    /// </summary>
    private void InitializeComponents()
    {
        if (slotManager == null)
        {
            Debug.LogError("InventorySlotManager component missing on InventoryUI GameObject!", this);
        }

        dragAndDrop = GetComponent<InventoryDragAndDrop>();
        if (dragAndDrop == null)
        {
            Debug.LogError("InventoryDragAndDrop component missing on InventoryUI GameObject!", this);
        }

        tooltip = GetComponent<InventoryTooltip>();
        if (tooltip == null)
        {
            Debug.LogError("InventoryTooltip component missing on InventoryUI GameObject!", this);
        }

        selector = GetComponent<SlotSelector>();
        if (selector == null)
        {
            Debug.LogError("SlotSelector component missing on InventoryUI GameObject!", this);
        }

        if (playerInventory == null)
        {
            Debug.LogError("playerInventory is not assigned in InventoryUI!", this);
        }
    }

    /// <summary>
    /// Configuration des écouteurs d'événements
    /// </summary>
    private void SetupEventListeners()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += UpdateInventory;
            // Ne pas appeler UpdateInventory() ici, on le fera explicitement après
        }

        if (slotManager != null)
        {
            slotManager.OnSlotHover += OnSlotHover;
            slotManager.OnSlotExit += OnSlotExit;
            slotManager.OnSlotLeftClick += OnSlotLeftClick;
            slotManager.OnSlotRightClick += OnSlotRightClick;
        }
    }

    #endregion

    #region Méthodes publiques

    /// <summary>
    /// Crée tous les slots d'inventaire (peut être appelé par d'autres scripts)
    /// </summary>
    public void CreateInventorySlots()
    {
        if (slotManager != null)
        {
            // Si les slots de hotbar sont déjà créés par HotbarController, on ne crée que les slots d'inventaire
            if (HotbarSlotsAlreadyCreated())
            {
                slotManager.CreateMainInventorySlots();
                Debug.Log("Création uniquement des slots d'inventaire (hotbar déjà créée)");
            }
            else
            {
                // Sinon on crée tout
                slotManager.CreateInventorySlots();
                Debug.Log("Création de tous les slots (hotbar + inventaire)");
            }
        }
        else
        {
            Debug.LogError("Impossible de créer les slots: slotManager est null", this);
        }
    }

    #endregion

    #region Méthodes privées

    /// <summary>
    /// Met à jour l'UI de l'inventaire
    /// </summary>
    private void UpdateInventory()
    {
        if (slotManager != null)
        {
            slotManager.UpdateInventoryUI();
            Debug.Log("UI d'inventaire mise à jour");
        }
    }

    /// <summary>
    /// Cache le tooltip et le sélecteur
    /// </summary>
    private void HideTooltipAndSelector()
    {
        if (tooltip != null)
            tooltip.HideTooltip();

        if (selector != null)
            selector.HideSelector();
    }

    /// <summary>
    /// Vérifie si les slots de hotbar ont déjà été créés (par HotbarController)
    /// </summary>
    private bool HotbarSlotsAlreadyCreated()
    {
        // Rechercher un HotbarController dans la scène
        HotbarController hotbarController = FindObjectOfType<HotbarController>();
        return hotbarController != null && hotbarController.isActiveAndEnabled;
    }

    #endregion

    #region Gestionnaires d'événements

    /// <summary>
    /// Gère le survol d'un slot
    /// </summary>
    private void OnSlotHover(int slotID, GameObject slot, bool isHotbar)
    {
        if (!isHotbar && playerInventory != null)
        {
            var item = playerInventory.GetItemInSlot(slotID);
            if (item != null && tooltip != null)
                tooltip.ShowTooltip(item.itemData.itemName, item.itemData.description, slot.transform.position);
        }

        if (selector != null)
            selector.ShowSelector(slot);
    }

    /// <summary>
    /// Gère la sortie d'un slot
    /// </summary>
    private void OnSlotExit(int slotID, GameObject slot, bool isHotbar)
    {
        HideTooltipAndSelector();
    }

    /// <summary>
    /// Gère le clic gauche sur un slot
    /// </summary>
    private void OnSlotLeftClick(int slotID)
    {
        if (dragAndDrop != null)
            dragAndDrop.HandleLeftClick(slotID);
    }

    /// <summary>
    /// Gère le clic droit sur un slot
    /// </summary>
    private void OnSlotRightClick(int slotID)
    {
        // 1) Comportement existant de drag & drop
        if (dragAndDrop != null)
            dragAndDrop.HandleRightClick(slotID);

        // 2) Vente si dans une VendorZone et si l'item a un prix
        TrySellItemInSlot(slotID);
    }

    private void TrySellItemInSlot(int slotID)
    {
        if (!VendorZone.IsPlayerInside)
            return;
        if (playerInventory == null)
            return;

        var stack = playerInventory.GetItemInSlot(slotID);
        if (stack == null || stack.itemData == null)
            return;

        var data = stack.itemData;
        if (data.sellPrice <= 0)
            return;

        // Vendre une unité par clic droit
        playerInventory.RemoveItemFromSlot(slotID, 1);
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddGold(data.sellPrice);
        }
        else
        {
            Debug.LogWarning("EconomyManager.Instance est null: aucun or ajouté");
        }
    }

    #endregion
}
