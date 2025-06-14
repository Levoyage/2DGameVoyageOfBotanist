using UnityEngine;

public class PersistentPlayerInventory : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectsOfType<PersistentPlayerInventory>().Length > 1)
        {
            Destroy(gameObject); // 防止重复
            return;
        }

        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<PlayerInventory>(); // ✅ 关键：确保 PlayerInventory 永远存在
    }

}
