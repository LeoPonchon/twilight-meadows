using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HUDManager : MonoBehaviour
{
    public GameObject player;
    public GameObject healthIconPrefab;
    private List<GameObject> healthIcons = new List<GameObject>();
    private int previousHealth;

    void Start()
    {
        previousHealth = Mathf.FloorToInt(player.GetComponent<StatsManager>().health);
        InitializeHealthIcons();
    }

    void Update()
    {
        int currentHealth = player.GetComponent<StatsManager>().health;
        
        if (currentHealth != previousHealth)
        {
            UpdateHealthIcons(currentHealth);
            previousHealth = currentHealth;
        }
    }

    void InitializeHealthIcons()
    {
        for (int i = 0; i < previousHealth; i++)
        {
            GameObject icon = Instantiate(healthIconPrefab, transform);
            icon.transform.localPosition = new Vector3((i * 30) -500, 210, 0);
            icon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            healthIcons.Add(icon);
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(icon, "Create Health Icon");
            }
#endif
        }
    }

    void UpdateHealthIcons(int currentHealth)
    {
        for (int i = 0; i < healthIcons.Count; i++)
        {
            healthIcons[i].SetActive(i < currentHealth);
        }
    }
}
