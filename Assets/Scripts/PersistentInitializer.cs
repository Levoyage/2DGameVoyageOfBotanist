using UnityEngine;

public class PersistentInitializer : MonoBehaviour
{
    void Awake()
    {
        if (GameObject.Find("PersistentSystems") == null)
        {
            GameObject persistent = Instantiate(Resources.Load<GameObject>("PersistentSystems"));
            persistent.name = "PersistentSystems"; // 确保名称一致，防止多次加载
            Debug.Log("✅ PersistentSystems loaded.");
        }
        else
        {
            Debug.Log("ℹ️ PersistentSystems already exists.");
        }
    }
}
