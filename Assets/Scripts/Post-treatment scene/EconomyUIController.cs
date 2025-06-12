using UnityEngine;
using TMPro;

public class EconomyUIController : MonoBehaviour
{
    public GameObject frameGroup; // 🎯 你用来显示Gold+Supply的Frame整体

    public TMP_Text goldText;

    private bool isVisible = false;

    public void ShowHUD()
    {
        if (frameGroup != null)
            frameGroup.SetActive(true);

        isVisible = true;
    }

    void Start()
    {
        if (frameGroup != null)
            frameGroup.SetActive(false);
    }

    void Update()
    {
        if (!isVisible || GameStateManager.Instance == null) return;

        if (goldText != null)
            goldText.text = GameStateManager.Instance.gold.ToString();
    }
}
