using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Composant pour les NPCs vendeurs qui ouvrent un panel de magasin
/// </summary>
public class NPCVendor : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private string interactionActionName = "Interact";
    
    private PlayerInput playerInput;
    private bool isPlayerInRange = false;
    private bool isShopOpen = false;
    private InputAction interactAction;
    private InputAction closeMenuAction;
    
    private void Awake()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("NPCVendor: PlayerInput non trouvé dans la scène!");
        }
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("NPCVendor: Panel de magasin non assigné!");
        }
    }
    
    private void Start()
    {
        SetupInputEvents();
    }
    
    private void OnDestroy()
    {
        CleanupInputEvents();
    }
    
    private void SetupInputEvents()
    {
        if (playerInput == null) return;
        
        // Récupérer les actions une seule fois
        var gameActionMap = playerInput.actions.FindActionMap("Game");
        var uiActionMap = playerInput.actions.FindActionMap("UI");
        
        if (gameActionMap != null)
        {
            interactAction = gameActionMap.FindAction(interactionActionName);
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }
            else
            {
                Debug.LogWarning($"NPCVendor: Action '{interactionActionName}' non trouvée dans la map 'Game'");
            }
        }
        else
        {
            Debug.LogWarning("NPCVendor: Action map 'Game' non trouvée");
        }
        
        if (uiActionMap != null)
        {
            closeMenuAction = uiActionMap.FindAction("CloseMenu");
        }
    }
    
    private void CleanupInputEvents()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isPlayerInRange)
        {
            ToggleShop();
        }
    }
    
    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (isShopOpen)
        {
            CloseShop();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isShopOpen)
            {
                CloseShop();
            }
        }
    }
    
    private void ToggleShop()
    {
        if (isShopOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }
    
    public void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError($"NPCVendor: Panel de magasin non assigné sur {gameObject.name}");
            return;
        }
        
        CloseInventoryIfOpen();
        shopPanel.SetActive(true);
        isShopOpen = true;
        
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
            if (closeMenuAction != null)
            {
                closeMenuAction.performed += OnEscapePressed;
            }
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel == null) return;
        
        shopPanel.SetActive(false);
        isShopOpen = false;
        
        if (playerInput != null)
        {
            if (closeMenuAction != null)
            {
                closeMenuAction.performed -= OnEscapePressed;
            }
            playerInput.SwitchCurrentActionMap("Game");
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = isShopOpen ? Color.red : Color.blue;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        
        Gizmos.color = isPlayerInRange ? Color.green : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    // Méthodes publiques pour contrôle externe
    public bool IsPlayerInRange => isPlayerInRange;
    public bool IsShopOpen => isShopOpen;
    
    public void SetShopPanel(GameObject newPanel)
    {
        shopPanel = newPanel;
        if (shopPanel != null && isShopOpen)
        {
            shopPanel.SetActive(true);
        }
    }
    
    public void ForceCloseShop()
    {
        CloseShop();
    }
    
    private void CloseInventoryIfOpen()
    {
        var inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsInventoryOpen)
        {
            inventoryManager.CloseInventory();
        }
    }
}
