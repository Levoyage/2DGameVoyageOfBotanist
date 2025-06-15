using UnityEngine;

public class PersistentPlayerInventory : MonoBehaviour
{
    public PlayerInventory inventory;

    void Awake()
    {
        if (inventory == null)
        {
            inventory = FindObjectOfType<PlayerInventory>();
        }

        if (inventory != null)
        {
            DontDestroyOnLoad(inventory.gameObject);
            Debug.Log("📌 Persisting PlayerInventory: " + inventory.name);
        }
        else
        {
            Debug.LogError("❌ Could not find PlayerInventory to persist.");
        }

        DontDestroyOnLoad(gameObject); // 让自己也不被销毁
    }
}
