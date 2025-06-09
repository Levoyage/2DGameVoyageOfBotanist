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

    [Header("Instruction Bubbles")]
    public GameObject instructionBubble;
    public TextMeshProUGUI instructionText;
    public GameObject secondInstructionBubble;
    public TextMeshProUGUI secondInstructionText;

    public GameObject codexInstructionBubble;
    public TextMeshProUGUI codexInstructionText;
    public GameObject satchelInstructionBubble;
    public TextMeshProUGUI satchelInstructionText;

    public GameObject promptPanel;

    public GameObject gatherIntroBubble1;
    public TextMeshProUGUI gatherIntroText1;
    public GameObject gatherIntroBubble2;
    public TextMeshProUGUI gatherIntroText2;

    [Header("Character Portrait")]
    public GameObject portrait;

    private string[] herbIntroLines = {
        "From now on, you'll carry your own satchel for herbs. Keep it tidy.",
        "We've also compiled an encyclopedia for the plants you find.",
        "Check it often — knowledge saves lives."
    };

    private string codexInstruction = "This is your botanical codex. It shall preserve records of the plants you collect.";
    private string satchelInstruction = "This is your satchel. It contains only the plants you have gathered correctly and with care.";
    private string tabInstructionLine = "Press Tab to open your encyclopedia and backpack.";
    private string secondTabInstruction = "Press <b>Tab</b> again or click <b><color=#9C7B54>'x'</color></b> to proceed →";

    private string[] patientDialogue = {
        "My skin is red and itchy.",
        "These rashes won’t go away, and they burn."
    };

    private string mentorIntroLine1 = "You’ve done well with simple cases. But this time, two patients await. Observe carefully, and once diagnosed, you’ll gather the herbs yourself.";
    private string mentorIntroLine2 = "Time will not wait, and missteps come with cost. Should you run short, the emergency kits we’ve stocked may suffice—but only once, and for one. Choose wisely, or their fates may be sealed.";
    private string mentorQuestion = "What is your diagnosis, apprentice?";

    private int herbIntroIndex = 0;
    private int patientDialogueIndex = 0;
    private int mentorIntroStage = 0;

    private enum Stage
    {
        Intro,
        WaitForTab,
        BackpackOpen,
        PatientDialogue,
        MentorIntro,
        Diagnosis
    }

    private Stage currentStage = Stage.Intro;
    public static string selectedDiagnosis = "";

    void Start()
    {
        if (BackpackSystemManager.Instance != null)
        {
            BackpackSystemManager.Instance.InitializeIfNeeded();
        }

        introDialogueBubble.SetActive(true);
        mentorDialogueBubble.SetActive(false);
        patientDialogueBubble.SetActive(false);
        instructionBubble.SetActive(false);
        secondInstructionBubble.SetActive(false);
        codexInstructionBubble.SetActive(false);
        satchelInstructionBubble.SetActive(false);
        diagnosisPanel.SetActive(false);
        gatherButton.SetActive(false);
        doctorFeedbackText.gameObject.SetActive(false);
        promptPanel.SetActive(false);
        gatherIntroBubble1.SetActive(false);
        gatherIntroBubble2.SetActive(false);
        if (portrait != null) portrait.SetActive(true);

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
                    promptPanel.SetActive(true);

                    codexInstructionBubble.SetActive(true);
                    codexInstructionText.text = codexInstruction;

                    satchelInstructionBubble.SetActive(true);
                    satchelInstructionText.text = satchelInstruction;

                    secondInstructionBubble.SetActive(true);
                    secondInstructionText.text = secondTabInstruction;

                    currentStage = Stage.BackpackOpen;
                    break;

                case Stage.BackpackOpen:
                    if (BackpackSystemManager.Instance != null)
                        BackpackSystemManager.Instance.CloseBackpack();

                    codexInstructionBubble.SetActive(false);
                    satchelInstructionBubble.SetActive(false);
                    secondInstructionBubble.SetActive(false);
                    promptPanel.SetActive(false);

                    currentStage = Stage.MentorIntro;
                    ShowNextDialogue();
                    break;
            }
        }
    }

    public void ShowNextDialogue()
    {
        if (currentStage == Stage.Intro)
        {
            if (herbIntroIndex < herbIntroLines.Length)
            {
                introDialogueText.text = herbIntroLines[herbIntroIndex++];
            }
            else
            {
                introDialogueBubble.SetActive(false);
                instructionBubble.SetActive(true);
                instructionText.text = tabInstructionLine;
                introNextButton.gameObject.SetActive(false);
                if (portrait != null) portrait.SetActive(false);
                currentStage = Stage.WaitForTab;
            }
        }
        else if (currentStage == Stage.PatientDialogue)
        {
            if (patientDialogueIndex < patientDialogue.Length)
            {
                patientDialogueText.text = patientDialogue[patientDialogueIndex++];
            }
            else
            {
                EndPatientDialogue();
            }
        }
        else if (currentStage == Stage.MentorIntro)
        {
            if (mentorIntroStage == 0)
            {
                gatherIntroBubble1.SetActive(true);
                gatherIntroText1.text = mentorIntroLine1;
                if (portrait != null) portrait.SetActive(true);
                mentorIntroStage++;
            }
            else if (mentorIntroStage == 1)
            {
                gatherIntroBubble1.SetActive(false);
                gatherIntroBubble2.SetActive(true);
                gatherIntroText2.text = mentorIntroLine2;
                if (portrait != null) portrait.SetActive(true);
                mentorIntroStage++;
            }
            else if (mentorIntroStage == 2)
            {
                gatherIntroBubble2.SetActive(false);
                if (portrait != null) portrait.SetActive(false);
                currentStage = Stage.PatientDialogue;
                patientDialogueBubble.SetActive(true);
                patientDialogueText.text = patientDialogue[patientDialogueIndex++];
            }
        }
    }

    public void OnBackpackClosedByButton()
    {
        if (currentStage == Stage.BackpackOpen)
        {
            codexInstructionBubble.SetActive(false);
            satchelInstructionBubble.SetActive(false);
            secondInstructionBubble.SetActive(false);
            promptPanel.SetActive(false);

            currentStage = Stage.MentorIntro;
            ShowNextDialogue();
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
