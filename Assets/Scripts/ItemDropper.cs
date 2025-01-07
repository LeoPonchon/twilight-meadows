using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropMapping
    {
        public ItemData itemData; // Donn�es associ�es � l'objet
        public int quantity = 1;  // Quantit� � cr�er
    }

    [System.Serializable]
    public class TileDropMapping
    {
        public TileBase tileType;
        public List<DropMapping> dropList;
    }

    [Header("Drop Mappings")]
    [SerializeField] private List<TileDropMapping> tileDropMappings;

    [SerializeField] private Inventory playerInventory;

    private Dictionary<TileBase, List<DropMapping>> dropDictionary;

    private void Awake()
    {
        InitializeDropDictionary();
    }

    private void InitializeDropDictionary()
    {
        dropDictionary = new Dictionary<TileBase, List<DropMapping>>();

        foreach (var tileMapping in tileDropMappings)
        {
            if (tileMapping.tileType != null && tileMapping.dropList != null && tileMapping.dropList.Count > 0)
            {
                dropDictionary[tileMapping.tileType] = tileMapping.dropList;
            }
        }
    }

    public void DropItems(TileBase tileType, Vector3 worldPosition)
    {
        if (!dropDictionary.TryGetValue(tileType, out var dropList))
        {
            Debug.LogWarning($"Aucun drop configur� pour la tuile : {tileType?.name ?? "Inconnue"}");
            return;
        }

        foreach (var dropMapping in dropList)
        {
            SpawnDrops(dropMapping, worldPosition);
        }

        Debug.Log($"Drops cr��s pour la tuile : {tileType.name}");
    }

    private void SpawnDrops(DropMapping dropMapping, Vector3 worldPosition)
    {
        for (int i = 0; i < dropMapping.quantity; i++)
        {
            var drop = new GameObject("Drop"); // Cr�er un nouvel objet pour le drop
            drop.transform.position = worldPosition;

            // Ajouter et configurer les composants n�cessaires
            SetupSpriteRenderer(drop, dropMapping.itemData.icon);
            SetupRigidbody(drop);
            SetupCollider(drop);

            // Appliquer le comportement de saut
            StartCoroutine(SmoothParabolicJump(drop, worldPosition));

            // Ajouter le script Collectible
            var collectible = drop.AddComponent<Collectible>();
            collectible.Setup(playerInventory, dropMapping.itemData);
        }
    }

    private void SetupSpriteRenderer(GameObject drop, Sprite sprite)
    {
        var renderer = drop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    private void SetupRigidbody(GameObject drop)
    {
        var rb = drop.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; // D�sactiver la gravit� pour un comportement contr�l�
    }

    private void SetupCollider(GameObject drop)
    {
        var col = drop.AddComponent<BoxCollider2D>();
        col.isTrigger = true; // Utiliser un trigger pour la d�tection de ramassage
    }

    private IEnumerator SmoothParabolicJump(GameObject drop, Vector3 initialPosition)
    {
        if (drop != null) {
            float duration = 1f;
            float maxHeight = 1f;

            Vector3 finalPosition = CalculateRandomFinalPosition(initialPosition);

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                Vector3 horizontalPosition = Vector3.Lerp(initialPosition, finalPosition, t);
                float verticalOffset = 4 * maxHeight * t * (1 - t);

                drop.transform.position = horizontalPosition + Vector3.up * verticalOffset;

                yield return null;
            }

            drop.transform.position = finalPosition;
        }
    }

    private Vector3 CalculateRandomFinalPosition(Vector3 initialPosition)
    {
        return initialPosition + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
    }
}
