using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryHotbarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryContainer playerInventory;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private SceneContext sceneContext;

    [Header("Input")]
    [SerializeField] private string hotbarScrollActionName = "ScrollWheel";

    [Header("Hotbar")]
    [SerializeField] private int currentHotbarSlot;

    [Header("Open UI")]
    [SerializeField] private MonoBehaviour[] shops;

    private readonly List<InputAction> hotbarActions = new List<InputAction>();
    private readonly List<System.Action<InputAction.CallbackContext>> hotbarCallbacks = new List<System.Action<InputAction.CallbackContext>>();
    private InputAction hotbarScrollAction;
    private bool isInventoryOpen;

    public System.Action<int> OnHotbarSlotChanged;
    public System.Action<ItemStack> OnHotbarItemUsed;
    public System.Action<bool> OnInventoryStateChanged;

    public int CurrentHotbarSlot => currentHotbarSlot;
    public InventoryContainer PlayerInventory => playerInventory;
    public bool IsInventoryOpen => isInventoryOpen;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        BindInput();
    }

    private void OnDestroy()
    {
        UnbindInput();
    }

    public void ToggleInventory()
    {
        if (isInventoryOpen) CloseInventory();
        else OpenInventory();
    }

    public void OpenInventory()
    {
        CloseOpenShops();
        SetInventoryOpen(true);
        playerInput?.SwitchCurrentActionMap("UI");
    }

    public void CloseInventory()
    {
        SetInventoryOpen(false);
        playerInput?.SwitchCurrentActionMap("Game");
    }

    public void ForceGameMode()
    {
        SetInventoryOpen(false);
        playerInput?.SwitchCurrentActionMap("Game");
    }

    public ItemStack GetCurrentHotbarItem() => playerInventory != null ? playerInventory.GetItemInSlot(currentHotbarSlot) : null;
    public int GetCurrentHotbarSlot() => currentHotbarSlot;

    public void SetCurrentHotbarSlot(int slotIndex)
    {
        if (playerInventory == null || slotIndex < 0 || slotIndex >= playerInventory.maxHotbarSlots) return;
        currentHotbarSlot = slotIndex;
        OnHotbarSlotChanged?.Invoke(currentHotbarSlot);
    }

    public bool UseCurrentHotbarItem()
    {
        var stack = GetCurrentHotbarItem();
        if (stack?.itemInstance == null) return false;

        bool used = stack.itemInstance switch
        {
            ToolInstance tool => tool.Use(),
            WateringCanInstance wateringCan => wateringCan.Use(),
            _ => false
        };

        if (used) OnHotbarItemUsed?.Invoke(stack);
        return used;
    }

    private void SetInventoryOpen(bool open)
    {
        isInventoryOpen = open;
        OnInventoryStateChanged?.Invoke(open);
    }

    private void BindInput()
    {
        if (playerInput == null || playerInput.actions == null || playerInventory == null) return;

        for (int i = 0; i < playerInventory.maxHotbarSlots; i++)
        {
            int slotIndex = i;
            var action = playerInput.actions.FindAction($"HotbarSlot{slotIndex + 1}", false);
            if (action == null) continue;
            System.Action<InputAction.CallbackContext> callback = _ => SetCurrentHotbarSlot(slotIndex);
            action.performed += callback;
            hotbarActions.Add(action);
            hotbarCallbacks.Add(callback);
        }

        hotbarScrollAction = playerInput.actions.FindAction(hotbarScrollActionName, false);
        if (hotbarScrollAction != null) hotbarScrollAction.performed += OnHotbarScroll;
    }

    private void UnbindInput()
    {
        for (int i = 0; i < hotbarActions.Count; i++)
        {
            var action = hotbarActions[i];
            if (action == null) continue;
            action.performed -= hotbarCallbacks[i];
        }
        hotbarActions.Clear();
        hotbarCallbacks.Clear();

        if (hotbarScrollAction != null) hotbarScrollAction.performed -= OnHotbarScroll;
    }

    private void OnHotbarScroll(InputAction.CallbackContext context)
    {
        if (playerInventory == null) return;
        if (playerInput != null && playerInput.currentActionMap != null && playerInput.currentActionMap.name == "UI") return;

        float value = context.valueType == typeof(Vector2) ? context.ReadValue<Vector2>().y : context.ReadValue<float>();
        if (value == 0f) return;

        int direction = value < 0f ? 1 : -1;
        int slot = (currentHotbarSlot + direction + playerInventory.maxHotbarSlots) % playerInventory.maxHotbarSlots;
        SetCurrentHotbarSlot(slot);
    }


    private void CloseOpenShops()
    {
        if (shops == null) return;
        foreach (var behaviour in shops)
        {
            if (behaviour is IShopUi shop && shop.IsShopOpen) shop.CloseShop();
        }
    }

    private void ResolveReferences()
    {
        if (sceneContext == null) sceneContext = FindObjectOfType<SceneContext>();
        if (playerInventory == null) playerInventory = sceneContext != null ? sceneContext.Get<InventoryContainer>() : null;
        if (playerInput == null) playerInput = sceneContext != null ? sceneContext.PlayerInput : null;
    }
}
