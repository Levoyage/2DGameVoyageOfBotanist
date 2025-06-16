using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// QTE rhythm controller for **fixed 6‑key sequences** (↑ ↓ ← → only).
/// Attach one instance to each *QTEpanel* in **TreatmentScene‑1**.
/// - In Inspector drag 6 prompt images into <b>Prompts</b> (Prompt1‑Prompt6)
/// - Drag the matching 6‑key <b>Key Sequence</b> list (size = 6)
/// - Success / Fail callbacks wired via<br>   <code>onQTESuccess / onQTEFail</code>
/// </summary>
public class QTERhythmManager6 : MonoBehaviour
{
    [Header("UI Prompts (6 images)")]
    public List<Image> prompts;                 // Prompt1‑Prompt6 (fixed)

    [Header("Fixed 6‑Key Sequence")]
    public List<KeyCode> keySequence = new()    // Configure per panel in Inspector
    {
        KeyCode.UpArrow,
        KeyCode.RightArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow
    };

    [Header("Progress Bar (optional)")]
    public Slider progressBar;                  // Fill bar; optional
    public float totalQTETime = 8f;             // Seconds allowed

    public System.Action onQTESuccess;          // Assign in TreatmentManager1
    public System.Action onQTEFail;

    [Header("Sounds (optional)")]
    public AudioClip hitSound;
    public AudioClip successSound;
    public AudioClip failSound;

    private AudioSource _audio;
    private int _index;                         // current position in sequence
    private float _timer;
    private bool _active;

    void Awake()
    {
        _audio = gameObject.AddComponent<AudioSource>();
        HidePrompts();
    }

    // -------- public API ----------------------------------------------------

    /// <summary>Call from TreatmentManager1 when player chooses boil/grind.</summary>
    public void StartQTE()
    {
        if (keySequence.Count != 6 || prompts.Count < 6)
        {
            Debug.LogError("[QTE‑6] Need exactly 6 prompts + 6 key codes!");
            return;
        }

        _index = 0;
        _timer = 0f;
        _active = true;
        ShowPrompts();
        UpdatePromptHighlight();
        progressBar?.SetValueWithoutNotify(0f);
        Debug.Log("[QTE‑6] Started with sequence: " + string.Join(",", keySequence));
    }

    /// <summary>Reset after success / fail to prepare next herb.</summary>
    public void ResetQTE()
    {
        _active = false;
        _index = 0;
        _timer = 0f;
        HidePrompts();
        progressBar?.SetValueWithoutNotify(0f);
    }

    // -------- runtime -------------------------------------------------------

    void Update()
    {
        if (!_active) return;

        _timer += Time.deltaTime;
        progressBar?.SetValueWithoutNotify(Mathf.Clamp01(_timer / totalQTETime));

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(keySequence[_index]))
            {
                Play(hitSound);
                prompts[_index].color = Color.green; // mark correct
                _index++;
                if (_index >= keySequence.Count)
                    Finish(true);
                else
                    UpdatePromptHighlight();
            }
            else
            {
                Finish(false);
            }
        }

        if (_timer >= totalQTETime)
            Finish(false);
    }

    // -------- helpers -------------------------------------------------------

    void Finish(bool success)
    {
        _active = false;
        Play(success ? successSound : failSound);
        HidePrompts();
        progressBar?.SetValueWithoutNotify(0f);
        if (success) onQTESuccess?.Invoke(); else onQTEFail?.Invoke();
    }

    void UpdatePromptHighlight()
    {
        for (int i = 0; i < prompts.Count; i++)
            prompts[i].color = (i == _index) ? Color.yellow : Color.white;
    }

    void ShowPrompts()
    {
        foreach (var p in prompts) p.gameObject.SetActive(true);
    }

    void HidePrompts()
    {
        foreach (var p in prompts) p.gameObject.SetActive(false);
    }

    void Play(AudioClip clip)
    {
        if (clip) _audio.PlayOneShot(clip);
    }
}
