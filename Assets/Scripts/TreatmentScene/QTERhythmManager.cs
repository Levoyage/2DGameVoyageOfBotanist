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
            KeyCode.LeftArrow,
            KeyCode.DownArrow,
            KeyCode.LeftShift,
            KeyCode.Return
        };


    public float totalQTETime = 8f;
    private int currentPromptIndex = 0;
    private bool qteActive = false;
    private float timer = 0f;

    public Slider progressBar;
    public System.Action onQTESuccess;
    public System.Action onQTEFail;

    private bool[] promptSuccess;

    [Header("Audio")]
    public AudioClip keyHitSound;   // ✅ 单次正确按键
    public AudioClip successSound;  // ✅ 成功完成QTE
    public AudioClip failSound;     // ✅ 失败（按错/超时）
    private AudioSource audioSource;

    void Start()
    {
        HideAllPrompts();

        // 自动添加 AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void StartQTE()
    {
        currentPromptIndex = 0;
        timer = 0f;
        qteActive = true;

        promptSuccess = new bool[prompts.Count];
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

                // ✅ 播放按键音
                PlaySound(keyHitSound);

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
                img.color = Color.green;
            else
                img.color = (i == index) ? Color.yellow : Color.white;
        }
    }

    void MarkPromptSuccess(int index)
    {
        if (index >= 0 && index < prompts.Count && prompts[index] != null)
        {
            Image img = prompts[index].GetComponent<Image>();
            if (img != null)
                img.color = Color.green;

            promptSuccess[index] = true;
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

        // ✅ 播放成功或失败的音效
        if (success)
        {
            PlaySound(successSound);
            onQTESuccess?.Invoke();
        }
        else
        {
            PlaySound(failSound);
            onQTEFail?.Invoke();
        }
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

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 1f);
        }
    }

}
