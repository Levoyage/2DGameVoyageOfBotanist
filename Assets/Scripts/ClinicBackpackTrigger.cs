using UnityEngine;

public class ClinicBackpackTrigger : MonoBehaviour
{
    private bool canPressTab = false;
    private bool backpackOpened = false;

    void Start()
    {
        // 启动后延迟允许按 Tab，防止误触
        Invoke(nameof(EnableTabTrigger), 0.2f);
    }

    void EnableTabTrigger()
    {
        canPressTab = true;
    }

    void Update()
    {
        if (canPressTab && !backpackOpened && Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("🎒 Tab pressed in ClinicScene-1 — opening backpack");

            if (BackpackSystemManager.Instance != null)
            {
                BackpackSystemManager.Instance.OpenBackpack();
                backpackOpened = true; // 只允许打开一次
            }
            else
            {
                Debug.LogWarning("❌ BackpackSystemManager is missing in ClinicScene-1!");
            }
        }
    }
}
