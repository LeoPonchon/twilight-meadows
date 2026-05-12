using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    private bool rebuildQueued;

    private static readonly string[] SlotNames =
    {
        "Back Hair",
        "Front Hair",
        "Eyes",
        "Eyes Outline",
        "Accessory",
        "Shirt",
        "Pants",
        "Gauntlets",
        "Shoes",
    };

    private void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();

        if (Application.isPlaying)
        {
            CreateCustomizationObjects();
        }
    }

    private void OnEnable()
    {
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();

        if (!Application.isPlaying)
        {
            // En mode éditeur, Unity peut appeler OnEnable/OnValidate durant des phases où la modification
            // de hiérarchie est interdite. On ne crée/détruit jamais ici.
            if (!HasAllRenderers()) RequestRebuildInEditor();
            ApplyCustomizationIfReady();
        }
    }

    private void Start()
    {
        UpdateCustomization();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();

        // OnValidate ne doit pas modifier la hiérarchie.
        if (!HasAllRenderers())
        {
            RequestRebuildInEditor();
            return;
        }

        ApplyCustomizationIfReady();
    }

    private void Update()
    {
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();

        if (!Application.isPlaying)
        {
            // Preview éditeur sans reconstruction automatique.
            ApplyCustomizationIfReady();
            return;
        }

        if (playerSprite?.sprite == null) return;
        if (!EnsureRenderersReady()) return;

        ApplyCustomizationIfReady();
    }

    [ContextMenu("Rebuild Customization Objects")]
    private void RebuildCustomizationObjects()
    {
        if (Application.isPlaying)
        {
            CreateCustomizationObjects();
            return;
        }

        RequestRebuildInEditor(force: true);
    }

    private void CreateCustomizationObjects()
    {
        // Nettoyer les anciens objets
        foreach (SpriteRenderer renderer in customizationRenderers)
        {
            if (renderer == null) continue;
            if (Application.isPlaying) Destroy(renderer.gameObject);
            else DestroyImmediate(renderer.gameObject);
        }

        // Supprimer tous les objets fils existants pour éviter les duplicatas
        for (int i = 0; i < SlotNames.Length; i++)
        {
            Transform existing = transform.Find(SlotNames[i]);
            if (existing == null) continue;

            if (Application.isPlaying) Destroy(existing.gameObject);
            else DestroyImmediate(existing.gameObject);
        }

        if (customizationRenderers == null || customizationRenderers.Length != 9)
        {
            customizationRenderers = new SpriteRenderer[9];
        }

        // Créer les GameObjects pour chaque slot
        for (int i = 0; i < customizationRenderers.Length; i++)
        {
            GameObject obj = new GameObject(SlotNames[i]);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(0, 0.8f, 0);
            customizationRenderers[i] = obj.AddComponent<SpriteRenderer>();
        }

        ApplyCustomizationIfReady();
    }

    private bool EnsureRenderersReady()
    {
        if (customizationRenderers == null || customizationRenderers.Length != 9)
        {
            customizationRenderers = new SpriteRenderer[9];
        }

        if (!Application.isPlaying)
        {
            return HasAllRenderers();
        }

        for (int i = 0; i < customizationRenderers.Length; i++)
        {
            if (customizationRenderers[i] != null) continue;
            CreateCustomizationObjects();
            break;
        }

        return HasAllRenderers();
    }

    private bool HasAllRenderers()
    {
        if (customizationRenderers == null || customizationRenderers.Length != 9) return false;

        for (int i = 0; i < customizationRenderers.Length; i++)
        {
            if (customizationRenderers[i] == null) return false;
        }

        return true;
    }

    private bool ApplyCustomizationIfReady()
    {
        if (playerSprite?.sprite == null) return false;
        if (!HasAllRenderers()) return false;

        int frameNumber = ExtractFrameNumber(playerSprite.sprite.name);
        bool isLookingUp = frameNumber >= 4 && frameNumber <= 6;

        CustomizationObject[] slots = { backHairStyle, hairStyle, eyes, eyesOutline, accessory, shirt, pants, gauntlets, shoes };
        Color[] colors = { backHairColor, hairColor, eyesColor, eyesOutlineColor, accessoryColor, shirtColor, pantsColor, gauntletsColor, shoesColor };

        for (int i = 0; i < slots.Length; i++)
        {
            SpriteRenderer renderer = customizationRenderers[i];
            if (renderer == null) continue;

            if (slots[i]?.sprites != null && frameNumber < slots[i].sprites.Length)
            {
                renderer.sprite = slots[i].sprites[frameNumber];
            }

            if (slots[i]?.colorable == true)
            {
                renderer.color = colors[i];
            }
            else
            {
                renderer.color = slots[i]?.defaultTint ?? Color.white;
            }

            renderer.sortingLayerID = playerSprite.sortingLayerID;

            int sortingOrder;
            if (i == 0)
            {
                sortingOrder = playerSprite.sortingOrder - 1;
            }
            else if (i == 1)
            {
                sortingOrder = playerSprite.sortingOrder + 1;
            }
            else if (i == 8)
            {
                sortingOrder = playerSprite.sortingOrder + 2;
            }
            else
            {
                sortingOrder = playerSprite.sortingOrder + i;
            }

            if (isLookingUp && i == 0)
            {
                sortingOrder = playerSprite.sortingOrder + 10;
            }
            else if (isLookingUp && i == 1)
            {
                sortingOrder = playerSprite.sortingOrder + 1;
            }

            renderer.sortingOrder = sortingOrder;
        }

        return true;
    }

    private void RequestRebuildInEditor(bool force = false)
    {
#if UNITY_EDITOR
        if (!force && rebuildQueued) return;
        rebuildQueued = true;

        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            rebuildQueued = false;
            if (Application.isPlaying) return;

            CreateCustomizationObjects();
        };
#endif
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

    public void UpdateCustomization()
    {
        if (Application.isPlaying)
        {
            if (!EnsureRenderersReady()) return;
            ApplyCustomizationIfReady();
            return;
        }

        if (!HasAllRenderers())
        {
            RequestRebuildInEditor();
            return;
        }

        ApplyCustomizationIfReady();
    }

    public void SetHairStyle(HairStyle style) { hairStyle = style; UpdateCustomization(); }
    public void SetBackHairStyle(BackHairStyle style) { backHairStyle = style; UpdateCustomization(); }
    public void SetEyes(Eyes newEyes) { eyes = newEyes; UpdateCustomization(); }
    public void SetEyesOutline(EyesOutline outline) { eyesOutline = outline; UpdateCustomization(); }
    public void SetAccessory(Accessory newAccessory) { accessory = newAccessory; UpdateCustomization(); }
    public void SetShirt(Shirt newShirt) { shirt = newShirt; UpdateCustomization(); }
    public void SetPants(Pants newPants) { pants = newPants; UpdateCustomization(); }
    public void SetGauntlets(Gauntlets newGauntlets) { gauntlets = newGauntlets; UpdateCustomization(); }
    public void SetShoes(Shoes newShoes) { shoes = newShoes; UpdateCustomization(); }

    public HairStyle GetHairStyle() => hairStyle;
    public BackHairStyle GetBackHairStyle() => backHairStyle;
    public Eyes GetEyes() => eyes;
    public EyesOutline GetEyesOutline() => eyesOutline;
    public Accessory GetAccessory() => accessory;
    public Shirt GetShirt() => shirt;
    public Pants GetPants() => pants;
    public Gauntlets GetGauntlets() => gauntlets;
    public Shoes GetShoes() => shoes;

    public void SetBackHairColor(Color color) { backHairColor = color; UpdateCustomization(); }
    public void SetHairColor(Color color) { hairColor = color; UpdateCustomization(); }
    public void SetEyesColor(Color color) { eyesColor = color; UpdateCustomization(); }
    public void SetEyesOutlineColor(Color color) { eyesOutlineColor = color; UpdateCustomization(); }
    public void SetAccessoryColor(Color color) { accessoryColor = color; UpdateCustomization(); }
    public void SetShirtColor(Color color) { shirtColor = color; UpdateCustomization(); }
    public void SetPantsColor(Color color) { pantsColor = color; UpdateCustomization(); }
    public void SetGauntletsColor(Color color) { gauntletsColor = color; UpdateCustomization(); }
    public void SetShoesColor(Color color) { shoesColor = color; UpdateCustomization(); }

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
