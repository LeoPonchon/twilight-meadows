using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manager principal pour l'inventaire et la hotbar - gÃ¨re la logique mÃ©tier
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("RÃ©fÃ©rences")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private SceneContext sceneContext;

    [Header("Input")]
    [SerializeField] private string hotbarScrollActionName = "ScrollWheel";
    
    [Header("Hotbar Settings")]
    [SerializeField] private int currentHotbarSlot = 0;
    [SerializeField] private bool logHotbarBindings = false;
    
    [Header("UI Prefabs")]
    [SerializeField] private GameObject hotbarSelectorPrefab;
    [SerializeField] private GameObject slotSelectorPrefab;
    
    [Header("UI References")]
    [SerializeField] private Transform uiParent; // Parent UI pour instancier les sÃ©lecteurs
    
    [Header("Slot Manager")]
    [SerializeField] private InventorySlotManager slotManager;

    [Header("Optional scene registries")]
    [SerializeField] private MonoBehaviour[] shops;
    
    // Variables pour gÃ©rer l'Ã©tat de l'inventaire
    private bool isInventoryOpen = false;
    
    // SÃ©lecteurs
    private GameObject hotbarSelectorInstance;
    private GameObject slotSelectorInstance;

    private InputAction hotbarScrollAction;
    
    // Ã‰vÃ©nements
    public System.Action<int> OnHotbarSlotChanged;
    public System.Action<ItemStack> OnHotbarItemUsed;
    public System.Action<bool> OnInventoryStateChanged;
    
    public int CurrentHotbarSlot => currentHotbarSlot;
    public Inventory PlayerInventory => playerInventory;
    public bool IsInventoryOpen => isInventoryOpen;
    
    private void Awake()
    {
        ValidateReferences();
    }
    
    private void Start()
    {
        SetupInputEvents();
        BindHotbarScrollAction();
        InitializeSlotManager();
        // Initialiser les sÃ©lecteurs aprÃ¨s la crÃ©ation des slots
        StartCoroutine(InitializeSelectorsAfterSlots());
    }
    
    private System.Collections.IEnumerator InitializeSelectorsAfterSlots()
    {
        // Attendre une frame pour que les slots soient crÃ©Ã©s
        yield return null;
        InitializeSelectors();
    }
    
    private void Update()
    {
        // Hotbar scroll handled via Input System callback.
    }

    private void OnDestroy()
    {
        UnbindHotbarScrollAction();
    }
    private void SetupInputEvents()
    {
        if (playerInput == null || playerInventory == null) return;

        // S'abonner aux évènements d'input pour les slots de hotbar
        for (int i = 0; i < playerInventory.maxHotbarSlots; i++)
        {
            var action = playerInput.actions.FindAction($"HotbarSlot{i + 1}");
            if (action != null)
            {
                action.performed += _ => SelectHotbarSlot(i);
                if (logHotbarBindings)
                {
                    Debug.Log($"HotbarSlot{i + 1} action trouvée et connectée");
                }
            }
            else
            {
                if (logHotbarBindings)
                {
                    Debug.LogWarning($"HotbarSlot{i + 1} action non trouvée!");
                }
            }
        }
    }

    private void BindHotbarScrollAction()
    {
        if (playerInput == null || playerInput.actions == null) return;

        hotbarScrollAction = playerInput.actions.FindAction(hotbarScrollActionName, throwIfNotFound: false);
        if (hotbarScrollAction == null)
        {
            Debug.LogError($"InventoryManager: Missing InputAction '{hotbarScrollActionName}'.", this);
            return;
        }

        hotbarScrollAction.performed += OnHotbarScrollPerformed;
    }

    private void UnbindHotbarScrollAction()
    {
        if (hotbarScrollAction == null) return;
        hotbarScrollAction.performed -= OnHotbarScrollPerformed;
        hotbarScrollAction = null;
    }

    private void OnHotbarScrollPerformed(InputAction.CallbackContext ctx)
    {
        if (playerInput == null || playerInventory == null) return;

        // Ne pas permettre le scroll de hotbar en mode UI
        if (playerInput.currentActionMap != null && playerInput.currentActionMap.name == "UI") return;

        float scrollInput = 0f;
        if (ctx.valueType == typeof(float))
        {
            scrollInput = ctx.ReadValue<float>();
        }
        else if (ctx.valueType == typeof(Vector2))
        {
            scrollInput = ctx.ReadValue<Vector2>().y;
        }

        if (scrollInput == 0f) return;

        if (scrollInput < 0f)
        {
            currentHotbarSlot = (currentHotbarSlot + 1) % playerInventory.maxHotbarSlots;
        }
        else
        {
            currentHotbarSlot = (currentHotbarSlot - 1 + playerInventory.maxHotbarSlots) % playerInventory.maxHotbarSlots;
        }

        OnHotbarSlotChanged?.Invoke(currentHotbarSlot);
        UpdateHotbarSelectorPosition();
    }

    private void SelectHotbarSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < playerInventory.maxHotbarSlots)
        {
            Debug.Log($"SÃ©lection du slot hotbar {slotIndex}");
            currentHotbarSlot = slotIndex;
            OnHotbarSlotChanged?.Invoke(currentHotbarSlot);
            UpdateHotbarSelectorPosition();
        }
    }
    
    public GameObject GetSlot(int slotID)
    {
        return slotManager != null ? slotManager.GetSlot(slotID) : null;
    }
    
    public Dictionary<int, GameObject> GetAllSlots()
    {
        return slotManager != null ? slotManager.AllSlots : new Dictionary<int, GameObject>();
    }
    
    public ItemStack GetCurrentHotbarItem()
    {
        return playerInventory.GetItemInSlot(currentHotbarSlot);
    }
    
    public bool UseCurrentHotbarItem()
    {
        var item = GetCurrentHotbarItem();
        if (item != null && item.itemInstance != null)
        {
            bool success = false;
            
            if (item.itemInstance is ToolInstance toolInstance)
            {
                success = toolInstance.Use();
            }
            else if (item.itemInstance is WateringCanInstance wateringCan)
            {
                success = wateringCan.Use();
            }
            
            if (success)
            {
                OnHotbarItemUsed?.Invoke(item);
            }
            
            return success;
        }
        return false;
    }
    
    public int GetCurrentHotbarSlot()
    {
        return currentHotbarSlot;
    }
    
    public void SetCurrentHotbarSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < playerInventory.maxHotbarSlots)
        {
            currentHotbarSlot = slotIndex;
            OnHotbarSlotChanged?.Invoke(currentHotbarSlot);
        }
    }
    
    private void ValidateReferences()
    {
        if (sceneContext == null)
        {
            sceneContext = FindObjectOfType<SceneContext>();
        }

        if (playerInventory == null)
        {
            playerInventory = sceneContext != null ? sceneContext.GetRequired<Inventory>(this, nameof(playerInventory)) : null;
            if (playerInventory == null)
            {
                Debug.LogError("playerInventory is not assigned and could not be found automatically in InventoryManager!", this);
            }
        }
        
        if (playerInput == null)
        {
            playerInput = sceneContext != null ? sceneContext.PlayerInput : null;
            if (playerInput == null)
            {
                Debug.LogError("playerInput is not assigned and could not be found automatically in InventoryManager!", this);
            }
        }
        
        if (inventoryUI == null)
        {
            inventoryUI = sceneContext != null ? sceneContext.GetRequired<InventoryUI>(this, nameof(inventoryUI)) : null;
            if (inventoryUI == null)
            {
                Debug.LogError("inventoryUI is not assigned and could not be found automatically in InventoryManager!", this);
            }
        }
        
        if (slotManager == null)
        {
            slotManager = GetComponent<InventorySlotManager>();
            if (slotManager == null)
            {
                Debug.LogError("slotManager is not assigned and could not be found automatically in InventoryManager!", this);
            }
        }
        
        if (uiParent == null)
        {
            // Essayer de trouver un Canvas dans la scÃ¨ne
            Canvas canvas = sceneContext != null ? sceneContext.UiCanvas : null;
            if (canvas != null)
            {
                uiParent = canvas.transform;
                Debug.LogWarning("uiParent was not assigned, using Canvas as fallback!", this);
            }
            else
            {
                Debug.LogError("uiParent is not assigned and no Canvas found in scene!", this);
            }
        }
    }
    
    
    public void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }
    
    public void OpenInventory()
    {
        // Fermer le shop s'il est ouvert
        CloseShopIfOpen();
        
        isInventoryOpen = true;
        
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }
        
        // Mettre Ã  jour la visibilitÃ© des sÃ©lecteurs
        UpdateHotbarSelectorPosition();
        
        OnInventoryStateChanged?.Invoke(true);
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Game");
        }
        
        // Mettre Ã  jour la visibilitÃ© des sÃ©lecteurs
        UpdateHotbarSelectorPosition();
        
        OnInventoryStateChanged?.Invoke(false);
    }
    
    public void ForceGameMode()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Game");
        }
        isInventoryOpen = false;
        UpdateHotbarSelectorPosition();
        OnInventoryStateChanged?.Invoke(false);
    }
    
    private void InitializeSlotManager()
    {
        if (slotManager != null)
        {
            slotManager.Initialize(playerInventory, inventoryUI);
            slotManager.CreateInventorySlots();
            
            // Forcer la mise Ã  jour de l'UI aprÃ¨s la crÃ©ation des slots
            if (inventoryUI != null)
            {
                inventoryUI.UpdateInventoryUI();
            }
        }
    }
    
    private void InitializeSelectors()
    {
        InitializeHotbarSelector();
        InitializeSlotSelector();
    }
    
    private void InitializeHotbarSelector()
    {
        if (hotbarSelectorPrefab == null) 
        {
            Debug.LogError("InventoryManager: Cannot initialize hotbar selector - missing prefab!");
            return;
        }
        
        if (uiParent == null)
        {
            Debug.LogError("InventoryManager: Cannot initialize hotbar selector - missing UI parent!");
            return;
        }
        
        // VÃ©rifier que les slots de hotbar existent
        if (slotManager == null || slotManager.AllSlots.Count == 0)
        {
            Debug.LogError("InventoryManager: Cannot initialize hotbar selector - no slots created yet!");
            return;
        }
        
        // CrÃ©er le sÃ©lecteur sous le parent UI
        hotbarSelectorInstance = Instantiate(hotbarSelectorPrefab, uiParent);
        hotbarSelectorInstance.name = "HotbarSelector";
        
        // Configurer le sÃ©lecteur
        var image = hotbarSelectorInstance.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }
        
        // S'assurer que le sÃ©lecteur est au-dessus des autres Ã©lÃ©ments
        Canvas selectorCanvas = hotbarSelectorInstance.GetComponent<Canvas>();
        if (selectorCanvas == null)
        {
            selectorCanvas = hotbarSelectorInstance.AddComponent<Canvas>();
        }
        selectorCanvas.overrideSorting = true;
        selectorCanvas.sortingOrder = 601;
        
        // Activer le sÃ©lecteur
        hotbarSelectorInstance.SetActive(true);
        
        UpdateHotbarSelectorPosition();
    }
    
    private void UpdateHotbarSelectorPosition()
    {
        if (hotbarSelectorInstance == null) 
        {
            Debug.LogError("Hotbar selector not initialized!");
            return;
        }
        
        if (slotManager != null && slotManager.AllSlots.TryGetValue(currentHotbarSlot, out GameObject slot))
        {
            hotbarSelectorInstance.transform.position = slot.transform.position;
            UpdateHotbarSelectorVisibility();
        }
        else
        {
            Debug.LogWarning($"Slot {currentHotbarSlot} not found! Available slots: {slotManager?.AllSlots.Count ?? 0}");
        }
    }
    
    private void UpdateHotbarSelectorVisibility()
    {
        if (hotbarSelectorInstance == null) return;
        
        // Afficher le sÃ©lecteur seulement en mode Game
        bool shouldShow = playerInput != null && playerInput.currentActionMap != null && playerInput.currentActionMap.name == "Game";
        hotbarSelectorInstance.SetActive(shouldShow);
    }
    
    
    private void InitializeSlotSelector()
    {
        if (slotSelectorPrefab == null) return;
        
        if (uiParent == null)
        {
            Debug.LogError("InventoryManager: Cannot initialize slot selector - missing UI parent!");
            return;
        }
        
        slotSelectorInstance = Instantiate(slotSelectorPrefab, uiParent);
        slotSelectorInstance.name = "SlotSelector";
        
        // Configurer le sÃ©lecteur pour qu'il n'affecte pas le layout
        RectTransform rectTransform = slotSelectorInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        // S'assurer que le sÃ©lecteur est au-dessus des autres Ã©lÃ©ments
        Canvas selectorCanvas = slotSelectorInstance.GetComponent<Canvas>();
        if (selectorCanvas == null)
        {
            selectorCanvas = slotSelectorInstance.AddComponent<Canvas>();
        }
        selectorCanvas.overrideSorting = true;
        selectorCanvas.sortingOrder = 600; // Juste en dessous du hotbar selector
        
        // DÃ©sactiver par dÃ©faut
        slotSelectorInstance.SetActive(false);
    }
    
    public GameObject GetSlotSelectorInstance()
    {
        return slotSelectorInstance;
    }
    
    public void ShowSlotSelector(GameObject slot)
    {
        // Ne pas afficher le sÃ©lecteur de slot en mode jeu
        if (playerInput != null && playerInput.currentActionMap.name == "Game") return;
        
        if (slotSelectorInstance != null && slot != null)
        {
            slotSelectorInstance.SetActive(true);
            slotSelectorInstance.transform.position = slot.transform.position;
            slotSelectorInstance.transform.SetAsLastSibling();
        }
    }
    
    public void HideSlotSelector()
    {
        if (slotSelectorInstance != null)
            slotSelectorInstance.SetActive(false);
    }
    
    private void CloseShopIfOpen()
    {
        // Fermer les shops cÃ¢blÃ©s dans la scÃ¨ne (pas de scan global par dÃ©faut)
        if (shops == null || shops.Length == 0) return;

        var candidateBehaviours = shops;
        foreach (var behaviour in candidateBehaviours)
        {
            if (behaviour is not IShopUi shopUi) continue;
            if (!shopUi.IsShopOpen) continue;
            shopUi.CloseShop();
        }
    }
}





