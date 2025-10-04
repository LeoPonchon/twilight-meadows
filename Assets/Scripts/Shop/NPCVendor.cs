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
    
    private void Awake()
    {
        // Trouver le PlayerInput
        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("NPCVendor: PlayerInput non trouvé dans la scène!");
        }
        
        // S'assurer que le panel est fermé au démarrage
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
        CleanupUIInputEvents();
    }
    
    private void SetupInputEvents()
    {
        if (playerInput != null)
        {
            // S'abonner à l'action d'interaction depuis la map "Game"
            var gameActionMap = playerInput.actions.FindActionMap("Game");
            if (gameActionMap != null)
            {
                var interactAction = gameActionMap.FindAction(interactionActionName);
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
        }
    }
    
    private void SetupUIInputEvents()
    {
        if (playerInput != null)
        {
            // S'abonner à l'action CloseMenu depuis la map "UI"
            var uiActionMap = playerInput.actions.FindActionMap("UI");
            if (uiActionMap != null)
            {
                var closeMenuAction = uiActionMap.FindAction("CloseMenu");
                if (closeMenuAction != null)
                {
                    closeMenuAction.performed += OnEscapePressed;
                }
            }
        }
    }
    
    private void CleanupUIInputEvents()
    {
        if (playerInput != null)
        {
            var uiActionMap = playerInput.actions.FindActionMap("UI");
            if (uiActionMap != null)
            {
                var closeMenuAction = uiActionMap.FindAction("CloseMenu");
                if (closeMenuAction != null)
                {
                    closeMenuAction.performed -= OnEscapePressed;
                }
            }
        }
    }
    
    private void CleanupInputEvents()
    {
        if (playerInput != null)
        {
            var gameActionMap = playerInput.actions.FindActionMap("Game");
            if (gameActionMap != null)
            {
                var interactAction = gameActionMap.FindAction(interactionActionName);
                if (interactAction != null)
                {
                    interactAction.performed -= OnInteractPerformed;
                }
            }
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
            
            // Fermer le magasin si le joueur s'éloigne
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
        
        // Fermer l'inventaire s'il est ouvert
        CloseInventoryIfOpen();
        
        shopPanel.SetActive(true);
        isShopOpen = true;
        
        // Changer l'action map pour permettre la fermeture du magasin avec Échap
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
            SetupUIInputEvents(); // S'abonner aux événements UI
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel == null) return;
        
        shopPanel.SetActive(false);
        isShopOpen = false;
        
        // Revenir à l'action map de jeu et nettoyer les événements UI
        if (playerInput != null)
        {
            CleanupUIInputEvents(); // Se désabonner des événements UI
            playerInput.SwitchCurrentActionMap("Game");
        }
    }
    
    private void OnDrawGizmos()
    {
        // Dessiner l'état du magasin
        Gizmos.color = isShopOpen ? Color.red : Color.blue;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        
        // Dessiner l'état de détection du joueur
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
        // Trouver et fermer l'inventaire s'il est ouvert
        var inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsInventoryOpen)
        {
            inventoryManager.CloseInventory();
        }
    }
}
