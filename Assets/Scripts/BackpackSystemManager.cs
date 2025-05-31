using UnityEngine;

public class BackpackSystemManager : MonoBehaviour
{
    public static BackpackSystemManager Instance;

    [Header("Prefab Reference")]
    public GameObject backpackUIPrefab;  // Assign this in Inspector (your BackpackUI prefab)

    private GameObject backpackInstance;

    void Awake()
    {
        // Singleton pattern — ensure only one instance exists across scenes
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Persist across scenes
    }

    void Start()
    {
        if (backpackUIPrefab != null && backpackInstance == null)
        {
            backpackInstance = Instantiate(backpackUIPrefab);
            backpackInstance.name = "BackpackUI (Runtime)";
            backpackInstance.SetActive(false);  // Start hidden
            DontDestroyOnLoad(backpackInstance);

            Debug.Log("✅ Backpack UI instantiated successfully.");
        }
        else
        {
            Debug.LogWarning("⚠️ Backpack UI instantiation skipped (already exists or prefab is null).");
        }
    }

    public void OpenBackpack()
    {
        if (backpackInstance != null)
        {
            Debug.Log("🎒 Opening Backpack UI");
            backpackInstance.SetActive(true);

            PlayerInventory inv = backpackInstance.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.RefreshUI(); // Optional: reload slots
            }
            else
            {
                Debug.LogWarning("⚠️ No PlayerInventory component found on backpackInstance.");
            }
        }
        else
        {
            Debug.LogWarning("❌ backpackInstance is null. Cannot open backpack.");
        }
    }

    public void CloseBackpack()
    {
        if (backpackInstance != null)
        {
            backpackInstance.SetActive(false);
        }
    }

    public bool IsBackpackOpen()
    {
        return backpackInstance != null && backpackInstance.activeSelf;
    }
}
