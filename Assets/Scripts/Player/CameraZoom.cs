using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float zoomStep = 1f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private SceneContext sceneContext;
    private float currentZoomLevel;

    void Start()
    {
        if (sceneContext == null)
        {
            sceneContext = FindObjectOfType<SceneContext>();
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
        if (playerInput == null)
        {
            if (sceneContext == null)
            {
                Debug.LogError("CameraZoom: Missing SceneContext in scene.", this);
                enabled = false;
                return;
            }
            playerInput = sceneContext.PlayerInput;
        }
        
        // Initialiser le niveau de zoom actuel
        if (virtualCamera != null)
        {
            currentZoomLevel = virtualCamera.m_Lens.OrthographicSize;
        }
    }

    void Update()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("CameraZoom: virtualCamera is null!");
            return;
        }
        
        if (playerInput == null)
        {
            Debug.LogError("CameraZoom: playerInput is null!");
            return;
        }

        // Détecter les pressions de boutons (pas les maintiens)
        bool zoomPressedThisFrame = playerInput.actions["ZoomCamera"].triggered;
        bool unzoomPressedThisFrame = playerInput.actions["UnzoomCamera"].triggered;

        if (zoomPressedThisFrame)
        {
            ZoomIn();
        }
        else if (unzoomPressedThisFrame)
        {
            ZoomOut();
        }
    }
    
    private void ZoomIn()
    {
        float newZoomLevel = Mathf.Clamp(currentZoomLevel - zoomStep, minZoom, maxZoom);
        if (newZoomLevel != currentZoomLevel)
        {
            currentZoomLevel = newZoomLevel;
            virtualCamera.m_Lens.OrthographicSize = currentZoomLevel;
            Debug.Log($"Zoom IN to: {currentZoomLevel}");
        }
    }
    
    private void ZoomOut()
    {
        float newZoomLevel = Mathf.Clamp(currentZoomLevel + zoomStep, minZoom, maxZoom);
        if (newZoomLevel != currentZoomLevel)
        {
            currentZoomLevel = newZoomLevel;
            virtualCamera.m_Lens.OrthographicSize = currentZoomLevel;
            Debug.Log($"Zoom OUT to: {currentZoomLevel}");
        }
    }
}
