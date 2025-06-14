using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TreatmentManager1 : MonoBehaviour
{
    [Header("Panels")]
    public GameObject diagnosisPanel;
    public GameObject instructionPanel;
    public GameObject treatmentPanel;
    public GameObject resultPanel;

    [Header("UI Elements")]
    public TMP_Text patientInfoText;
    public TMP_Text plantNameText;
    public TMP_Text instructionText;
    public TMP_Text resultText;
    public Image successImage;
    public Image failureImage;

    [Header("Buttons")]
    public Button brewButton;
    public Button instructionContinueButton;
    public Button boilButton;
    public Button grindButton;
    public Button continueButton;

    [Header("Celebration Effect")]
    public GameObject[] confettiPrefabs;
    public GameObject confettiCanvas;

    [Header("Audio")]
    public AudioClip failSound;
    private AudioSource audioSource;

    private List<ItemData> plantsToTreat = new List<ItemData>();
    private int currentPlantIndex = 0;
    private string selectedMethod = "";

    void Start()
    {

        // debug:  打印当前背包物品内容

        Debug.Log("👜 Inventory Check: " + (PlayerInventory.Instance != null));

        if (PlayerInventory.Instance != null)
        {
            foreach (var slot in PlayerInventory.Instance.slots)
            {
                Debug.Log($"[Inventory Slot] {slot.item?.itemName ?? "Empty"} ×{slot.quantity}");
            }
        }
        else
        {
            Debug.LogWarning("❌ PlayerInventory.Instance is NULL at TreatmentScene-1 Start.");
        }



        audioSource = gameObject.AddComponent<AudioSource>();

        // 初始化植物数据
        plantsToTreat.Clear();
        AddIfInInventory("Foxglove");
        AddIfInInventory("Ginger");

        if (plantsToTreat.Count < 2)
        {
            ShowGameOver("You don't have both required herbs.");
            return;
        }

        SetupUIForCurrentPlant();
    }

    void AddIfInInventory(string name)
    {
        var inv = PlayerInventory.Instance;
        foreach (var slot in inv.slots)
        {
            if (slot.item != null && slot.item.itemName.ToLower().Contains(name.ToLower()))
            {
                plantsToTreat.Add(slot.item);
                return;
            }
        }

    }

    void SetupUIForCurrentPlant()
    {
        if (currentPlantIndex >= plantsToTreat.Count)
        {
            SceneManager.LoadScene("PostTreatmentScene");
            return;
        }

        var currentPlant = plantsToTreat[currentPlantIndex];

        diagnosisPanel.SetActive(true);
        instructionPanel.SetActive(false);
        treatmentPanel.SetActive(false);
        resultPanel.SetActive(false);

        patientInfoText.text = $"Prepare treatment for patient #{currentPlantIndex + 1}";
        plantNameText.text = $"Using: {currentPlant.itemName}";

        brewButton.onClick.RemoveAllListeners();
        brewButton.onClick.AddListener(() =>
        {
            diagnosisPanel.SetActive(false);
            instructionPanel.SetActive(true);
            instructionText.text = $"Prepare the {currentPlant.itemName} carefully...";
        });

        instructionContinueButton.onClick.RemoveAllListeners();
        instructionContinueButton.onClick.AddListener(() =>
        {
            instructionPanel.SetActive(false);
            treatmentPanel.SetActive(true);
        });

        boilButton.onClick.RemoveAllListeners();
        grindButton.onClick.RemoveAllListeners();
        boilButton.onClick.AddListener(() => OnTreatmentStep("boil"));
        grindButton.onClick.AddListener(() => OnTreatmentStep("grind"));
    }

    void OnTreatmentStep(string method)
    {
        selectedMethod = method;

        treatmentPanel.SetActive(false);
        resultPanel.SetActive(true);

        if (method == GetCorrectMethodForCurrentPlant())
        {
            ShowSuccess();
        }
        else
        {
            ShowGameOver("The preparation failed.");
        }
    }

    string GetCorrectMethodForCurrentPlant()
    {
        var plant = plantsToTreat[currentPlantIndex];
        if (plant.itemName.ToLower().Contains("foxglove")) return "boil";
        if (plant.itemName.ToLower().Contains("ginger")) return "grind";
        return "boil"; // default fallback
    }

    void ShowSuccess()
    {
        resultText.text = "Well done! The medicine is ready.";
        successImage.gameObject.SetActive(true);
        failureImage.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);

        if (confettiPrefabs != null && confettiCanvas != null)
        {
            foreach (var prefab in confettiPrefabs)
            {
                Instantiate(prefab, confettiCanvas.transform);
            }
        }

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            currentPlantIndex++;
            SetupUIForCurrentPlant();
        });
    }

    void ShowGameOver(string message)
    {
        diagnosisPanel.SetActive(false);
        instructionPanel.SetActive(false);
        treatmentPanel.SetActive(false);
        resultPanel.SetActive(true);

        resultText.text = $"Game Over: {message}";
        successImage.gameObject.SetActive(false);
        failureImage.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);

        if (failSound != null)
            audioSource.PlayOneShot(failSound);

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("ClinicScene-1");
        });
    }
}
