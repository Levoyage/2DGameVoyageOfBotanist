using UnityEngine;
using TMPro;

public class EconomyUIController : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text supplyText;

    private bool isVisible = false;

    public void ShowHUD()
    {
        goldText.gameObject.SetActive(true);
        supplyText.gameObject.SetActive(true);
        isVisible = true;
    }

    void Start()
    {
        goldText.gameObject.SetActive(false);
        supplyText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isVisible || GameStateManager.Instance == null) return;

        goldText.text = "Gold: " + GameStateManager.Instance.gold;
        supplyText.text = "Supply: " + GameStateManager.Instance.supplyPacks;
    }
}
