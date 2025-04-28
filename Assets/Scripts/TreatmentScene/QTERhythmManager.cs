using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QTERhythmManager : MonoBehaviour
{
    public List<GameObject> prompts;
    public List<KeyCode> keySequence = new List<KeyCode>
    {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.RightArrow,
        KeyCode.DownArrow,
        KeyCode.Return
    };

    public float totalQTETime = 3f;
    private int currentPromptIndex = 0;
    private bool qteActive = false;
    private float timer = 0f;

    public Slider progressBar;
    public System.Action onQTESuccess;
    public System.Action onQTEFail;

    private bool[] promptSuccess;

    void Start()
    {
        HideAllPrompts();
    }

    public void StartQTE()
    {
        currentPromptIndex = 0;
        timer = 0f;
        qteActive = true;

        promptSuccess = new bool[prompts.Count]; // initialize the array based on the number of prompts
        for (int i = 0; i < promptSuccess.Length; i++)
            promptSuccess[i] = false;

        foreach (var prompt in prompts)
            prompt.SetActive(true);

        if (progressBar != null)
            progressBar.value = 0f;

        Debug.Log("[QTE] Started rhythm QTE");
    }

    void Update()
    {
        if (!qteActive) return;

        timer += Time.deltaTime;
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01(timer / totalQTETime);

        if (Input.anyKeyDown && currentPromptIndex < keySequence.Count)
        {
            if (Input.GetKeyDown(keySequence[currentPromptIndex]))
            {
                Debug.Log($"[QTE] Correct key {keySequence[currentPromptIndex]}");
                MarkPromptSuccess(currentPromptIndex);
                currentPromptIndex++;

                if (currentPromptIndex >= keySequence.Count)
                {
                    EndQTE(true);
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[QTE] Wrong key pressed!");
                EndQTE(false);
                return;
            }
        }

        HighlightPrompt(currentPromptIndex);

        if (timer >= totalQTETime)
        {
            if (currentPromptIndex >= keySequence.Count)
                EndQTE(true);
            else
                EndQTE(false);
        }
    }

    void HighlightPrompt(int index)
    {
        for (int i = 0; i < prompts.Count; i++)
        {
            Image img = prompts[i].GetComponent<Image>();

            if (promptSuccess[i])
            {
                // if the prompt was already successful, keep it green
                img.color = Color.green;
            }
            else
            {
                // if the prompt is the current one, highlight it
                img.color = (i == index) ? Color.yellow : Color.white;
            }
        }
    }

    void MarkPromptSuccess(int index)
    {
        if (index >= 0 && index < prompts.Count && prompts[index] != null)
        {
            Image img = prompts[index].GetComponent<Image>();
            if (img != null)
                img.color = Color.green;

            promptSuccess[index] = true; //  mark this prompt as successful
        }
    }

    void HideAllPrompts()
    {
        foreach (var p in prompts)
        {
            if (p != null)
                p.SetActive(false);
        }
    }

    void EndQTE(bool success)
    {
        qteActive = false;

        foreach (var p in prompts)
        {
            if (p != null)
            {
                Image img = p.GetComponent<Image>();
                if (img != null) img.color = Color.white;
                p.SetActive(false);
            }
        }

        if (progressBar != null)
            progressBar.value = 0f;

        Debug.Log(success ? "[QTE] SUCCESS" : "[QTE] FAIL");

        if (success)
            onQTESuccess?.Invoke();
        else
            onQTEFail?.Invoke();
    }

    public void ResetQTE()
    {
        Debug.Log("[QTE] Resetting QTE");
        HideAllPrompts();
        if (progressBar != null)
            progressBar.value = 0f;
        qteActive = false;
        currentPromptIndex = 0;
        timer = 0f;
    }
}
