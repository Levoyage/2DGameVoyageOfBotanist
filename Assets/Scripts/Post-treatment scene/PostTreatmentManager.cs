using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PostTreatmentManager : MonoBehaviour
{
    [Header("Patient Dialogue")]
    public GameObject patientBubble;
    public TMP_Text patientText;
    public Button nextButton;

    [Header("Tutor Dialogue")]
    public GameObject tutorBubble;
    public TMP_Text tutorText;
    public Button tutorNextButton;
    public Button startGameButton;
    public Button backButton;

    [Header("Reward UI")]
    public GameObject rewardUIPanel;
    public GameObject textBackground;




    [Header("Audio")]
    public AudioClip rewardSound;
    private AudioSource audioSource;

    private int dialogueIndex = 0;
    private string[] mentorLines;

    [Header("Post-Treatment Continue")]
    public Button continueButton;

    [Header("UI Elements")]
    public GameObject coinFrame; // 拖入 CoinFrame


    private Coroutine rewardCoroutine;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // 1. 初始病人感谢 → 导师第一句
            if (nextButton != null && nextButton.gameObject.activeInHierarchy)
            {
                nextButton.onClick.Invoke();
            }

            // 2. 导师第一句 → 奖励面板
            else if (tutorNextButton != null && tutorNextButton.gameObject.activeInHierarchy && !rewardUIPanel.activeInHierarchy)
            {
                tutorNextButton.onClick.Invoke();
            }

            //3.rewardUIPanel → 导师第二句
            else if (continueButton != null && continueButton.gameObject.activeInHierarchy)
            {
                continueButton.onClick.Invoke();
            }
            // 4. 最后一句 mentorLines 出现 → 显示 StartGameButton
            else if (startGameButton != null && startGameButton.gameObject.activeInHierarchy)
            {
                startGameButton.onClick.Invoke();
            }
        }
    }

    void Start()
    {
        mentorLines = new string[] {
            "For each patient you heal, you will earn <color=red><b>5 gold coins</b></color>. When you have enough coins, you can afford to <color=red><b>travel to new places</b></color> to gather herbs.",
            "Your training is complete. From here, your path lies among wild herbs and ailing souls."
        };

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.gold = 5;
            GameStateManager.Instance.patientsCured++;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initial state: show patient gratitude bubble
        patientBubble.SetActive(true);
        patientText.text = "Thank you! I feel much better now!";

        tutorBubble.SetActive(false);
        rewardUIPanel.SetActive(false);
        if (textBackground != null)
            textBackground.SetActive(false);
        tutorNextButton.gameObject.SetActive(false);
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);
        if (backButton != null)
            backButton.gameObject.SetActive(false);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(ShowTutorDialogue);
        nextButton.gameObject.SetActive(true);

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartMainGame);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ShowPreviousMentorLine);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ShowFinalMentorLines);
        }

        if (coinFrame != null)
            coinFrame.SetActive(false);


    }

    void ShowTutorDialogue()
    {
        // Hide patient dialogue
        patientBubble.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Show tutor dialogue
        tutorBubble.SetActive(true);
        tutorText.text = "Well done. Your treatment was effective and precise.";

        tutorNextButton.onClick.RemoveAllListeners();
        tutorNextButton.onClick.AddListener(ShowRewardPanel);
        tutorNextButton.gameObject.SetActive(true);
    }

    void ShowRewardPanel()
    {
        tutorBubble.SetActive(false);
        tutorNextButton.gameObject.SetActive(false);

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnPatientCured(true);
        }
        else
        {
            Debug.LogWarning("GameStateManager is null. Skipping OnPatientCured.");
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.gold = 5;
            GameStateManager.Instance.patientsCured++;
        }

        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(true);

        if (rewardSound != null && audioSource != null)
            audioSource.PlayOneShot(rewardSound);

        if (textBackground != null)
            textBackground.SetActive(true);

        rewardCoroutine = StartCoroutine(HideRewardAndShowEconomy());
    }

    IEnumerator HideRewardAndShowEconomy()
    {
        yield return new WaitForSeconds(3f);

        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(false);

        if (textBackground != null)
            textBackground.SetActive(false);



        dialogueIndex = 0;
        ShowMentorLine();
    }

    void ShowMentorLine()
    {
        tutorBubble.SetActive(true);
        tutorText.text = mentorLines[dialogueIndex];

        if (backButton != null)
            backButton.gameObject.SetActive(dialogueIndex > 0);

        tutorNextButton.onClick.RemoveAllListeners();

        if (dialogueIndex < mentorLines.Length - 1)
        {
            tutorNextButton.gameObject.SetActive(true);
            tutorNextButton.onClick.AddListener(() =>
            {
                dialogueIndex++;
                ShowMentorLine();
            });

            if (startGameButton != null)
                startGameButton.gameObject.SetActive(false);
        }
        else
        {
            tutorNextButton.gameObject.SetActive(false);
            if (startGameButton != null)
                startGameButton.gameObject.SetActive(true);
        }
    }

    void ShowPreviousMentorLine()
    {
        if (dialogueIndex > 0)
        {
            dialogueIndex--;
            ShowMentorLine();
        }
    }

    void ShowFinalMentorLines()
    {
        mentorLines = new string[] {
        "For each patient you heal, you will earn <color=red><b>5 gold coins</b></color>. When you have enough coins, you can afford to <color=red><b>travel to new places</b></color> to gather herbs.",
        "Your training is complete. From here, your path lies among wild herbs and ailing souls."
    };

        dialogueIndex = 0;

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);  // 隐藏自己

        if (coinFrame != null)
            coinFrame.SetActive(true);


        ShowMentorLine(); // 🔁 使用原有 mentor line 系统展示台词

        if (rewardCoroutine != null)
        {
            StopCoroutine(rewardCoroutine);
            rewardCoroutine = null;
        }

        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(false);

        if (textBackground != null)
            textBackground.SetActive(false);

    }


    void StartMainGame()
    {
        SceneManager.LoadScene("ClinicScene-1"); // Replace with actual scene name
    }
}
