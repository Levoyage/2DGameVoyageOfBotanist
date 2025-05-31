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

    [Header("Economy HUD")]
    public GameObject economyUIObject; // GameObject with EconomyUIController

    [Header("Audio")]
    public AudioClip rewardSound;
    private AudioSource audioSource;

    private int dialogueIndex = 0;
    private string[] mentorLines;

    void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.gold = 0;
            GameStateManager.Instance.supplyPacks = 0;
        }
        else
        {
            Debug.LogWarning("⚠️ GameStateManager.Instance is null in PostTreatmentScene.");
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

        mentorLines = new string[] {
            "Supply Packs are used when you travel to gather herbs in the wild. Be sure to keep enough on hand.",
            "Since you used the clinic’s own supplies to treat the patient, no additional payment is granted.",
            "You’ll earn one supply pack for each successful treatment. But only gold will let you explore new regions to gather rarer herbs.",
            "Your training is complete. From here, your path lies among wild herbs and ailing souls."
        };

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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
            GameStateManager.Instance.supplyPacks = 1;
            GameStateManager.Instance.patientsCured++;
        }

        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(true);

        if (rewardSound != null && audioSource != null)
            audioSource.PlayOneShot(rewardSound);

        if (textBackground != null)
            textBackground.SetActive(true);

        StartCoroutine(HideRewardAndShowEconomy());
    }

    IEnumerator HideRewardAndShowEconomy()
    {
        yield return new WaitForSeconds(3f);

        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(false);

        if (textBackground != null)
            textBackground.SetActive(false);

        if (economyUIObject != null)
        {
            var econ = economyUIObject.GetComponent<EconomyUIController>();
            if (econ != null)
                econ.ShowHUD();
        }

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

    void StartMainGame()
    {
        SceneManager.LoadScene("ClinicScene-1"); // Replace with actual scene name
    }
}
