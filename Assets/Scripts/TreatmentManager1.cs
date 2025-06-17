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

    public GameObject mentorPortrait;


    [Header("Mentor Dialogue Flow")]
    public Button mentorNextButton;


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

    public GameObject backpackPromptBubble;
    public TMP_Text backpackPromptText;

    public Button retryPlantSelectionButton; // 🆕 选择植物失败的 Retry 按钮

    public static TreatmentManager1 Instance { get; private set; }


    private bool awaitingPlantSelection = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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

        // 绑定 next 按钮事件
        if (mentorNextButton != null)
        {
            mentorNextButton.onClick.RemoveAllListeners();
            mentorNextButton.onClick.AddListener(OnMentorNextClicked);
            mentorNextButton.gameObject.SetActive(true); // 先显示 mentor next 按钮
        }

        if (brewButton != null)
        {
            brewButton.gameObject.SetActive(false);
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

        if (retryPlantSelectionButton != null)
        {
            retryPlantSelectionButton.gameObject.SetActive(false); // ✅ 先隐藏
            retryPlantSelectionButton.onClick.RemoveAllListeners(); // 避免重复
            retryPlantSelectionButton.onClick.AddListener(OnRetryPlantSelection); // ✅ 加上这句
        }

    }

    void ShowMentorDialogue()
    {
        var plant = treatmentPlants[currentIndex];
        string method = correctMethod;

        if (plant == null)
        {
            Debug.LogWarning($"[MentorDialogue] ❌ treatmentPlants[{currentIndex}] is NULL!");
            return;
        }

        if (isFirstTreatment)
        {
            mentorDialogueBubble.SetActive(true);
            mentorDialogueText.text = "Do you remember which plant is used to treat <b>Heart Arrhythmia</b>?";

            // 下一步按钮已由 Start 中控制
        }
        else
        {
            mentorDialogueBubble.SetActive(true);
            mentorDialogueText.text =
                $"Now, please prepare the medicine. {plant.itemName} must be <u><b>{method.ToUpper()}</b></u> to extract its healing power.";
        }
    }


    void Update()
    {
        if (awaitingPlantSelection && Input.GetKeyDown(KeyCode.Tab))
        {
            if (BackpackSystemManager.Instance != null)
            {
                BackpackSystemManager.Instance.OpenBackpack();
            }

            if (backpackPromptBubble != null)
                backpackPromptBubble.SetActive(false);
        }
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

    public void OnPlantSelected(ItemData selected)
    {
        if (!awaitingPlantSelection) return;

        if (selected == foxgloveData)
        {
            GameStateManager.Instance.collectedPlant = foxgloveData;
            correctMethod = "boil";

            // ✅ 确保 UI 激活
            if (mentorDialogueBubble != null)
                mentorDialogueBubble.SetActive(true);
            if (mentorPortrait != null)
                mentorPortrait.SetActive(true);

            mentorDialogueText.text = "Correct. Foxglove must be <b>boiled</b> to extract its healing power.";
            brewButton.gameObject.SetActive(true);
            awaitingPlantSelection = false;

            if (backpackPromptBubble != null)
                backpackPromptBubble.SetActive(false);

            if (BackpackSystemManager.Instance != null)
                BackpackSystemManager.Instance.CloseBackpack();

        }
        else
        {
            if (mentorDialogueBubble != null)
                mentorDialogueBubble.SetActive(true);
            if (mentorPortrait != null)
                mentorPortrait.SetActive(true);

            mentorDialogueText.text = "That’s not the right plant for this illness.";
            retryPlantSelectionButton.gameObject.SetActive(true);

            if (BackpackSystemManager.Instance != null)
                BackpackSystemManager.Instance.CloseBackpack();

        }
    }

    public void OnRetryPlantSelection()
    {

        Debug.Log("[RetryPlant] Retry button clicked.");

        mentorDialogueText.text = "Try again. Which plant treats <b>Heart Arrhythmia</b>?";

        if (backpackPromptBubble != null && backpackPromptText != null)
        {
            backpackPromptText.text = "Press          to open your backpack.";
            backpackPromptBubble.SetActive(true);
        }

        if (BackpackSystemManager.Instance != null)
            BackpackSystemManager.Instance.CloseBackpack();


        awaitingPlantSelection = true;

        retryPlantSelectionButton.gameObject.SetActive(false);
    }

    void OnMentorNextClicked()
    {
        if (mentorNextButton != null)
            mentorNextButton.gameObject.SetActive(false); // ✅ 隐藏 next

        if (mentorDialogueBubble != null)
            mentorDialogueBubble.SetActive(false);        // ✅ 隐藏对话框

        if (mentorPortrait != null)
            mentorPortrait.SetActive(false);


        if (backpackPromptBubble != null && backpackPromptText != null)
        {
            backpackPromptText.text = "Press          to open your backpack.";
            backpackPromptBubble.SetActive(true);         // ✅ 显示 TAB 提示
        }

        awaitingPlantSelection = true;
    }


}



