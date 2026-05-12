using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestion simplifiée des actions UI.
/// - En Game: ToggleInventory ouvre l'inventaire.
/// - En UI: ToggleInventory agit comme CloseMenu (ferme UI, repasse en Game).
/// - CloseMenu (en UI): ferme l'UI ouverte et repasse en Game.
/// </summary>
public class UIActionHandler : MonoBehaviour
{
	[SerializeField] private PlayerInput playerInput;
	[SerializeField] private InventoryManager inventoryManager;
	[SerializeField] private SceneContext sceneContext;

	[Header("Optional scene registries")]
	[SerializeField] private ShopRegistry shopRegistry;
	[SerializeField] private DialogueRegistry dialogueRegistry;

	private void Awake()
	{
		if (sceneContext == null)
		{
			sceneContext = FindObjectOfType<SceneContext>();
		}

		if (sceneContext == null)
		{
			Debug.LogError("UIActionHandler: Missing SceneContext in scene.", this);
			enabled = false;
			return;
		}

		if (playerInput == null)
		{
			playerInput = sceneContext.PlayerInput;
		}
		if (inventoryManager == null)
		{
			inventoryManager = sceneContext.GetRequired<InventoryManager>(this, nameof(inventoryManager));
		}
	}

	private void Update()
	{
		if (playerInput == null || playerInput.actions == null) return;

		var currentMap = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : string.Empty;
		var actions = playerInput.actions;

		if (currentMap == "Game")
		{
			if (actions["ToggleInventory"] != null && actions["ToggleInventory"].triggered)
			{
				if (inventoryManager != null)
				{
					inventoryManager.OpenInventory();
				}
			}
		}
		else if (currentMap == "UI")
		{
			bool close = false;
			var closeAction = actions.FindAction("CloseMenu", false);
			var toggleAction = actions.FindAction("ToggleInventory", false);
			if (closeAction != null && closeAction.triggered) close = true;
			if (toggleAction != null && toggleAction.triggered) close = true;

			if (close)
			{
				CloseAllUIAndSwitchToGame();
			}
		}
	}

	private void CloseAllUIAndSwitchToGame()
	{
		if (inventoryManager != null && inventoryManager.IsInventoryOpen)
		{
			inventoryManager.CloseInventory();
		}

		if (shopRegistry != null && shopRegistry.Shops != null)
		{
			var shops = shopRegistry.Shops;
			for (int i = 0; i < shops.Length; i++)
			{
				if (shops[i] is not IShopUi shopUi) continue;
				if (!shopUi.IsShopOpen) continue;
				shopUi.CloseShop();
			}
		}

		// Fermer les dialogues ouverts
		if (dialogueRegistry != null && dialogueRegistry.Dialogues != null)
		{
			var dialogues = dialogueRegistry.Dialogues;
			for (int i = 0; i < dialogues.Length; i++)
			{
				if (dialogues[i] is not IDialogueUi dialogueUi) continue;
				if (!dialogueUi.IsDialogueOpen) continue;
				dialogueUi.CloseDialogue();
			}
		}

		// Sécurité: s'assurer d'être en map Game à la fin
		if (playerInput != null && playerInput.currentActionMap != null && playerInput.currentActionMap.name != "Game")
		{
			playerInput.SwitchCurrentActionMap("Game");
		}
	}
}


