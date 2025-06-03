using UnityEngine;

public class ClinicBackpackTrigger : MonoBehaviour
{
    public GameObject backpackSystemPrefab;

    void Start()
    {
        // 延迟一点初始化背包系统
        Invoke(nameof(InitializeAndShowBackpack), 0.1f);
    }

    void InitializeAndShowBackpack()
    {
        if (BackpackSystemManager.Instance == null)
        {
            GameObject backpack = Instantiate(backpackSystemPrefab);
            backpack.name = "BackpackSystemManager";
            Debug.Log("🧪 Instantiated Backpack prefab.");
        }

        if (BackpackSystemManager.Instance != null)
        {
            BackpackSystemManager.Instance.OpenBackpack();
            Debug.Log("🎒 Backpack opened.");
        }

        // ✅ 禁用自身防止后续执行
        this.enabled = false;
    }
}
