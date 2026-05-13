using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    [Header("Scene Routing")]
    public string GameSceneName = "SampleScene";
    public string MultiplayerSceneName = "Multiplayer";

    [Header("Save Slots")]
    public string HasSavePlayerPrefsKey = "HasSave";
    public string LoadSlotButtonLabelPrefix = "Partie";

    [Header("UI Panels (optional)")]
    public GameObject MainPanel;
    public GameObject LoadPanel;
    public GameObject SettingsPanel;
    public GameObject CreditsPanel;

    [Header("Load Panel List (optional)")]
    public Transform LoadSavesContainer;
    public Button LoadSaveSlotButtonPrefab;
    [Tooltip("Nombre d'enfants au début de LoadSavesContainer à ne pas supprimer (ex: Continue + Back).")]
    public int LoadSavesStaticChildrenCount = 2;

    [Header("UI Buttons (optional)")]
    public Button ContinueButton;
    public Button NewGameButton;
    public Button LoadGameButton;
    public Button MultiplayerButton;
    public Button SettingsButton;
    public Button CreditsButton;
    public Button ExitButton;

    public Button LoadContinueButton;
    public Button LoadBackButton;
    public Button SettingsBackButton;
    public Button CreditsBackButton;

    private bool HasSave
    {
        get
        {
            if (PlayerPrefs.GetInt(HasSavePlayerPrefsKey, 0) == 1)
            {
                return true;
            }

            return SaveSlots.AnySaveExists();
        }
    }

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AutoWireIfMissing();
        BindButtons();
        ShowMain();
        RefreshContinueState();

        // Continue is handled inside Load panel now.
        if (ContinueButton != null) ContinueButton.gameObject.SetActive(false);
    }

    private void AutoWireIfMissing()
    {
        if (MainPanel == null) MainPanel = FindGo("MainPanel");
        if (LoadPanel == null) LoadPanel = FindGo("LoadPanel");
        if (SettingsPanel == null) SettingsPanel = FindGo("SettingsPanel");
        if (CreditsPanel == null) CreditsPanel = FindGo("CreditsPanel");

        if (LoadSavesContainer == null) LoadSavesContainer = FindTransform("LoadSavesContainer");
        if (LoadSaveSlotButtonPrefab == null) LoadSaveSlotButtonPrefab = FindButton("LoadSaveSlotButtonPrefab");

        if (ContinueButton == null) ContinueButton = FindButton("ContinueButton");
        if (NewGameButton == null) NewGameButton = FindButton("NewGameButton");
        if (LoadGameButton == null) LoadGameButton = FindButton("LoadGameButton");
        if (MultiplayerButton == null) MultiplayerButton = FindButton("MultiplayerButton");
        if (SettingsButton == null) SettingsButton = FindButton("SettingsButton");
        if (CreditsButton == null) CreditsButton = FindButton("CreditsButton");
        if (ExitButton == null) ExitButton = FindButton("ExitButton");

        if (LoadContinueButton == null) LoadContinueButton = FindButton("LoadContinueButton");
        if (LoadBackButton == null) LoadBackButton = FindButton("LoadBackButton");
        if (SettingsBackButton == null) SettingsBackButton = FindButton("SettingsBackButton");
        if (CreditsBackButton == null) CreditsBackButton = FindButton("CreditsBackButton");
    }

    private void BindButtons()
    {
        if (NewGameButton != null) NewGameButton.onClick.AddListener(OnNewGameClicked);
        if (LoadGameButton != null) LoadGameButton.onClick.AddListener(ShowLoad);
        if (MultiplayerButton != null) MultiplayerButton.onClick.AddListener(OnMultiplayerClicked);
        if (SettingsButton != null) SettingsButton.onClick.AddListener(ShowSettings);
        if (CreditsButton != null) CreditsButton.onClick.AddListener(ShowCredits);
        if (ExitButton != null) ExitButton.onClick.AddListener(OnExitClicked);

        if (LoadContinueButton != null) LoadContinueButton.onClick.AddListener(OnContinueClicked);
        if (LoadBackButton != null) LoadBackButton.onClick.AddListener(ShowMain);
        if (SettingsBackButton != null) SettingsBackButton.onClick.AddListener(ShowMain);
        if (CreditsBackButton != null) CreditsBackButton.onClick.AddListener(ShowMain);
    }

    private void RefreshContinueState()
    {
        var hasSave = HasSave;
        if (LoadGameButton != null) LoadGameButton.interactable = hasSave;
        if (LoadContinueButton != null) LoadContinueButton.interactable = hasSave;
    }

    private void SetPanels(bool main, bool load, bool settings, bool credits)
    {
        if (MainPanel != null) MainPanel.SetActive(main);
        if (LoadPanel != null) LoadPanel.SetActive(load);
        if (SettingsPanel != null) SettingsPanel.SetActive(settings);
        if (CreditsPanel != null) CreditsPanel.SetActive(credits);
    }

    public void ShowMain() => SetPanels(main: true, load: false, settings: false, credits: false);
    public void ShowLoad()
    {
        SetPanels(main: false, load: true, settings: false, credits: false);
        RefreshLoadSavesList();
    }
    public void ShowSettings() => SetPanels(main: false, load: false, settings: true, credits: false);
    public void ShowCredits() => SetPanels(main: false, load: false, settings: false, credits: true);

    public void OnContinueClicked()
    {
        var mostRecent = SaveSlots.GetMostRecent();
        if (mostRecent == null)
        {
            Debug.LogWarning("MainMenu: No save found for Continue.");
            return;
        }

        SaveSlots.SetActiveSlotId(mostRecent.SlotId);
        SaveSlots.TouchLastPlayed(mostRecent.SlotId);
        LoadSceneIfExists(GameSceneName);
    }

    public void OnNewGameClicked()
    {
        PlayerPrefs.DeleteKey(HasSavePlayerPrefsKey);
        var created = SaveSlots.CreateNewSave();
        SaveSlots.SetActiveSlotId(created.SlotId);
        SaveSlots.TouchLastPlayed(created.SlotId);

        LoadSceneIfExists(GameSceneName);
    }

    public void OnMultiplayerClicked() => LoadSceneIfExists(MultiplayerSceneName);

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static GameObject FindGo(string name) => GameObject.Find(name);

    private static Transform FindTransform(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.transform : null;
    }

    private static Button FindButton(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Button>() : null;
    }

    private void RefreshLoadSavesList()
    {
        if (LoadSavesContainer == null || LoadSaveSlotButtonPrefab == null)
        {
            return;
        }

        var keep = Mathf.Max(0, LoadSavesStaticChildrenCount);
        for (var i = LoadSavesContainer.childCount - 1; i >= keep; i--)
        {
            var child = LoadSavesContainer.GetChild(i);
            if (child != null) Destroy(child.gameObject);
        }

        var saves = SaveSlots.ListSaves();
        if (saves.Count == 0) return;

        foreach (var save in saves)
        {
            var button = Instantiate(LoadSaveSlotButtonPrefab, LoadSavesContainer);
            button.gameObject.SetActive(true);

            var label = save.GetDisplayName(LoadSlotButtonLabelPrefix);
            TrySetButtonLabel(button, label);

            var slotId = save.SlotId;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SaveSlots.SetActiveSlotId(slotId);
                SaveSlots.TouchLastPlayed(slotId);
                LoadSceneIfExists(GameSceneName);
            });
        }
    }

    private static void TrySetButtonLabel(Button button, string label)
    {
        if (button == null) return;

        // Unity UI Text
        var text = button.GetComponentInChildren<Text>(includeInactive: true);
        if (text != null)
        {
            text.text = label;
            return;
        }

        // TMPro
        var tmp = button.GetComponentInChildren<TMPro.TMP_Text>(includeInactive: true);
        if (tmp != null)
        {
            tmp.text = label;
        }
    }

    private static void LoadSceneIfExists(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("MainMenu: Scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"MainMenu: Scene not in Build Settings or not found: '{sceneName}'.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
