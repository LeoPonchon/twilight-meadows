using UnityEngine;
using UnityEngine.InputSystem;

public class NPCVendor : MonoBehaviour, IShopUi
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string interactionActionName = "Interact";
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InventoryHotbarController inventoryManager;
    [SerializeField] private SceneContext sceneContext;

    private InputAction interactAction;
    private bool playerInRange;

    public bool IsShopOpen => shopPanel != null && shopPanel.activeInHierarchy;

    private void Awake()
    {
        ResolveReferences();
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        interactAction = playerInput != null ? playerInput.actions.FindActionMap("Game", false)?.FindAction(interactionActionName, false) : null;
        if (interactAction != null) interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.performed -= OnInteract;
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        playerInput?.SwitchCurrentActionMap("Game");
    }

    public void OpenShop()
    {
        if (shopPanel == null) return;
        inventoryManager?.CloseInventory();
        shopPanel.SetActive(true);
        playerInput?.SwitchCurrentActionMap("UI");
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        if (!playerInRange) return;
        if (IsShopOpen) CloseShop();
        else OpenShop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        if (IsShopOpen) CloseShop();
    }

    private void ResolveReferences()
    {
        if (sceneContext == null) sceneContext = FindObjectOfType<SceneContext>();
        if (playerInput == null) playerInput = sceneContext != null ? sceneContext.PlayerInput : null;
        if (inventoryManager == null) inventoryManager = sceneContext != null ? sceneContext.Get<InventoryHotbarController>() : null;
    }
}
