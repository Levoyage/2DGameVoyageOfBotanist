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
    public Button introBackButton;

    [Header("Patient Dialogue")]
    public GameObject patientDialogueBubble;
    public TextMeshProUGUI patientDialogueText;
    public Button patientNextButton;
    public Button nextPatientButton;


    [Header("Mentor Dialogue (Diagnosis Prompt)")]
    public GameObject mentorDialogueBubble;
    public TextMeshProUGUI mentorDialogueText;

    [Header("Diagnosis System")]
    public GameObject diagnosisPanel;
    public Button arrhythmiaButton;
    public Button fluButton;
    public Button heatstrokeButton;
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

    [Header("Mentor Intro Bubbles")]
    public GameObject gatherIntroBubble1;
    public TextMeshProUGUI gatherIntroText1;
    public GameObject gatherIntroBubble2;
    public TextMeshProUGUI gatherIntroText2;
    public Button gatherBackButton2;

    [Header("Character Portraits")]
    public GameObject portrait;
    public GameObject patientPortrait;
    public GameObject patientPortrait2;

    private string[] herbIntroLines = {
        "From now on, you'll carry your own satchel for herbs. Keep it tidy.",
        "We've also compiled an encyclopedia for the plants you find.",
        "Check it often — knowledge saves lives."
    };

    private string codexInstruction = "This is your encyclopedia. It shall preserve records of the plants you collect. <color=red><b>Click >> to open it.</b></color>";
    private string satchelInstruction = "This is your satchel. It contains only the plants you have gathered correctly.";
    private string tabInstructionLine = "Press Tab to open your encyclopedia and satchel.";
    private string secondTabInstruction = "Press <color=red><b>Tab</b></color> again or click <b><color=#9C7B54>'x'</color></b> to proceed →";

    private string[] patientDialogue = {
        "My heart races, and I feel faint just walking.",
        "It started with fatigue, but now my pulse pounds like thunder."
    };

    private string[] patient2Dialogue = {
        "Nausea grips me every morning, I can't keep food down.",
        "It feels like the world spins when I try to eat."
    };

    private string[] patient2DiagnosisOptions = { "Nausea", "Migraine", "Food Poisoning" };

    private string mentorIntroLine1 = "You've done well with simple cases. But this time, two patients await. Diagnose carefully and gather the herbs yourself.";
    private string mentorIntroLine2 = "If you fail to collect the herbs, the <color=red><b>supply pack</b></color> may suffice — but now we have only <color=red><b>one</b></color>. Choose wisely, or their fates may be sealed.";
    private string mentorQuestion = "What is your diagnosis, apprentice?";

    private int herbIntroIndex = 0;
    private int patientDialogueIndex = 0;
    private int patient2DialogueIndex = 0;
    private int mentorIntroStage = 0;
    private bool isSecondPatient = false;

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
        if (patientPortrait != null) patientPortrait.SetActive(true);
        if (patientPortrait2 != null) patientPortrait2.SetActive(false);

        introDialogueText.text = herbIntroLines[herbIntroIndex++];
        introNextButton.onClick.RemoveAllListeners();
        introNextButton.onClick.AddListener(ShowNextDialogue);
        introBackButton.onClick.RemoveAllListeners();
        introBackButton.onClick.AddListener(ShowPreviousIntroDialogue);
        introBackButton.gameObject.SetActive(false);

        patientNextButton.onClick.RemoveAllListeners();
        patientNextButton.onClick.AddListener(ShowNextDialogue);

        arrhythmiaButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heart Arrhythmia";
        fluButton.GetComponentInChildren<TextMeshProUGUI>().text = "Flu";
        heatstrokeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heatstroke";

        arrhythmiaButton.onClick.AddListener(() => CheckDiagnosis("Heart Arrhythmia"));
        fluButton.onClick.AddListener(() => CheckDiagnosis("Flu"));
        heatstrokeButton.onClick.AddListener(() => CheckDiagnosis("Heatstroke"));

        gatherBackButton2.onClick.RemoveAllListeners();
        gatherBackButton2.onClick.AddListener(ShowPreviousGatherIntro);
        gatherBackButton2.gameObject.SetActive(false);

        nextPatientButton.onClick.RemoveAllListeners();
        nextPatientButton.onClick.AddListener(ShowSecondPatient);
        nextPatientButton.gameObject.SetActive(false);
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
                if (herbIntroIndex >= 2)
                    introBackButton.gameObject.SetActive(true);
            }
            else
            {
                introDialogueBubble.SetActive(false);
                instructionBubble.SetActive(true);
                instructionText.text = tabInstructionLine;
                introNextButton.gameObject.SetActive(false);
                introBackButton.gameObject.SetActive(false);
                if (portrait != null) portrait.SetActive(false);
                currentStage = Stage.WaitForTab;
            }
        }
        else if (currentStage == Stage.PatientDialogue)
        {
            if (!isSecondPatient)
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
            else
            {
                if (patient2DialogueIndex < patient2Dialogue.Length)
                {
                    patientDialogueText.text = patient2Dialogue[patient2DialogueIndex++];
                }
                else
                {
                    EndPatientDialogue();
                }
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
                gatherBackButton2.gameObject.SetActive(false);
            }
            else if (mentorIntroStage == 1)
            {
                gatherIntroBubble1.SetActive(false);
                gatherIntroBubble2.SetActive(true);
                gatherIntroText2.text = mentorIntroLine2;
                if (portrait != null) portrait.SetActive(true);
                mentorIntroStage++;
                gatherBackButton2.gameObject.SetActive(true);
            }
            else if (mentorIntroStage == 2)
            {
                gatherIntroBubble2.SetActive(false);
                gatherBackButton2.gameObject.SetActive(false);
                if (portrait != null) portrait.SetActive(false);
                currentStage = Stage.PatientDialogue;
                patientDialogueBubble.SetActive(true);
                patientDialogueText.text = patientDialogue[patientDialogueIndex++];
            }
        }
    }

    public void ShowPreviousIntroDialogue()
    {
        if (herbIntroIndex > 1)
        {
            herbIntroIndex--;
            introDialogueText.text = herbIntroLines[herbIntroIndex - 1];

            if (herbIntroIndex == 1)
                introBackButton.gameObject.SetActive(false);
            else
                introBackButton.gameObject.SetActive(true);
        }
    }

    public void ShowPreviousGatherIntro()
    {
        if (mentorIntroStage == 2)
        {
            mentorIntroStage = 1;
            gatherIntroBubble2.SetActive(false);
            gatherIntroBubble1.SetActive(true);
            gatherIntroText1.text = mentorIntroLine1;
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

        if (!isSecondPatient)
        {
            arrhythmiaButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heart Arrhythmia";
            fluButton.GetComponentInChildren<TextMeshProUGUI>().text = "Flu";
            heatstrokeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heatstroke";

            arrhythmiaButton.onClick.RemoveAllListeners();
            fluButton.onClick.RemoveAllListeners();
            heatstrokeButton.onClick.RemoveAllListeners();

            arrhythmiaButton.onClick.AddListener(() => CheckDiagnosis("Heart Arrhythmia"));
            fluButton.onClick.AddListener(() => CheckDiagnosis("Flu"));
            heatstrokeButton.onClick.AddListener(() => CheckDiagnosis("Heatstroke"));
        }
        else
        {
            arrhythmiaButton.GetComponentInChildren<TextMeshProUGUI>().text = "Nausea";
            fluButton.GetComponentInChildren<TextMeshProUGUI>().text = "Migraine";
            heatstrokeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Food Poisoning";

            arrhythmiaButton.onClick.RemoveAllListeners();
            fluButton.onClick.RemoveAllListeners();
            heatstrokeButton.onClick.RemoveAllListeners();

            arrhythmiaButton.onClick.AddListener(() => CheckDiagnosis("Nausea"));
            fluButton.onClick.AddListener(() => CheckDiagnosis("Migraine"));
            heatstrokeButton.onClick.AddListener(() => CheckDiagnosis("Food Poisoning"));
        }
        currentStage = Stage.Diagnosis;
    }

    void CheckDiagnosis(string choice)
    {
        mentorDialogueBubble.SetActive(false);
        selectedDiagnosis = choice;
        string feedback = "";
        bool isCorrect = false;
        if (!isSecondPatient)
        {
            switch (choice)
            {
                case "Heart Arrhythmia":
                    feedback = "Correct. The patient's symptoms point to a disrupted heart rhythm. <color=green><b>Foxglove</b></color>-based tincture is the preferred treatment.";
                    isCorrect = true;
                    break;
                case "Flu":
                    feedback = "No fever or congestion is present. This doesn't match the profile of influenza.";
                    break;
                case "Heatstroke":
                    feedback = "The patient wasn't exposed to heat, and there are no signs of overheating or confusion.";
                    break;
                default:
                    feedback = "Hmm... not quite. Review the symptoms again.";
                    break;
            }
            doctorFeedbackText.text = feedback;
            doctorFeedbackText.gameObject.SetActive(true);
            if (isCorrect)
            {
                nextPatientButton.gameObject.SetActive(true);
                gatherButton.SetActive(false);
            }
            else
            {
                nextPatientButton.gameObject.SetActive(false);
                gatherButton.SetActive(false);
            }
        }
        else
        {
            switch (choice)
            {
                case "Nausea":
                    feedback = "Correct. The symptoms indicate persistent nausea. <color=green><b>Ginger</b></color> root is often used as a remedy.";
                    isCorrect = true;
                    break;
                case "Migraine":
                    feedback = "No mention of headache or light sensitivity. This doesn't match migraine.";
                    break;
                case "Food Poisoning":
                    feedback = "No acute pain or fever. Food poisoning is unlikely.";
                    break;
                default:
                    feedback = "Hmm... not quite. Review the symptoms again.";
                    break;
            }
            doctorFeedbackText.text = feedback;
            doctorFeedbackText.gameObject.SetActive(true);
            if (isCorrect)
            {
                gatherButton.SetActive(true);
                nextPatientButton.gameObject.SetActive(false);
            }
            else
            {
                gatherButton.SetActive(false);
                nextPatientButton.gameObject.SetActive(false);
            }
        }
    }

    public void ShowSecondPatient()
    {
        isSecondPatient = true;
        patient2DialogueIndex = 0;
        mentorDialogueBubble.SetActive(false);
        diagnosisPanel.SetActive(false);
        doctorFeedbackText.gameObject.SetActive(false);
        nextPatientButton.gameObject.SetActive(false);
        if (patientPortrait != null) patientPortrait.SetActive(false);
        if (patientPortrait2 != null) patientPortrait2.SetActive(true);
        if (portrait != null) portrait.SetActive(false);
        patientDialogueBubble.SetActive(true);
        patientDialogueText.text = patient2Dialogue[patient2DialogueIndex++];
        currentStage = Stage.PatientDialogue;
    }

    public void GoToFieldScene()
    {
        SceneManager.LoadScene("FieldScene");
    }
}
