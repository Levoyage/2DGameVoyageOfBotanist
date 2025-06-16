using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TreatmentManager1 : MonoBehaviour
{
    [Header("Panels")]
    public GameObject diagnosisPanel;
    public GameObject treatmentPanel;
    public GameObject medicineResultPanel;

    [Header("UI Elements")]
    public TMP_Text patientNameText;
    public TMP_Text plantNameText;
    public Button boilButton;
    public Button grindButton;
    public Button brewButton;
    public Button retryButton;
    public Button continueButton;
    public TMP_Text resultText;
    public Image successImage;
    public Image failureImage;

    [Header("Mentor Dialogue")]
    public GameObject mentorDialogueBubble;
    public TMP_Text mentorDialogueText;

    [Header("QTE System")]
    public QTEProgressBar qteProgressBar;
    public QTERhythmManager6 qteManager1;
    public QTERhythmManager6 qteManager2;
    private QTERhythmManager6 currentQTEManager;

    [Header("Celebration Effect")]
    public List<GameObject> confettiPrefabs;
    public GameObject confettiCanvas;

    [Header("Audio")]
    public AudioClip failSound;
    private AudioSource audioSource;

    private string correctMethod = "boil";
    private string selectedMethod = "";

    private bool isFirstTreatment = true;

    [Header("Picked Plant UI")]
    public Image pickedPlantImage;

    public ItemData foxgloveData, gingerData;
    private int currentIndex = 0;

    private List<ItemData> treatmentPlants = new List<ItemData>();
    private List<string> treatmentDiseases = new List<string>();
    private List<string> treatmentMethods = new List<string>();



    void Start()
    {
        treatmentPlants.Add(foxgloveData);
        treatmentPlants.Add(gingerData);

        treatmentDiseases.Add("Heart Arrhythmia");
        treatmentDiseases.Add("Nausea");

        treatmentMethods.Add("boil");
        treatmentMethods.Add("grind");

        GameStateManager.Instance.collectedPlant = treatmentPlants[0]; // 初始为第一株
        correctMethod = treatmentMethods[0];


        audioSource = gameObject.AddComponent<AudioSource>();



        // 初始化 UI 状态
        diagnosisPanel.SetActive(true);
        treatmentPanel.SetActive(false);

        medicineResultPanel.SetActive(false);
        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);


        ShowMentorDialogue(); // ✅ 一开始显示导师讲解

        if (brewButton != null)
        {
            brewButton.gameObject.SetActive(true);
            brewButton.onClick.RemoveAllListeners();
            brewButton.onClick.AddListener(() =>
            {

                diagnosisPanel.SetActive(false);
                treatmentPanel.SetActive(true);              // 显示治疗按钮
                SetupTreatmentPanel();                       // 设置文字内容等
            });
        }



        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(ShowNextStep);
    }

    void ShowMentorDialogue()
    {
        var plant = treatmentPlants[currentIndex];  // ✅ 使用当前植物
        string method = correctMethod;

        if (plant == null)
        {
            Debug.LogWarning("[MentorDialogue] ❌ collectedPlant is NULL!");
            return;
        }

        Debug.Log($"[MentorDialogue] ✅ Showing: {plant.itemName} must be {method}");

        mentorDialogueBubble.SetActive(true);
        mentorDialogueText.text =
            $"Now, please prepare the medicine. {plant.itemName} must be <u><b>{method.ToUpper()}</b></u> to extract its healing power.";
    }




    void SetupTreatmentPanel()
    {
        var plant = treatmentPlants[currentIndex];  // ✅ 使用当前植物

        if (plant != null && pickedPlantImage != null)
        {
            pickedPlantImage.sprite = plant.itemIcon;
            pickedPlantImage.enabled = true;
        }

        string disease = treatmentDiseases[currentIndex];  // ✅ 当前疾病名


        if (plant != null)
            plantNameText.text = $"Picked: <color=red>{plant.itemName}</color>";

        if (!string.IsNullOrEmpty(disease))
            patientNameText.text = $"To cure: <color=red>{disease}</color>";

        boilButton.onClick.RemoveAllListeners();
        grindButton.onClick.RemoveAllListeners();

        boilButton.onClick.AddListener(() => StartTreatment("boil"));
        grindButton.onClick.AddListener(() => StartTreatment("grind"));

        boilButton.interactable = true;
        grindButton.interactable = true;
    }

    void StartTreatment(string method)
    {
        selectedMethod = method;

        boilButton.interactable = false;
        grindButton.interactable = false;

        if (method != correctMethod)
        {
            if (failSound != null)
                audioSource.PlayOneShot(failSound);

            HandleMistake(method);
            return;
        }

        currentQTEManager = isFirstTreatment ? qteManager1 : qteManager2;
        currentQTEManager.onQTESuccess = EvaluateTreatment;
        currentQTEManager.onQTEFail = () => HandleMistake(selectedMethod);
        currentQTEManager.StartQTE();
    }


    void EvaluateTreatment()
    {
        if (selectedMethod == correctMethod)
        {
            ApplyTreatmentSuccess();
        }
        else
        {
            HandleMistake(selectedMethod);
        }
    }

    void ApplyTreatmentSuccess()
    {
        Debug.Log("[Treatment] Success! +5 gold");

        GameStateManager.Instance.gold += 5;
        GameStateManager.Instance.patientsCured += 1;

        ItemData plant = treatmentPlants[currentIndex];  // ✅ 使用当前植物

        if (plant != null)
        {
            PlayerInventory.Instance.RemoveItem(plant);
            GameStateManager.Instance.collectedPlant = null;
        }

        treatmentPanel.SetActive(false);
        medicineResultPanel.SetActive(true);
        resultText.text = "Well done! The medicine is ready!";
        successImage.gameObject.SetActive(true);
        failureImage.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);

        SpawnCelebrationEffect();

        isFirstTreatment = false;

    }

    void HandleMistake(string method)
    {
        treatmentPanel.SetActive(false);
        medicineResultPanel.SetActive(true);
        resultText.text = "Oh no... The preparation failed.";
        successImage.gameObject.SetActive(false);
        failureImage.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(false);
    }

    void OnRetryClicked()
    {
        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        medicineResultPanel.SetActive(false);
        treatmentPanel.SetActive(true);

        boilButton.interactable = true;
        grindButton.interactable = true;

        currentQTEManager?.ResetQTE();

    }

    void ShowNextStep()
    {
        currentIndex++;

        if (currentIndex < treatmentPlants.Count)
        {
            // 准备第二轮数据
            GameStateManager.Instance.collectedPlant = treatmentPlants[currentIndex];
            correctMethod = treatmentMethods[currentIndex];
            GameStateManager.Instance.currentDisease = treatmentDiseases[currentIndex];

            // 重置 UI
            continueButton.gameObject.SetActive(false);
            successImage.gameObject.SetActive(false);
            medicineResultPanel.SetActive(false);
            diagnosisPanel.SetActive(true);

            ShowMentorDialogue();
        }
        else
        {
            SceneManager.LoadScene("PostTreatmentScene");
        }
    }


    void SpawnCelebrationEffect()
    {
        if (confettiPrefabs.Count == 0 || confettiCanvas == null) return;

        for (int i = 0; i < 60; i++)
        {
            GameObject prefab = confettiPrefabs[Random.Range(0, confettiPrefabs.Count)];
            GameObject confetti = Instantiate(prefab, confettiCanvas.transform);
            confetti.transform.localPosition = new Vector3(
                Random.Range(-500f, 500f),
                Random.Range(200f, 400f),
                0f
            );
            confetti.transform.localScale = Vector3.one;
        }
    }
}
