using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TreatmentManager1 : MonoBehaviour
{
    [Header("Panels")]
    public GameObject diagnosisPanel;
    public GameObject treatmentPanel;
    public GameObject medicineResultPanel;

    [Header("UI Elements")]
    public TMP_Text patientInfoText;
    public TMP_Text preparedPlantsText;
    public Button boilButton;
    public Button grindButton;
    public Button brewButton;
    public Button retryButton;
    public Button continueButton;

    [Header("Instruction")]
    public GameObject instructionPanel;
    public TMP_Text instructionText;
    public Button instructionContinueButton;

    [Header("QTE Ready Prompt")]
    public GameObject qteReadyPanel;
    public TMP_Text qteReadyText;
    public Button qteStartButton;

    [Header("Result UI")]
    public TMP_Text resultText;
    public Image successImage;
    public Image failureImage;

    [Header("Celebration Effect")]
    public GameObject[] confettiPrefabs;
    public GameObject confettiCanvas;

    [Header("Audio")]
    public AudioClip failSound;

    private ItemData[] requiredPlants;
    private int requiredCount;
    private int collectedCount;

    void Start()
    {
        // 初始化收集数据
        requiredPlants = GameStateManager.Instance.requiredPlants;
        requiredCount = requiredPlants.Length;
        collectedCount = 0;
        foreach (var item in requiredPlants)
            if (PlayerInventory.Instance.GetPlantCount(item) > 0)
                collectedCount++;

        // 隐藏所有面板
        diagnosisPanel.SetActive(true);
        instructionPanel.SetActive(false);
        treatmentPanel.SetActive(false);
        qteReadyPanel.SetActive(false);
        medicineResultPanel.SetActive(false);

        // 显示患者信息和所需草药列表
        patientInfoText.text = "Two patients await treatment!";
        preparedPlantsText.text = $"Herbs needed: {string.Join(", ", System.Array.ConvertAll(requiredPlants, p => p.itemName))}";

        // 绑定诊断按钮
        brewButton.onClick.RemoveAllListeners();
        brewButton.onClick.AddListener(ShowInstructionPanel);

        // 绑定指令面板继续按钮
        instructionContinueButton.onClick.RemoveAllListeners();
        instructionContinueButton.onClick.AddListener(EnterTreatmentPhase);

        // 绑定治疗面板操作，示例直接跳转结果
        boilButton.onClick.RemoveAllListeners();
        boilButton.onClick.AddListener(ShowResult);
        grindButton.onClick.RemoveAllListeners();
        grindButton.onClick.AddListener(ShowResult);
        brewButton.onClick.AddListener(ShowResult);

        // 绑定结果面板按钮
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinue);
    }

    void ShowInstructionPanel()
    {
        diagnosisPanel.SetActive(false);
        instructionPanel.SetActive(true);
        instructionText.text = "Prepare the medicine with the herbs you collected.";
    }

    void EnterTreatmentPhase()
    {
        instructionPanel.SetActive(false);
        treatmentPanel.SetActive(true);
    }

    void ShowResult()
    {
        treatmentPanel.SetActive(false);
        medicineResultPanel.SetActive(true);
        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        if (collectedCount >= requiredCount)
        {
            successImage.gameObject.SetActive(true);
            failureImage.gameObject.SetActive(false);
            resultText.text = "Excellent! All required herbs collected.";
            PlayConfetti();
            continueButton.gameObject.SetActive(true);
        }
        else
        {
            successImage.gameObject.SetActive(false);
            failureImage.gameObject.SetActive(true);
            resultText.text = "You failed to collect all required herbs. Game Over.";
            continueButton.gameObject.SetActive(true);
            if (failSound != null)
                AudioSource.PlayClipAtPoint(failSound, Vector3.zero);
        }
    }

    void OnContinue()
    {
        SceneManager.LoadScene("PostTreatmentScene");
    }

    void PlayConfetti()
    {
        foreach (var prefab in confettiPrefabs)
        {
            Instantiate(prefab, confettiCanvas.transform);
        }
    }

    void CheckTreatmentCompletion()
    {
        int collectedCount = 0;
        int requiredCount = requiredPlants.Length;

        foreach (var plant in requiredPlants)
        {
            if (PlayerInventory.Instance.GetPlantCount(plant) > 0)
            {
                collectedCount++;
            }
        }

        if (collectedCount >= requiredCount)
        {
            resultText.text = "Medicine prepared!";
            resultText.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(GoToPostTreatment);
        }
        else
        {
            resultText.text = $"You collected {collectedCount}/{requiredCount} herbs. Game Over.";
            resultText.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(GoToGameOver);
        }
    }

    void GoToPostTreatment()
    {
        SceneManager.LoadScene("PostTreatmentScene");
    }

    void GoToGameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}
