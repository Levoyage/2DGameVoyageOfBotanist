using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TreatmentManager : MonoBehaviour
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

    [Header("Instruction")]
    public GameObject instructionPanel;
    public TMP_Text instructionText;
    public Button instructionContinueButton;

    [Header("QTE System")]
    public QTEProgressBar qteProgressBar;
    public QTERhythmManager qteRhythmManager;

    [Header("QTE Ready Prompt")]
    public GameObject qteReadyPanel;
    public TMP_Text qteReadyText;
    public Button qteStartButton;

    [Header("Settings")]
    //public ItemData preparedMedicine;
    private string correctMethod = "boil";
    private string selectedMethod = "";

    [Header("Result UI")]
    public TMP_Text resultText;
    public Image successImage;
    public Image failureImage;

    [Header("Celebration Effect")]
    public List<GameObject> confettiPrefabs;
    public GameObject confettiCanvas;

    [Header("Audio")]
    public AudioClip failSound;
    private AudioSource audioSource;

    void Start()
    {
        treatmentPanel.SetActive(false);
        medicineResultPanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (brewButton != null)
            brewButton.onClick.AddListener(() =>
            {
                diagnosisPanel.SetActive(false);
                treatmentPanel.SetActive(true);
                instructionPanel.SetActive(true);
                instructionText.text = "Now, choose carefully: grinding or boiling. Once chosen, be ready — your timing matters.";
            });

        if (instructionContinueButton != null)
        {
            instructionContinueButton.onClick.RemoveAllListeners();
            instructionContinueButton.onClick.AddListener(CloseInstructionPanel);
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(ShowNextStep);

        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        if (qteReadyPanel != null)
            qteReadyPanel.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();

    }

    void CloseInstructionPanel()
    {
        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        OpenTreatmentPanel();
    }

    public void OpenTreatmentPanel()
    {
        if (GameStateManager.Instance.collectedPlant != null)
            plantNameText.text = GameStateManager.Instance.collectedPlant.itemName;

        if (!string.IsNullOrEmpty(GameStateManager.Instance.currentDisease))
            patientNameText.text = GameStateManager.Instance.currentDisease;

        boilButton.onClick.RemoveAllListeners();
        grindButton.onClick.RemoveAllListeners();

        boilButton.onClick.AddListener(() => StartTreatment("boil"));
        grindButton.onClick.AddListener(() => StartTreatment("grind"));

        boilButton.interactable = true;
        grindButton.interactable = true;

        if (qteRhythmManager != null)
        {
            qteRhythmManager.onQTESuccess = EvaluateTreatment;
            qteRhythmManager.onQTEFail = () => HandleMistake(selectedMethod);
        }

        StartCoroutine(ForceRebuildLayoutNextFrame());
    }

    void StartTreatment(string method)
    {
        selectedMethod = method;

        boilButton.interactable = false;
        grindButton.interactable = false;

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (method != correctMethod)
        {
            if (failSound != null)
                audioSource.PlayOneShot(failSound);

            HandleMistake(method);
            return;
        }

        if (qteReadyPanel != null)
        {
            qteReadyPanel.SetActive(true);
            if (qteReadyText != null)
                qteReadyText.text = "Steady hands now... It's time to follow the rhythm exactly — no room for mistakes.";

            if (qteStartButton != null)
            {
                qteStartButton.onClick.RemoveAllListeners();
                qteStartButton.onClick.AddListener(() =>
                {
                    qteReadyPanel.SetActive(false);
                    if (qteRhythmManager != null)
                    {
                        qteRhythmManager.StartQTE();
                        Debug.Log("[Treatment] Started QTE rhythm system");
                    }
                });
            }
        }
        else
        {
            if (qteRhythmManager != null)
            {
                qteRhythmManager.StartQTE();
                Debug.Log("[Treatment] Started QTE rhythm system (fallback)");
            }
        }
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

        // ---------- 关键：消耗原料 ----------
        bool removed = false;
        PlayerInventory inv = PlayerInventory.Instance;

        if (inv != null)
        {
            // ① 尝试用引用直接删除
            ItemData rawPlant = GameStateManager.Instance.collectedPlant;
            if (rawPlant != null)
                removed = inv.RemoveItem(rawPlant);

            // ② 如果引用为空或删除失败，就按名字删第一株同名植物
            if (!removed)
                removed = inv.RemoveItemByName("Pimpernel");   // ← 如需治疗别的草，请改这里
        }

        // 无论成败都清空引用，防止多次消耗
        GameStateManager.Instance.collectedPlant = null;

        // ---------- UI 与反馈 ----------
        treatmentPanel.SetActive(false);
        medicineResultPanel.SetActive(true);
        resultText.text = "Well done! The medicine is ready!";
        successImage.gameObject.SetActive(true);
        failureImage.gameObject.SetActive(false);

        continueButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);

        SpawnCelebrationEffect();
    }

    void HandleMistake(string method)
    {
        GameStateManager.Instance.collectedPlant = null;
        Debug.LogWarning("[Treatment] Mistake! Herb wasted.");

        treatmentPanel.SetActive(false);
        medicineResultPanel.SetActive(true);
        resultText.text = "Oh no... The preparation failed.";
        successImage.gameObject.SetActive(false);
        failureImage.gameObject.SetActive(true);

        continueButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
    }

    public void OnRetryClicked()
    {
        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        medicineResultPanel.SetActive(false);
        treatmentPanel.SetActive(true);

        boilButton.interactable = true;
        grindButton.interactable = true;

        if (qteRhythmManager != null)
            qteRhythmManager.ResetQTE();
    }

    void ShowNextStep()
    {
        Debug.Log("[Treatment] Proceeding to post-treatment scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("PostTreatmentScene");
    }

    private IEnumerator ForceRebuildLayoutNextFrame()
    {
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(plantNameText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(patientNameText.rectTransform);
    }

    void SpawnCelebrationEffect()
    {
        if (confettiPrefabs.Count == 0 || confettiCanvas == null)
        {
            Debug.LogWarning("[Confetti] Missing prefab or canvas!");
            return;
        }

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

        Debug.Log("[Confetti] Spawned celebration confetti!");
    }
}
