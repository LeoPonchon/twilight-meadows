using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PlayerSpriteManager : MonoBehaviour
{
    [Header("Customization Slots")]
    [SerializeField] BackHairStyle backHairStyle;      [SerializeField] Color backHairColor = Color.white;
    [SerializeField] HairStyle hairStyle;              [SerializeField] Color hairColor = Color.white;
    [SerializeField] Eyes eyes;                        [SerializeField] Color eyesColor = Color.white;
    [SerializeField] EyesOutline eyesOutline;          [SerializeField] Color eyesOutlineColor = Color.white;
    [SerializeField] Accessory accessory;              [SerializeField] Color accessoryColor = Color.white;
    [SerializeField] Shirt shirt;                      [SerializeField] Color shirtColor = Color.white;
    [SerializeField] Pants pants;                      [SerializeField] Color pantsColor = Color.white;
    [SerializeField] Gauntlets gauntlets;              [SerializeField] Color gauntletsColor = Color.white;
    [SerializeField] Shoes shoes;                      [SerializeField] Color shoesColor = Color.white;

    SpriteRenderer[] renderers = new SpriteRenderer[9];
    SpriteRenderer playerSprite;
    bool rebuildQueued;

    static readonly string[] Names =
    {
        "Back Hair", "Front Hair", "Eyes", "Eyes Outline", "Accessory",
        "Shirt", "Pants", "Gauntlets", "Shoes"
    };

    CustomizationObject[] Slots => new CustomizationObject[]
    {
        backHairStyle, hairStyle, eyes, eyesOutline, accessory, shirt, pants, gauntlets, shoes
    };

    Color[] Colors => new Color[]
    {
        backHairColor, hairColor, eyesColor, eyesOutlineColor, accessoryColor,
        shirtColor, pantsColor, gauntletsColor, shoesColor
    };

    void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        if (Application.isPlaying) CreateObjects();
    }

    void OnEnable()
    {
        Init();
        if (!Application.isPlaying)
        {
            if (!Ready()) RequestRebuild();
            Apply();
        }
    }

    void Start() => UpdateCustomization();

    void OnValidate()
    {
        if (Application.isPlaying) return;
        Init();
        if (!Ready()) RequestRebuild();
        else Apply();
    }

    void Update()
    {
        Init();

        if (!Application.isPlaying)
        {
            Apply();
            return;
        }

        if (playerSprite?.sprite == null || !EnsureReady()) return;
        Apply();
    }

    void Init()
    {
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (renderers == null || renderers.Length != 9) renderers = new SpriteRenderer[9];
    }

    [ContextMenu("Rebuild Customization Objects")]
    void RebuildCustomizationObjects()
    {
        if (Application.isPlaying) CreateObjects();
        else RequestRebuild(true);
    }

    void CreateObjects()
    {
        Init();

        foreach (var r in renderers)
            if (r) DestroyObject(r.gameObject);

        foreach (var n in Names)
        {
            var child = transform.Find(n);
            if (child) DestroyObject(child.gameObject);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            var obj = new GameObject(Names[i]);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(0, 0.8f, 0);
            renderers[i] = obj.AddComponent<SpriteRenderer>();
        }

        Apply();
    }

    void DestroyObject(GameObject obj)
    {
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    bool EnsureReady()
    {
        Init();

        if (!Application.isPlaying) return Ready();

        if (!Ready()) CreateObjects();
        return Ready();
    }

    bool Ready()
    {
        if (renderers == null || renderers.Length != 9) return false;

        foreach (var r in renderers)
            if (r == null) return false;

        return true;
    }

    bool Apply()
    {
        if (playerSprite?.sprite == null || !Ready()) return false;

        int frame = ExtractFrameNumber(playerSprite.sprite.name);
        bool lookingUp = frame >= 4 && frame <= 6;
        var slots = Slots;
        var colors = Colors;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            var slot = slots[i];

            if (slot?.sprites != null && frame < slot.sprites.Length)
                r.sprite = slot.sprites[frame];

            r.color = slot?.colorable == true ? colors[i] : slot?.defaultTint ?? Color.white;
            r.sortingLayerID = playerSprite.sortingLayerID;
            r.sortingOrder = GetSortingOrder(i, lookingUp);
        }

        return true;
    }

    int GetSortingOrder(int i, bool lookingUp)
    {
        if (i == 0) return lookingUp ? playerSprite.sortingOrder + 10 : playerSprite.sortingOrder - 1;
        if (i == 1) return playerSprite.sortingOrder + 100;
        if (i == 8) return playerSprite.sortingOrder + 2;
        return playerSprite.sortingOrder + i;
    }

    void RequestRebuild(bool force = false)
    {
#if UNITY_EDITOR
        if (!force && rebuildQueued) return;
        rebuildQueued = true;

        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            rebuildQueued = false;
            if (!Application.isPlaying) CreateObjects();
        };
#endif
    }

    int ExtractFrameNumber(string spriteName)
    {
        int index = spriteName.LastIndexOf('_');
        return index >= 0 && int.TryParse(spriteName[(index + 1)..], out int frame) ? frame : 0;
    }

    public void UpdateCustomization()
    {
        if (Application.isPlaying)
        {
            if (EnsureReady()) Apply();
            return;
        }

        if (!Ready()) RequestRebuild();
        else Apply();
    }

    public void SetHairStyle(HairStyle v) { hairStyle = v; UpdateCustomization(); }
    public void SetBackHairStyle(BackHairStyle v) { backHairStyle = v; UpdateCustomization(); }
    public void SetEyes(Eyes v) { eyes = v; UpdateCustomization(); }
    public void SetEyesOutline(EyesOutline v) { eyesOutline = v; UpdateCustomization(); }
    public void SetAccessory(Accessory v) { accessory = v; UpdateCustomization(); }
    public void SetShirt(Shirt v) { shirt = v; UpdateCustomization(); }
    public void SetPants(Pants v) { pants = v; UpdateCustomization(); }
    public void SetGauntlets(Gauntlets v) { gauntlets = v; UpdateCustomization(); }
    public void SetShoes(Shoes v) { shoes = v; UpdateCustomization(); }

    public HairStyle GetHairStyle() => hairStyle;
    public BackHairStyle GetBackHairStyle() => backHairStyle;
    public Eyes GetEyes() => eyes;
    public EyesOutline GetEyesOutline() => eyesOutline;
    public Accessory GetAccessory() => accessory;
    public Shirt GetShirt() => shirt;
    public Pants GetPants() => pants;
    public Gauntlets GetGauntlets() => gauntlets;
    public Shoes GetShoes() => shoes;

    public void SetBackHairColor(Color v) { backHairColor = v; UpdateCustomization(); }
    public void SetHairColor(Color v) { hairColor = v; UpdateCustomization(); }
    public void SetEyesColor(Color v) { eyesColor = v; UpdateCustomization(); }
    public void SetEyesOutlineColor(Color v) { eyesOutlineColor = v; UpdateCustomization(); }
    public void SetAccessoryColor(Color v) { accessoryColor = v; UpdateCustomization(); }
    public void SetShirtColor(Color v) { shirtColor = v; UpdateCustomization(); }
    public void SetPantsColor(Color v) { pantsColor = v; UpdateCustomization(); }
    public void SetGauntletsColor(Color v) { gauntletsColor = v; UpdateCustomization(); }
    public void SetShoesColor(Color v) { shoesColor = v; UpdateCustomization(); }

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