using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public GameObject player;
    public GameObject healthIconPrefab;  // Le prefab de l'icône de santé
    private List<GameObject> healthIcons = new List<GameObject>();
    private int previousHealth;

    void Start()
    {
        // Récupère la santé initiale du joueur
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
        // Initialise les icônes de santé en tant qu'enfants de cet objet
        for (int i = 0; i < previousHealth; i++)
        {
            GameObject icon = Instantiate(healthIconPrefab, transform);
            icon.transform.localPosition = new Vector3((i * 30) -500, 210, 0);  // Positionne les icônes horizontalement avec un écart
            icon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            healthIcons.Add(icon);
        }
    }

    void UpdateHealthIcons(int currentHealth)
    {
        // Active ou désactive les icônes selon la santé du joueur
        for (int i = 0; i < healthIcons.Count; i++)
        {
            healthIcons[i].SetActive(i < currentHealth);
        }
    }
}
