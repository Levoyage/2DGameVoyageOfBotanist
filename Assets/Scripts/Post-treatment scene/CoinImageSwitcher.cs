using UnityEngine;
using UnityEngine.UI;

public class CoinImageSwitcher : MonoBehaviour
{
    public Sprite coinIdle;
    public Sprite coinFlash;

    public float switchInterval = 0.3f;

    private Image image;
    private float timer;
    private bool showingFlash = false;

    void Start()
    {
        image = GetComponent<Image>();
        timer = switchInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            showingFlash = !showingFlash;
            image.sprite = showingFlash ? coinFlash : coinIdle;
            timer = switchInterval;
        }
    }
}
