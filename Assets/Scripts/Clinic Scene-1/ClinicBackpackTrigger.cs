using UnityEngine;

public class ClinicBackpackTrigger : MonoBehaviour
{
    void Start()
    {
        // 延迟一点，等场景内容加载完成
        Invoke(nameof(OpenBackpackOnce), 0.2f);
    }

    void OpenBackpackOnce()
    {
        if (BackpackSystemManager.Instance != null)
        {
            BackpackSystemManager.Instance.OpenBackpack();
            Debug.Log("🎒 ClinicScene: Backpack opened.");
        }
        else
        {
            Debug.LogWarning("❌ BackpackSystemManager not found in ClinicScene.");
        }

        // 禁用自身，确保只触发一次
        enabled = false;
    }
}
