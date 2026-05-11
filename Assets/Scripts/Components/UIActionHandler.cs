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

	private void Awake()
	{
		if (playerInput == null)
		{
			playerInput = FindObjectOfType<PlayerInput>();
		}
		if (inventoryManager == null)
		{
			inventoryManager = FindObjectOfType<InventoryManager>();
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

		var vendors = FindObjectsOfType<NPCVendor>();
		for (int i = 0; i < vendors.Length; i++)
		{
			if (vendors[i] != null && vendors[i].IsShopOpen)
			{
				vendors[i].CloseShop();
			}
		}

		// Fermer les dialogues ouverts
		var npcControllers = FindObjectsOfType<NPCController>();
		for (int i = 0; i < npcControllers.Length; i++)
		{
			if (npcControllers[i] != null && npcControllers[i].dialogueUI != null && npcControllers[i].dialogueUI.activeInHierarchy)
			{
				npcControllers[i].dialogueUI.SetActive(false);
			}
		}

		// Sécurité: s'assurer d'être en map Game à la fin
		if (playerInput != null && playerInput.currentActionMap != null && playerInput.currentActionMap.name != "Game")
		{
			playerInput.SwitchCurrentActionMap("Game");
		}
	}
}


