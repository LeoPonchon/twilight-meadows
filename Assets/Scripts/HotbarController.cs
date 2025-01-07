using UnityEngine;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private GameObject selectorPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private int currentSlot = 0;

    private RectTransform[] slots;
    private GameObject selectorInstance;

    private void Start()
    {
        if (slots == null || slots.Length == 0)
        {
            slots = new RectTransform[slotsParent.childCount];

            for (int i = 0; i < slotsParent.childCount; i++)
            {
                slots[i] = slotsParent.GetChild(i).GetComponent<RectTransform>();
            }

            if (slots.Length == 0)
            {
                Debug.LogError("Aucun slot trouvé dans le GridLayout !");
                return;
            }
        }
        selectorInstance = Instantiate(selectorPrefab, slotsParent.parent);
        selectorInstance.GetComponent<Image>().raycastTarget = false;
        UpdateSelectorPosition();
    }

    private void Update()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0f)
        {
            if (scrollInput < 0)
            {
                currentSlot = (currentSlot + 1) % slots.Length;
            }
            else if (scrollInput > 0)
            {
                currentSlot = (currentSlot - 1 + slots.Length) % slots.Length;
            }
            UpdateSelectorPosition();
        }
    }

    private void UpdateSelectorPosition()
    {
        selectorInstance.transform.position = slots[currentSlot].position;
        Debug.Log($"Slot actif : {currentSlot}");

    }

    public int GetCurrentSlot()
    {
        return currentSlot;
    }
}