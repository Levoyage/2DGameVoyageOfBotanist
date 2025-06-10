using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PatientManager : MonoBehaviour
{
    [Header("Patient Dialogue")]
    public GameObject patientDialogueBubble;
    public TextMeshProUGUI patientDialogueText;
    public Button patientNextButton;

    [Header("Mentor Dialogue")]
    public GameObject mentorDialogueBubble;
    public TextMeshProUGUI mentorDialogueText;

    [Header("Diagnosis System")]
    public GameObject diagnosisPanel;
    public Button eczemaButton, woundInfectionButton, scurvyButton;
    public TextMeshProUGUI doctorFeedbackText;
    public GameObject gatherButton;

    private string[] patientDialogue = { "My skin is red and itchy.", "These rashes won’t go away, and they burn." };
    private string mentorQuestion = "What is your diagnosis, apprentice?";

    private int patientDialogueIndex = 0;

    void Start()
    {
        // 初始化界面状态
        patientDialogueBubble.SetActive(true);
        mentorDialogueBubble.SetActive(false);
        diagnosisPanel.SetActive(false);
        doctorFeedbackText.gameObject.SetActive(false);
        gatherButton.SetActive(false);


        // **初始化第一句对话，并手动递增索引，避免重复**
        patientDialogueText.text = patientDialogue[patientDialogueIndex];
        patientDialogueIndex++;



        // 绑定诊断选项按钮
        eczemaButton.onClick.AddListener(() => CheckDiagnosis("Eczema"));
        woundInfectionButton.onClick.AddListener(() => CheckDiagnosis("Wound Infection"));
        scurvyButton.onClick.AddListener(() => CheckDiagnosis("Scurvy"));
    }

    public void ShowNextPatientDialogue()
    {
        if (patientDialogueIndex < patientDialogue.Length)
        {
            patientDialogueText.text = patientDialogue[patientDialogueIndex];
            patientDialogueIndex++;
        }
        else
        {
            EndPatientDialogue();
        }
    }

    void EndPatientDialogue()
    {
        patientDialogueBubble.SetActive(false);

        // 显示导师提问 + 诊断界面
        mentorDialogueBubble.SetActive(true);
        mentorDialogueText.text = mentorQuestion;
        diagnosisPanel.SetActive(true);
    }

    void CheckDiagnosis(string choice)
    {
        // 隐藏导师对话
        mentorDialogueBubble.SetActive(false);

        if (choice == "Eczema")
        {
            doctorFeedbackText.text = "Correct! Red, itchy rashes and burning skin suggest eczema. The best remedy? A soothing herbal balm with <color=green><b>pimpernel</b></color>.";
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

    // **🌿 进入采药场景**
    public void GoToFieldScene()
    {
        SceneManager.LoadScene("FieldScene");
    }
}