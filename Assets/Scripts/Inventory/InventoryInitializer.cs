using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// S'assure que l'inventaire et son UI sont correctement initialisés dans le bon ordre
/// </summary>
public class InventoryInitializer : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private InventorySlotManager slotManager;
    [SerializeField] private bool debugMode = true;

    private void Start()
    {
        if (debugMode) Debug.Log("Démarrage de l'initialisation de l'inventaire");

        // Vérifier les références
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory n'est pas assigné dans InventoryInitializer", this);
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogError("inventoryUI n'est pas assigné dans InventoryInitializer", this);
            return;
        }

        if (slotManager == null)
        {
            Debug.LogError("slotManager n'est pas assigné dans InventoryInitializer", this);
            return;
        }

        // Vérifier et configurer les layouts
        ConfigureLayoutGroups();

        // Forcer l'initialisation de l'inventaire
        if (debugMode) Debug.Log("Initialisation forcée de l'inventaire");

        // Assigner l'inventaire aux composants
        // (ceci est déjà fait via l'inspecteur, mais on s'assure que c'est correct)
        SetInventoryReferenceOnComponents();

        // Créer les slots
        if (debugMode) Debug.Log("Création des slots d'inventaire");
        slotManager.CreateInventorySlots();

        // Mettre à jour l'UI
        if (debugMode) Debug.Log("Mise à jour forcée de l'UI d'inventaire");
        slotManager.UpdateInventoryUI();

        if (debugMode) Debug.Log("Initialisation de l'inventaire terminée");
    }

    // Méthode pour forcer l'ajout d'items de test
    public void AddTestItems()
    {
        if (playerInventory == null) return;

        Debug.Log("Ajout d'items de test à l'inventaire");

        // Trouver quelques ItemData dans les Resources
        ItemData[] items = Resources.LoadAll<ItemData>("Items");

        if (items.Length == 0)
        {
            ItemData[] toolsFromItems = LoadAllTools();
            if (toolsFromItems.Length > 0)
            {
                ConfigureDefaultTools(toolsFromItems);
                return;
            }

            Debug.LogWarning("Aucun ItemData trouvé dans Resources/Items ou Assets/Items");
            return;
        }

        // Ajouter quelques items
        for (int i = 0; i < Mathf.Min(5, items.Length); i++)
        {
            playerInventory.AddItem(items[i], Random.Range(1, 10));
            Debug.Log($"Item ajouté: {items[i].itemName}");
        }

        // Mettre à jour l'UI
        slotManager.UpdateInventoryUI();
    }

    // Configurer les outils par défaut dans l'inventaire
    private void ConfigureDefaultTools(ItemData[] tools)
    {
        if (playerInventory == null || tools == null || tools.Length == 0) return;

        Debug.Log($"Configuration des {tools.Length} outils par défaut");

        // Vider l'inventaire actuel
        playerInventory.GetAllItemsWithIDs().Clear();

        // Ajouter chaque outil à la liste des items par défaut
        playerInventory.defaultItems.Clear();

        foreach (var tool in tools)
        {
            var defaultItem = new Inventory.DefaultItem
            {
                itemData = tool,
                quantity = 1,
                isHotbarTool = IsTool(tool) // Utiliser la méthode IsTool pour déterminer si c'est un outil de hotbar
            };

            playerInventory.defaultItems.Add(defaultItem);
            Debug.Log($"Outil configuré: {tool.itemName} (est un outil: {IsTool(tool)})");
        }

        // Redémarrer l'inventaire pour appliquer les changements
        playerInventory.Start();

        // Mettre à jour l'UI
        slotManager.UpdateInventoryUI();
    }

    // Charger tous les outils disponibles dans les dossiers du projet
    private ItemData[] LoadAllTools()
    {
        List<ItemData> tools = new List<ItemData>();

#if UNITY_EDITOR
        // Version éditeur
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            
            if (item != null)
            {
                // Si c'est un outil, l'ajouter à la liste
                if (IsTool(item))
                {
                    tools.Add(item);
                    Debug.Log($"Outil trouvé: {item.itemName} à {path}");
                }
            }
        }
#else
        // Version runtime
        // Essayer de charger depuis Resources/Tools et Resources/Items
        ItemData[] resourcesTools = Resources.LoadAll<ItemData>("Tools");
        ItemData[] resourcesItems = Resources.LoadAll<ItemData>("Items");

        // Ajouter les outils explicites
        tools.AddRange(resourcesTools);

        // Ajouter les items qui sont des outils
        foreach (var item in resourcesItems)
        {
            if (IsTool(item))
            {
                tools.Add(item);
                Debug.Log($"Outil trouvé: {item.itemName}");
            }
        }

        // Si aucun outil n'est trouvé, essayer un chemin plus générique
        if (tools.Count == 0)
        {
            ItemData[] allItems = Resources.LoadAll<ItemData>("");
            foreach (var item in allItems)
            {
                if (IsTool(item))
                {
                    tools.Add(item);
                    Debug.Log($"Outil trouvé: {item.itemName}");
                }
            }
        }
#endif

        return tools.ToArray();
    }

    // Plus de fallback par nom: toolKind fait foi

    private void SetInventoryReferenceOnComponents()
    {
        // Cette méthode est commentée car normalement les références sont déjà
        // définies dans l'inspecteur. Décommentez si nécessaire pour forcer
        // les références par code.

        // Méthode Reflection pour setter les références private/serialized
        /*
        var inventoryField = typeof(InventoryUI).GetField("playerInventory", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (inventoryField != null)
            inventoryField.SetValue(inventoryUI, playerInventory);
            
        var slotManagerInventoryField = typeof(InventorySlotManager).GetField("playerInventory", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (slotManagerInventoryField != null)
            slotManagerInventoryField.SetValue(slotManager, playerInventory);
        */
    }

    // Méthode pour vérifier et configurer les layouts
    private void ConfigureLayoutGroups()
    {
        try
        {
            // Obtenir les références aux panels depuis le SlotManager
            var slotManagerType = typeof(InventorySlotManager);
            var inventoryPanelField = slotManagerType.GetField("inventoryPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hotbarPanelField = slotManagerType.GetField("hotbarPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (inventoryPanelField != null && hotbarPanelField != null)
            {
                Transform inventoryPanel = inventoryPanelField.GetValue(slotManager) as Transform;
                Transform hotbarPanel = hotbarPanelField.GetValue(slotManager) as Transform;

                if (inventoryPanel != null)
                {
                    ConfigurePanelLayout(inventoryPanel.gameObject);
                }

                if (hotbarPanel != null)
                {
                    ConfigurePanelLayout(hotbarPanel.gameObject);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la configuration des layouts: {e.Message}", this);
        }
    }

    // Configure un panel pour éviter les problèmes de layout
    private void ConfigurePanelLayout(GameObject panel)
    {
        if (panel == null) return;

        // Vérifier si le panel a un LayoutGroup
        LayoutGroup layoutGroup = panel.GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            if (layoutGroup is GridLayoutGroup gridLayout)
            {
                // Configuration optimale pour un GridLayoutGroup
                gridLayout.childAlignment = TextAnchor.UpperLeft;
                // Ces paramètres peuvent être ajustés selon vos besoins visuels
                gridLayout.padding = new RectOffset(10, 10, 10, 10);
                gridLayout.spacing = new Vector2(5, 5);
                gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
                gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
                gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
            }
            else if (layoutGroup is HorizontalLayoutGroup horizontalLayout)
            {
                // Configuration pour HorizontalLayoutGroup
                horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
                horizontalLayout.childForceExpandWidth = false;
                horizontalLayout.childForceExpandHeight = false;
                horizontalLayout.spacing = 5;
                horizontalLayout.padding = new RectOffset(10, 10, 10, 10);
            }
            else if (layoutGroup is VerticalLayoutGroup verticalLayout)
            {
                // Configuration pour VerticalLayoutGroup
                verticalLayout.childAlignment = TextAnchor.UpperCenter;
                verticalLayout.childForceExpandWidth = false;
                verticalLayout.childForceExpandHeight = false;
                verticalLayout.spacing = 5;
                verticalLayout.padding = new RectOffset(10, 10, 10, 10);
            }
        }

        // Ajouter un LayoutElement pour assurer que ce panel ne soit pas redimensionné de manière inappropriée
        LayoutElement layoutElement = panel.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = panel.AddComponent<LayoutElement>();
        }
        layoutElement.ignoreLayout = false;
        layoutElement.preferredWidth = panel.GetComponent<RectTransform>().sizeDelta.x;
        layoutElement.preferredHeight = panel.GetComponent<RectTransform>().sizeDelta.y;
        layoutElement.flexibleWidth = 0;
        layoutElement.flexibleHeight = 0;
    }

    // Méthode pour déterminer si un ItemData est un outil
    private bool IsTool(ItemData item)
    {
        if (item == null) return false;
        
        // Vérifier si l'item hérite d'une classe d'outil
        return item is WateringCanData || 
               item is ShovelData || 
               item is AxeData || 
               item is PickaxeData || 
               item is SpearData || 
               item is HoeData;
    }
}