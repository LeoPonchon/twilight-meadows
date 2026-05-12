using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Contrôleur simplifié pour les NPCs - déclenche les dialogues depuis NPCData
/// </summary>
public class NPCController : MonoBehaviour, IDialogueUi
{
    [Header("Configuration")]
    public NPCData npcData;
    public string playerTag = "Player";
    
    [Header("UI References")]
    public GameObject dialogueUI;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI npcDialogueText;
    
    private bool isPlayerInRange = false;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private SceneContext sceneContext;
    private InputAction interactAction;
    private InputAction uiInteractAction;
    private int currentDialogueIndex = 0;
    
    private void Awake()
    {
        if (sceneContext == null)
            sceneContext = FindObjectOfType<SceneContext>();

        if (playerInput == null)
        {
            if (sceneContext == null)
            {
                Debug.LogError("NPCController: Missing SceneContext in scene.", this);
                enabled = false;
                return;
            }
            playerInput = sceneContext.PlayerInput;
        }
        
        if (npcData == null)
        {
            Debug.LogWarning($"NPCController sur {gameObject.name}: Aucune NPCData assignée!");
        }
        
    }

    public bool IsDialogueOpen => dialogueUI != null && dialogueUI.activeInHierarchy;

    public void CloseDialogue()
    {
        if (dialogueUI != null) dialogueUI.SetActive(false);
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
        
        // Récupérer l'action Interact depuis l'action map Game pour l'ouverture
        var gameActionMap = playerInput.actions.FindActionMap("Game");
        if (gameActionMap != null)
        {
            interactAction = gameActionMap.FindAction("Interact");
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }
        }
        
        // Récupérer l'action Interact depuis l'action map UI pour la progression
        var uiActionMap = playerInput.actions.FindActionMap("UI");
        if (uiActionMap != null)
        {
            uiInteractAction = uiActionMap.FindAction("Interact");
            if (uiInteractAction != null)
            {
                uiInteractAction.performed += OnUIInteractPerformed;
            }
        }
    }
    
    private void CleanupInputEvents()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
        if (uiInteractAction != null)
        {
            uiInteractAction.performed -= OnUIInteractPerformed;
        }
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isPlayerInRange)
        {
            if (dialogueUI.activeInHierarchy)
            {
                NextDialogue();
            }
            else
            {
                ShowDialogue();
            }
        }
    }
    
    private void OnUIInteractPerformed(InputAction.CallbackContext context)
    {
        // Action Interact en mode UI - seulement pour la progression du dialogue
        if (isPlayerInRange && dialogueUI.activeInHierarchy)
        {
            NextDialogue();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            if (dialogueUI != null && dialogueUI.activeInHierarchy)
            {
                dialogueUI.SetActive(false);
                if (playerInput != null)
                {
                    playerInput.SwitchCurrentActionMap("Game");
                }
                currentDialogueIndex = 0;
            }
        }
    }
    
    private void NextDialogue()
    {
        if (npcData == null || npcData.defaultDialogue == null) return;
        
        currentDialogueIndex++;
        
        if (currentDialogueIndex >= npcData.defaultDialogue.Length)
        {
            // Fin du dialogue, fermer
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
            }
            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("Game");
            }
            currentDialogueIndex = 0;
        }
        else
        {
            // Afficher le dialogue suivant
            if (npcDialogueText != null)
            {
                npcDialogueText.text = npcData.defaultDialogue[currentDialogueIndex];
            }
        }
    }
    
    private void ShowDialogue()
    {
        if (npcData == null) return;
        
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
        }
        
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }
        
        if (npcNameText != null)
        {
            npcNameText.text = npcData.npcName;
        }
        
        if (npcDialogueText != null && npcData.defaultDialogue != null && npcData.defaultDialogue.Length > 0)
        {
            currentDialogueIndex = 0;
            npcDialogueText.text = npcData.defaultDialogue[currentDialogueIndex];
        }
    }
}
