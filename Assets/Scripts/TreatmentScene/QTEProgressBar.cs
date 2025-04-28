using UnityEngine;
using UnityEngine.UI;

public class QTEProgressBar : MonoBehaviour
{
    public Image fillImage;          // The Fill child of the bar
    public float totalTime = 10f;    // Total countdown time in seconds
    private float timeLeft;
    private bool isRunning = false;

    public System.Action onTimeOut; // Optional: callback when time ends

    void Start()
    {
        // Optional: Start automatically
        // StartCountdown();
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;
        float ratio = Mathf.Clamp01(timeLeft / totalTime);
        fillImage.fillAmount = ratio;

        if (timeLeft <= 0f)
        {
            isRunning = false;
            onTimeOut?.Invoke();
        }
    }

    public void StartCountdown(float customTime = -1f)
    {
        timeLeft = (customTime > 0) ? customTime : totalTime;
        isRunning = true;
        fillImage.fillAmount = 0f;
    }

    public void StopCountdown()
    {
        isRunning = false;
    }

    public bool IsRunning()
    {
        return isRunning;
    }
}
