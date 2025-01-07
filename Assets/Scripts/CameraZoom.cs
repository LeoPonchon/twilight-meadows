using UnityEngine;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    /* Faut rework le zoom de la caméra avec des boutons sur l'UI
    public CinemachineVirtualCamera virtualCamera;
    public float zoomStep = 2f; // Taille de chaque cran de zoom
    public float minZoom = 5f;  // Zoom minimum
    public float maxZoom = 20f; // Zoom maximum

    void Update()
    {
        if (virtualCamera != null)
        {
            // Récupérer la molette de la souris
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (scrollInput != 0f)
            {
                // Pour une caméra orthographique
                CinemachineComponentBase lensComponent = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (lensComponent is CinemachineFramingTransposer framingTransposer)
                {
                    // Calculer le nouveau zoom
                    float newOrthographicSize = Mathf.Clamp(
                        virtualCamera.m_Lens.OrthographicSize - scrollInput * zoomStep,
                        minZoom,
                        maxZoom
                    );

                    virtualCamera.m_Lens.OrthographicSize = newOrthographicSize;
                }
                // Pour une caméra en perspective (changer le Field of View)
                else
                {
                    float newFieldOfView = Mathf.Clamp(
                        virtualCamera.m_Lens.FieldOfView - scrollInput * zoomStep,
                        minZoom,
                        maxZoom
                    );

                    virtualCamera.m_Lens.FieldOfView = newFieldOfView;
                }
            }
        }
    }
    */
}
