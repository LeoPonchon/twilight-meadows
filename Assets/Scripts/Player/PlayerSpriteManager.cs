using UnityEngine;

[ExecuteAlways]
public class PlayerSpriteManager : MonoBehaviour
{
    [Header("Customization Slots")]
    [SerializeField] private BackHairStyle backHairStyle;
    [SerializeField] private Color backHairColor = Color.white;
    [SerializeField] private HairStyle hairStyle;
    [SerializeField] private Color hairColor = Color.white;
    [SerializeField] private Eyes eyes;
    [SerializeField] private Color eyesColor = Color.white;
    [SerializeField] private EyesOutline eyesOutline;
    [SerializeField] private Color eyesOutlineColor = Color.white;
    [SerializeField] private Accessory accessory;
    [SerializeField] private Color accessoryColor = Color.white;
    [SerializeField] private Shirt shirt;
    [SerializeField] private Color shirtColor = Color.white;
    [SerializeField] private Pants pants;
    [SerializeField] private Color pantsColor = Color.white;
    [SerializeField] private Gauntlets gauntlets;
    [SerializeField] private Color gauntletsColor = Color.white;
    [SerializeField] private Shoes shoes;
    [SerializeField] private Color shoesColor = Color.white;
    
    private SpriteRenderer[] customizationRenderers = new SpriteRenderer[9];
    private SpriteRenderer playerSprite;
    
    private void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        CreateCustomizationObjects();
    }
    
    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
            bool needsRebuild = false;
            for (int i = 0; i < customizationRenderers.Length; i++)
            {
                if (customizationRenderers[i] == null)
                {
                    needsRebuild = true;
                    break;
                }
            }
            if (needsRebuild)
            {
                CreateCustomizationObjects();
            }
            UpdateCustomization();
        }
    }
    
    private void CreateCustomizationObjects()
    {
        // Nettoyer les anciens objets
        foreach (SpriteRenderer renderer in customizationRenderers)
        {
            if (renderer != null) DestroyImmediate(renderer.gameObject);
        }
        
        // Supprimer tous les objets fils existants pour éviter les duplicatas
        string[] slotNames = { "Back Hair", "Front Hair", "Eyes", "Eyes Outline", "Accessory", "Shirt", "Pants", "Gauntlets", "Shoes" };
        for (int i = 0; i < slotNames.Length; i++)
        {
            Transform existing = transform.Find(slotNames[i]);
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
            }
        }
        
        // Créer les GameObjects pour chaque slot (ordre: back hair, front hair, eyes, etc.)
        for (int i = 0; i < customizationRenderers.Length; i++)
        {
            GameObject obj = new GameObject(slotNames[i]);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(0, 0.8f, 0);
            customizationRenderers[i] = obj.AddComponent<SpriteRenderer>();
        }
        
        UpdateCustomization();
    }
    
    private void Start()
    {
        UpdateCustomization();
    }
    
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
            UpdateCustomization();
        }
    }
    
    private void Update()
    {
        if (playerSprite?.sprite == null) return;
        
        int frameNumber = ExtractFrameNumber(playerSprite.sprite.name);
        bool isLookingUp = frameNumber >= 4 && frameNumber <= 6;
        
        // Mettre à jour chaque slot
        CustomizationObject[] slots = { backHairStyle, hairStyle, eyes, eyesOutline, accessory, shirt, pants, gauntlets, shoes };
        Color[] colors = { backHairColor, hairColor, eyesColor, eyesOutlineColor, accessoryColor, shirtColor, pantsColor, gauntletsColor, shoesColor };
        
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i]?.sprites != null && frameNumber < slots[i].sprites.Length)
            {
                customizationRenderers[i].sprite = slots[i].sprites[frameNumber];
            }
            
            // Appliquer la couleur
            if (slots[i]?.colorable == true)
            {
                // Si colorable, utiliser la couleur personnalisée
                customizationRenderers[i].color = colors[i];
            }
            else
            {
                // Si non colorable, utiliser la teinte par défaut
                customizationRenderers[i].color = slots[i]?.defaultTint ?? Color.white;
            }
            
            // Synchroniser les layers
            customizationRenderers[i].sortingLayerID = playerSprite.sortingLayerID;
            
            // Définir l'ordre de rendu selon le type d'élément
            int sortingOrder;
            
            if (i == 0) // Back hair - derrière le joueur
            {
                sortingOrder = playerSprite.sortingOrder - 1;
            }
            else if (i == 1) // Front hair - devant le joueur
            {
                sortingOrder = playerSprite.sortingOrder + 1;
            }
            else if (i == 8) // Shoes - sous les vêtements
            {
                sortingOrder = playerSprite.sortingOrder + 2; // Juste au-dessus du front hair
            }
            else // Autres éléments - devant le front hair
            {
                sortingOrder = playerSprite.sortingOrder + i;
            }
            
            // Si regarde vers le haut, back hair au-dessus de tout
            if (isLookingUp && i == 0) // Back hair
            {
                sortingOrder = playerSprite.sortingOrder + 10; // Au-dessus de tout
            }
            else if (isLookingUp && i == 1) // Front hair reste normal
            {
                sortingOrder = playerSprite.sortingOrder + 1;
            }
            
            customizationRenderers[i].sortingOrder = sortingOrder;
        }
    }
    
    private int ExtractFrameNumber(string spriteName)
    {
        int lastUnderscoreIndex = spriteName.LastIndexOf('_');
        if (lastUnderscoreIndex != -1 && int.TryParse(spriteName.Substring(lastUnderscoreIndex + 1), out int frameNumber))
        {
            return frameNumber;
        }
        return 0;
    }
    
    // Méthodes publiques pour gérer les slots
    public void UpdateCustomization() => Update();
    
    public void SetHairStyle(HairStyle style) { hairStyle = style; CreateCustomizationObjects(); }
    public void SetBackHairStyle(BackHairStyle style) { backHairStyle = style; CreateCustomizationObjects(); }
    public void SetEyes(Eyes newEyes) { eyes = newEyes; CreateCustomizationObjects(); }
    public void SetEyesOutline(EyesOutline outline) { eyesOutline = outline; CreateCustomizationObjects(); }
    public void SetAccessory(Accessory newAccessory) { accessory = newAccessory; CreateCustomizationObjects(); }
    public void SetShirt(Shirt newShirt) { shirt = newShirt; CreateCustomizationObjects(); }
    public void SetPants(Pants newPants) { pants = newPants; CreateCustomizationObjects(); }
    public void SetGauntlets(Gauntlets newGauntlets) { gauntlets = newGauntlets; CreateCustomizationObjects(); }
    public void SetShoes(Shoes newShoes) { shoes = newShoes; CreateCustomizationObjects(); }
    
    // Getters
    public HairStyle GetHairStyle() => hairStyle;
    public BackHairStyle GetBackHairStyle() => backHairStyle;
    public Eyes GetEyes() => eyes;
    public EyesOutline GetEyesOutline() => eyesOutline;
    public Accessory GetAccessory() => accessory;
    public Shirt GetShirt() => shirt;
    public Pants GetPants() => pants;
    public Gauntlets GetGauntlets() => gauntlets;
    public Shoes GetShoes() => shoes;
    
    // Méthodes pour changer les couleurs
    public void SetBackHairColor(Color color) { backHairColor = color; UpdateCustomization(); }
    public void SetHairColor(Color color) { hairColor = color; UpdateCustomization(); }
    public void SetEyesColor(Color color) { eyesColor = color; UpdateCustomization(); }
    public void SetEyesOutlineColor(Color color) { eyesOutlineColor = color; UpdateCustomization(); }
    public void SetAccessoryColor(Color color) { accessoryColor = color; UpdateCustomization(); }
    public void SetShirtColor(Color color) { shirtColor = color; UpdateCustomization(); }
    public void SetPantsColor(Color color) { pantsColor = color; UpdateCustomization(); }
    public void SetGauntletsColor(Color color) { gauntletsColor = color; UpdateCustomization(); }
    public void SetShoesColor(Color color) { shoesColor = color; UpdateCustomization(); }
    
    // Getters pour les couleurs
    public Color GetBackHairColor() => backHairColor;
    public Color GetHairColor() => hairColor;
    public Color GetEyesColor() => eyesColor;
    public Color GetEyesOutlineColor() => eyesOutlineColor;
    public Color GetAccessoryColor() => accessoryColor;
    public Color GetShirtColor() => shirtColor;
    public Color GetPantsColor() => pantsColor;
    public Color GetGauntletsColor() => gauntletsColor;
    public Color GetShoesColor() => shoesColor;
}