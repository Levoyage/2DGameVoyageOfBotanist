using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MapSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public Button backButton;
    public Button backToMenuButton;

    public GameObject textBackground;  // ← 指向整个对白框对象（TextBackground）

    [Header("Dialogue Content")]
    [SerializeField, TextArea(3, 10)]
    private List<string> mentorLines = new List<string>
    {
        "This is the map of Europe. Each time you successfully cure a patient, it brings you closer to <color=red>the wider world</color>.",
        "You can see our current location in the <color=red>south of France</color>.",
        "Once you’ve collected 50 coins, you’ll have enough travel fare to journey into the <color=red>north of Spain</color>.",
        "There, you’ll be able to collect new types of <color=red>herbs</color> not found around here."
    };

    private int currentLineIndex = 0;

    void Start()
    {
        backToMenuButton.gameObject.SetActive(false);
        UpdateDialogue();

        nextButton.onClick.AddListener(NextLine);
        backButton.onClick.AddListener(PreviousLine);
        backToMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    void UpdateDialogue()
    {
        dialogueText.text = mentorLines[currentLineIndex];

        backButton.gameObject.SetActive(currentLineIndex > 0);
        nextButton.gameObject.SetActive(true);                  // 总是显示 Next，除非最后点击后关闭
        backToMenuButton.gameObject.SetActive(false);
        if (textBackground != null) textBackground.SetActive(true);
    }


    void NextLine()
    {
        if (currentLineIndex < mentorLines.Count - 1)
        {
            currentLineIndex++;
            UpdateDialogue();
        }
        else
        {
            // 正在最后一句时点 Next，立即隐藏对白框，显示返回按钮
            if (textBackground != null)
                textBackground.SetActive(false);

            backToMenuButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);  // 只隐藏 Next
                                                     // 不再隐藏 backButton
        }
    }


    void PreviousLine()
    {
        if (currentLineIndex > 0)
        {
            currentLineIndex--;
            UpdateDialogue();
        }
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
