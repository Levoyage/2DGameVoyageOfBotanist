using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClinicManager : MonoBehaviour
{
    [Header("Intro Dialogue (Herb Encyclopedia)")]
    public GameObject introDialogueBubble;
    public TextMeshProUGUI introDialogueText;
    public Button introNextButton;

    [Header("Patient Dialogue")]
    public GameObject patientDialogueBubble;
    public TextMeshProUGUI patientDialogueText;
    public Button patientNextButton;

    [Header("Mentor Dialogue (Diagnosis Prompt)")]
    public GameObject mentorDialogueBubble;
    public TextMeshProUGUI mentorDialogueText;

    [Header("Diagnosis System")]
    public GameObject diagnosisPanel;
    public Button eczemaButton, woundInfectionButton, scurvyButton;
    public TextMeshProUGUI doctorFeedbackText;
    public GameObject gatherButton;

    [Header("Tab Instruction Bubble")]
    public GameObject instructionBubble;
    public TextMeshProUGUI instructionText;
    public GameObject secondInstructionBubble;
    public TextMeshProUGUI secondInstructionText;

    [Header("Character Portrait")]
    public GameObject portrait;

    private string[] herbIntroLines = {
        "From now on, you'll carry your own satchel for herbs. Keep it tidy.",
        "We've also compiled an encyclopedia for the plants you find.",
        "Check it often — knowledge saves lives."
    };

    private string tabInstructionLine = "Press Tab to open your encyclopedia and backpack.";
    private string secondTabInstruction = "Press Tab again to proceed.";

    private string[] patientDialogue = {
        "My skin is red and itchy.",
        "These rashes won’t go away, and they burn."
    };

    private string mentorQuestion = "What is your diagnosis, apprentice?";

    private int herbIntroIndex = 0;
    private int patientDialogueIndex = 0;

    private enum Stage
    {
        Intro,
        WaitForTab,
        BackpackOpen,
        PatientDialogue,
        Diagnosis
    }

    private Stage currentStage = Stage.Intro;
    public static string selectedDiagnosis = "";

    void Start()
    {
        introDialogueBubble.SetActive(true);
        mentorDialogueBubble.SetActive(false);
        patientDialogueBubble.SetActive(false);
        instructionBubble.SetActive(false);
        secondInstructionBubble.SetActive(false);
        diagnosisPanel.SetActive(false);
        gatherButton.SetActive(false);
        doctorFeedbackText.gameObject.SetActive(false);

        introDialogueText.text = herbIntroLines[herbIntroIndex++];
        introNextButton.onClick.RemoveAllListeners();
        introNextButton.onClick.AddListener(ShowNextDialogue);

        patientNextButton.onClick.RemoveAllListeners();
        patientNextButton.onClick.AddListener(ShowNextDialogue);

        eczemaButton.onClick.AddListener(() => CheckDiagnosis("Eczema"));
        woundInfectionButton.onClick.AddListener(() => CheckDiagnosis("Wound Infection"));
        scurvyButton.onClick.AddListener(() => CheckDiagnosis("Scurvy"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            switch (currentStage)
            {
                case Stage.WaitForTab:
                    if (BackpackSystemManager.Instance != null)
                        BackpackSystemManager.Instance.OpenBackpack();

                    instructionBubble.SetActive(false);
                    secondInstructionBubble.SetActive(true);
                    secondInstructionText.text = secondTabInstruction;
                    currentStage = Stage.BackpackOpen;
                    break;

                case Stage.BackpackOpen:
                    if (BackpackSystemManager.Instance != null)
                        BackpackSystemManager.Instance.CloseBackpack();

                    secondInstructionBubble.SetActive(false);
                    currentStage = Stage.PatientDialogue;
                    patientDialogueBubble.SetActive(true);
                    patientDialogueText.text = patientDialogue[patientDialogueIndex++];
                    break;

                case Stage.PatientDialogue:
                    if (BackpackSystemManager.Instance != null)
                    {
                        if (BackpackSystemManager.Instance.IsBackpackOpen())
                            BackpackSystemManager.Instance.CloseBackpack();
                        else
                            BackpackSystemManager.Instance.OpenBackpack();
                    }
                    break;
            }
        }
    }

    public void ShowNextDialogue()
    {
        switch (currentStage)
        {
            case Stage.Intro:
                if (herbIntroIndex < herbIntroLines.Length)
                {
                    introDialogueText.text = herbIntroLines[herbIntroIndex++];
                }
                else
                {
                    introDialogueBubble.SetActive(false);
                    instructionBubble.SetActive(true);
                    instructionText.text = tabInstructionLine;

                    if (portrait != null) portrait.SetActive(false);

                    introNextButton.gameObject.SetActive(false);
                    currentStage = Stage.WaitForTab;
                }
                break;

            case Stage.PatientDialogue:
                if (patientDialogueIndex < patientDialogue.Length)
                {
                    patientDialogueText.text = patientDialogue[patientDialogueIndex++];
                }
                else
                {
                    EndPatientDialogue();
                }
                break;
        }
    }

    void EndPatientDialogue()
    {
        patientDialogueBubble.SetActive(false);
        mentorDialogueBubble.SetActive(true);
        mentorDialogueText.text = mentorQuestion;
        diagnosisPanel.SetActive(true);
        currentStage = Stage.Diagnosis;
    }

    void CheckDiagnosis(string choice)
    {
        mentorDialogueBubble.SetActive(false);
        selectedDiagnosis = choice;

        if (choice == "Eczema")
        {
            doctorFeedbackText.text = "Correct! Red, itchy rashes and burning skin suggest eczema. The best remedy? A soothing herbal balm with pimpernel.";
            gatherButton.SetActive(true);
        }
        else if (choice == "Scurvy")
        {
            doctorFeedbackText.text = "Think carefully. The patient has no swollen gums. Try again.";
        }
        else
        {
            doctorFeedbackText.text = "Think carefully. The patient has no wounds. Try again.";
        }

        doctorFeedbackText.gameObject.SetActive(true);
    }

    public void GoToFieldScene()
    {
        SceneManager.LoadScene("FieldScene");
    }
}
