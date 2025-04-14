using UnityEngine;
using System.Collections;

public class GatherPromptManager : MonoBehaviour
{
    public static GatherPromptManager Instance;

    [Header("UI Prompt Object")]
    public GameObject gatherPromptUI;
    private CanvasGroup canvasGroup;

    private Coroutine currentFade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup = gatherPromptUI.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            gatherPromptUI.SetActive(false);
        }
    }

    public void ShowAt(Vector3 worldPosition)
    {
        if (gatherPromptUI == null || canvasGroup == null) return;

        gatherPromptUI.SetActive(true);
        gatherPromptUI.transform.position = Camera.main.WorldToScreenPoint(worldPosition);

        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, 0.3f)); // 淡入
    }

    public void Hide()
    {
        if (canvasGroup == null) return;

        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeAndDeactivate(canvasGroup, 1f, 0f, 0.3f)); // 淡出
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float time = 0f;
        cg.alpha = from;

        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        cg.alpha = to;
    }

    private IEnumerator FadeAndDeactivate(CanvasGroup cg, float from, float to, float duration)
    {
        yield return FadeCanvasGroup(cg, from, to, duration);
        gatherPromptUI.SetActive(false);
    }
}
