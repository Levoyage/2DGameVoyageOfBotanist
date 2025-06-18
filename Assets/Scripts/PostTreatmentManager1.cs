using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PostTreatmentManager1 : MonoBehaviour
{
    [Header("Patients")]
    public GameObject patient1Portrait;
    public GameObject patient2Portrait;
    public GameObject patientBubble;
    public Button patientNextButton;

    public TMP_Text patientText;

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

    [Header("Continue Button")]
    public Button continueButton;


    [Header("Coin UI")]
    public GameObject coinFrame;             // Frame 父物体，用于设置显示
    public TMP_Text goldText;                // 实际的金币数文本

    [Header("Codex Herbs")]
    public ItemData pimpernel;
    public ItemData foxglove;
    public ItemData ginger;

    [Header("Codex UI Controller")]
    public CodexUIController codexUIController;

    private string[] mentorLines;
    private int dialogueIndex = 0;


    void EnsureBackpackSystemExists()
    {
        if (BackpackSystemManager.Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("BackpackSystemManager");
            if (prefab != null)
            {
                Instantiate(prefab);
            }
            else
            {
                Debug.LogError("❌ BackpackSystemManager prefab not found in Resources.");
            }
        }
    }

    void Start()
    {

        EnsureBackpackSystemExists();

        if (BackpackSystemManager.Instance != null)
            BackpackSystemManager.Instance.InitializeIfNeeded();

        // 🔥 加载图鉴，并添加新 herb
        var codexUI = FindObjectOfType<CodexUIController>();
        if (codexUI != null)
        {
            var list = new List<ItemData>(codexUI.knownHerbs);
            if (!list.Contains(foxglove)) list.Add(foxglove);
            if (!list.Contains(ginger)) list.Add(ginger);
            codexUI.knownHerbs = list;
        }

        mentorLines = new string[] {
            "Excellent work. You've healed two patients and earned <color=red><b>10 gold coins</b></color>.",
            "Keep treating others and gather more coins to travel and collect new herbs.",
            "Your latest medicine has been recorded in your encyclopedia. Press <color=red><b>Tab</b></color> to open your backpack and view it."
        };

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.gold = 10;
            GameStateManager.Instance.patientsCured = 2;
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();


        SetupInitialUI();


        if (BackpackSystemManager.Instance != null)
            BackpackSystemManager.Instance.InitializeIfNeeded();

        StartCoroutine(AddCodexEntriesWhenReady());   // ← 新协程

    }

    IEnumerator AddCodexEntriesWhenReady()
    {
        // ① 等到场景里真有 CodexUIController（含 inactive）
        yield return new WaitUntil(() => FindObjectOfType<CodexUIController>(true) != null);

        CodexUIController codex = FindObjectOfType<CodexUIController>(true);

        if (codex == null)
        {
            Debug.LogError("[PostTreatment] CodexUIController still missing!");
            yield break;
        }

        // ② 组装新增条目（空引用自动过滤）
        var toAdd = new List<ItemData>();
        if (foxglove != null) toAdd.Add(foxglove);
        if (ginger != null) toAdd.Add(ginger);

        codex.AddNewEntries(toAdd);   // ← 自动去重并刷新 UI
        Debug.Log($"[PostTreatment] 已向图鉴追加 {toAdd.Count} 种草药");
    }


    void SetupInitialUI()
    {
        // 显示两个病人
        if (patient1Portrait != null) patient1Portrait.SetActive(true);
        if (patient2Portrait != null) patient2Portrait.SetActive(true);

        patientBubble.SetActive(true);
        if (patientNextButton != null)
        {
            patientNextButton.gameObject.SetActive(true);
            patientNextButton.onClick.RemoveAllListeners();
            patientNextButton.onClick.AddListener(() =>
            {
                patientBubble.SetActive(false);
                patientNextButton.gameObject.SetActive(false);

                tutorBubble.SetActive(true);
                tutorText.text = "Well done. Your treatment was effective and precise.";
                tutorNextButton.gameObject.SetActive(true);
            });
        }

        patientText.text = "Thank you! We both feel so much better!";

        tutorBubble.SetActive(false);
        tutorNextButton.gameObject.SetActive(false);
        rewardUIPanel.SetActive(false);
        textBackground.SetActive(false);
        continueButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        // 绑定 continue 按钮
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(ShowFinalMentorLines);

        // 绑定 tutor 按钮
        tutorNextButton.onClick.RemoveAllListeners();
        tutorNextButton.onClick.AddListener(ShowRewardPanel);

        if (coinFrame != null)
            coinFrame.SetActive(true);

        if (goldText != null)              // ← 初始数字 5
            goldText.text = "5";

    }



    void ShowRewardPanel()
    {
        tutorBubble.SetActive(false);
        tutorNextButton.gameObject.SetActive(false);

        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(true);

        if (rewardSound != null && audioSource != null)
            audioSource.PlayOneShot(rewardSound);

        if (textBackground != null)
            textBackground.SetActive(true);

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            rewardUIPanel.SetActive(false);
            textBackground.SetActive(false);

            // ✅ 立刻刷新金币 UI
            if (goldText != null)
                goldText.text = "15";

            ShowMentorLine();  // 然后再继续导师话
        });



    }



    void ShowMentorLine()
    {
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);  // 隐藏自己

        tutorBubble.SetActive(true);
        tutorText.text = mentorLines[dialogueIndex];

        backButton.gameObject.SetActive(dialogueIndex > 0);
        tutorNextButton.gameObject.SetActive(dialogueIndex < mentorLines.Length - 1);
        continueButton.gameObject.SetActive(dialogueIndex == mentorLines.Length - 1);

        tutorNextButton.onClick.RemoveAllListeners();
        tutorNextButton.onClick.AddListener(() =>
        {
            dialogueIndex++;
            ShowMentorLine();
        });

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
        {
            dialogueIndex = Mathf.Max(0, dialogueIndex - 1);
            ShowMentorLine();
        });
    }

    void ShowFinalMentorLines()
    {
        continueButton.gameObject.SetActive(false);
        tutorBubble.SetActive(false);

        goldText.text = GameStateManager.Instance.gold.ToString();//refresh gold UI to 10




        if (BackpackSystemManager.Instance != null)
            BackpackSystemManager.Instance.OpenBackpack();


        if (startGameButton != null)
            startGameButton.gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (tutorNextButton != null && tutorNextButton.gameObject.activeInHierarchy)
                tutorNextButton.onClick.Invoke();
            else if (continueButton != null && continueButton.gameObject.activeInHierarchy)
                continueButton.onClick.Invoke();
            else if (startGameButton != null && startGameButton.gameObject.activeInHierarchy)
                startGameButton.onClick.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (BackpackSystemManager.Instance != null)
            {
                BackpackSystemManager.Instance.OpenBackpack();
            }
        }
    }

    public void StartMainGame()
    {
        SceneManager.LoadScene("ClinicScene-1");
    }
}
