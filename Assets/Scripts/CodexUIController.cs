using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodexUIController : MonoBehaviour
{
    [Header("Herbs to Show in Codex")]
    public ItemData[] knownHerbs;

    [Header("UI Panels")]
    public GameObject codexPanel;
    public GameObject detailPanel;

    [Header("Codex Entry UI")]
    public Image herbIcon;
    public Toggle herbTitleToggle; // Assume you're using Toggle for title
    public Text herbTitleLabel; // UnityEngine.UI.Text

    public Button detailButton;

    [Header("Detail View UI (inside Scroll View)")]
    public Image herbImage;
    public TMP_Text detailTitle;
    public TMP_Text herbDescription;
    public TMP_Text herbLocation;

    public Button returnButton;
    private ItemData currentItem;

    void Start()
    {
        codexPanel.SetActive(true);
        detailPanel.SetActive(false);

        if (knownHerbs.Length > 0)
        {
            // For simplicity, display only the first herb's button
            currentItem = knownHerbs[0];
            herbIcon.sprite = currentItem.itemIcon;
            herbTitleLabel.text = currentItem.itemName;

            detailButton.onClick.RemoveAllListeners();
            detailButton.onClick.AddListener(ShowDetails);
        }
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToCodex);
    }

    public void ShowDetails()
    {
        if (currentItem == null) return;

        codexPanel.SetActive(false);
        detailPanel.SetActive(true);

        herbImage.sprite = currentItem.herbLargeImage;
        detailTitle.text = currentItem.itemName;
        herbDescription.text = currentItem.description;
        herbLocation.text = currentItem.growthLocation;
    }

    public void ReturnToCodex()
    {
        detailPanel.SetActive(false);
        codexPanel.SetActive(true);
    }
}
