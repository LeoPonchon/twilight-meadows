using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{/*
    [Header("UI Elements")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject player;

    private PlayerInput playerInput;
    private InputAction openMenuAction;

    private void Awake()
    {
        playerInput = player.GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.actions["OpenMenu"].performed += ToggleMenu;
    }

    private void OnDisable()
    {
        if (playerInput != null) playerInput.actions["OpenMenu"].performed -= ToggleMenu;
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        menuUI.SetActive(!menuUI.activeSelf);
        if (menuUI.activeSelf)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void OnResumeButtonPressed()
    {
        menuUI.SetActive(false);
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }*/
}
