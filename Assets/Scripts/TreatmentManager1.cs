using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TreatmentManager1 : MonoBehaviour
{
    [Header("Panels")]
    public GameObject diagnosisPanel;
    public GameObject instructionPanel;
    public GameObject treatmentPanel;
    public GameObject resultPanel;

    [Header("UI Elements")]
    public TMP_Text patientInfoText;
    public TMP_Text preparedPlantsText;
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

    private ItemData[] requiredPlants;
    private int requiredCount;
    private int collectedCount;
    private string correctMethod = "boil";
    private string selectedMethod = "";

    void Start()
    {
        // Initialize plant data
        requiredPlants = GameStateManager.Instance.requiredPlants;
        requiredCount = requiredPlants.Length;
        collectedCount = 0;
        foreach (var item in requiredPlants)
            if (PlayerInventory.Instance.GetPlantCount(item) > 0)
                collectedCount++;

        // Auto game over if not all collected
        if (collectedCount < requiredCount)
        {
            ShowGameOver();
            return;
        }

        // Show initial diagnosis panel
        diagnosisPanel.SetActive(true);
        instructionPanel.SetActive(false);
        treatmentPanel.SetActive(false);
        resultPanel.SetActive(false);

        // Populate UI
        patientInfoText.text = "Two patients await treatment!";
        preparedPlantsText.text = $"Herbs needed: {string.Join(", ", System.Array.ConvertAll(requiredPlants, p => p.itemName))}";

        // Bind buttons
        brewButton.onClick.RemoveAllListeners();
        brewButton.onClick.AddListener(ShowInstructionPanel);

        instructionContinueButton.onClick.RemoveAllListeners();
        instructionContinueButton.onClick.AddListener(StartTreatmentPhase);

        boilButton.onClick.RemoveAllListeners();
        grindButton.onClick.RemoveAllListeners();
        boilButton.onClick.AddListener(() => OnTreatmentStep("boil"));
        grindButton.onClick.AddListener(() => OnTreatmentStep("grind"));

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinue);
    }

    void ShowInstructionPanel()
    {
        diagnosisPanel.SetActive(false);
        instructionPanel.SetActive(true);
        patientInfoText.text = "Prepare the medicine with the herbs you collected.";
    }

    void StartTreatmentPhase()
    {
        instructionPanel.SetActive(false);
        treatmentPanel.SetActive(true);
    }

    void OnTreatmentStep(string method)
    {
        selectedMethod = method;

        treatmentPanel.SetActive(false);
        resultPanel.SetActive(true);

        bool success = method == correctMethod;
        if (success)
        {
            ShowSuccess();
        }
        else
        {
            ShowGameOver();
        }
    }

    void ShowSuccess()
    {
        successImage.gameObject.SetActive(true);
        failureImage.gameObject.SetActive(false);
        resultText.text = "Excellent! All required herbs collected and medicine prepared.";
        PlayConfetti();
        continueButton.gameObject.SetActive(true);

        if (failSound != null)
            AudioSource.PlayClipAtPoint(failSound, Vector3.zero);
    }

    void ShowGameOver()
    {
        diagnosisPanel.SetActive(false);
        instructionPanel.SetActive(false);
        treatmentPanel.SetActive(false);
        resultPanel.SetActive(true);
        successImage.gameObject.SetActive(false);
        failureImage.gameObject.SetActive(true);
        resultText.text = "Game Over: insufficient herbs.";
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            // Return to clinic before field scene
            SceneManager.LoadScene("ClinicScene-1");
        });
    }

    void OnContinue()
    {
        // Proceed to post-treatment
        SceneManager.LoadScene("PostTreatmentScene");
    }

    void PlayConfetti()
    {
        foreach (var prefab in confettiPrefabs)
        {
            Instantiate(prefab, confettiCanvas.transform);
        }
    }
}
