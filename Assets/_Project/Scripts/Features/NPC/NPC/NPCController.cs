using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCController : MonoBehaviour, IDialogueUi
{
    [Header("Dialogue")]
    [SerializeField] private NPCData npcData;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI npcDialogueText;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private SceneContext sceneContext;
    [SerializeField] private string interactActionName = "Interact";

    private InputAction gameInteract;
    private InputAction uiInteract;
    private bool playerInRange;
    private int lineIndex;

    public bool IsDialogueOpen => dialogueUI != null && dialogueUI.activeInHierarchy;

    private void Awake()
    {
        ResolveReferences();
        if (dialogueUI != null) dialogueUI.SetActive(false);
    }

    private void OnEnable()
    {
        gameInteract = FindAction("Game", interactActionName);
        uiInteract = FindAction("UI", interactActionName);

        if (gameInteract != null) gameInteract.performed += OnInteract;
        if (uiInteract != null) uiInteract.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (gameInteract != null) gameInteract.performed -= OnInteract;
        if (uiInteract != null) uiInteract.performed -= OnInteract;
    }

    public void CloseDialogue()
    {
        if (dialogueUI != null) dialogueUI.SetActive(false);
        lineIndex = 0;
        playerInput?.SwitchCurrentActionMap("Game");
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        if (!playerInRange) return;
        if (IsDialogueOpen) NextLine();
        else OpenDialogue();
    }

    private void OpenDialogue()
    {
        if (npcData == null || dialogueUI == null) return;

        lineIndex = 0;
        dialogueUI.SetActive(true);
        playerInput?.SwitchCurrentActionMap("UI");

        if (npcNameText != null) npcNameText.text = npcData.npcName;
        ShowCurrentLine();
    }

    private void NextLine()
    {
        lineIndex++;
        if (npcData == null || npcData.defaultDialogue == null || lineIndex >= npcData.defaultDialogue.Length)
        {
            CloseDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (npcDialogueText == null || npcData == null || npcData.defaultDialogue == null || npcData.defaultDialogue.Length == 0) return;
        npcDialogueText.text = npcData.defaultDialogue[Mathf.Clamp(lineIndex, 0, npcData.defaultDialogue.Length - 1)];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        if (IsDialogueOpen) CloseDialogue();
    }

    private void ResolveReferences()
    {
        if (sceneContext == null) sceneContext = FindObjectOfType<SceneContext>();
        if (playerInput == null) playerInput = sceneContext != null ? sceneContext.PlayerInput : null;
    }

    private InputAction FindAction(string mapName, string actionName)
    {
        if (playerInput == null || playerInput.actions == null) return null;
        return playerInput.actions.FindActionMap(mapName, false)?.FindAction(actionName, false);
    }
}
