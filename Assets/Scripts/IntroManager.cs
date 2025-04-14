using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI introText;  // The text component
    public GameObject textBackground;  // Background panel for better readability
    public GameObject nextButton;      // The Next button UI
    public CanvasGroup canvasGroup;    // For fade effect

    private string[] introLines = {
        "Provence, France, late 18th century...",
        "You have arrived at the small countryside clinic.",
        "Dr. Madeleine Fournier welcomes you inside.",
        "Your training as an apprentice physician begins today.",
        "Now, a patient comes..."
    };

    private int currentLine = 0;

    void Start()
    {
        // Ensure text is visible at start
        introText.text = introLines[currentLine];
        textBackground.SetActive(true);
        nextButton.SetActive(true);

        // Ensure the panel starts fully visible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
        }
    }

    public void NextDialogue()
    {
        currentLine++;

        if (currentLine < introLines.Length)
        {
            StartCoroutine(FadeText(introLines[currentLine]));
        }
        else
        {
            // Transition to the clinic scene when finished
            SceneManager.LoadScene("ClinicScene");
        }
    }

IEnumerator FadeText(string newText, float fadeSpeed = 5f) // 添加可调节参数
{
    // Fade out
    for (float t = 1; t > 0; t -= Time.deltaTime * fadeSpeed)
    {
        introText.alpha = t;
        yield return null;
    }

    // Change text
    introText.text = newText;

    // Fade in
    for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
    {
        introText.alpha = t;
        yield return null;
    }
}

}
