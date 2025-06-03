using UnityEngine;
using System.Collections;



public class BackpackSystemManager : MonoBehaviour
{
    public static BackpackSystemManager Instance;

    [Header("Prefab Reference")]
    public GameObject backpackUIPrefab;  // Assign this in Inspector (your BackpackUI prefab)

    private GameObject backpackInstance;
    private GameObject parentUI;
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

        // 尝试从实例中递归查找 ParentUI
        if (backpackInstance != null)
        {
            Transform found = backpackInstance.transform.Find("ParentUI");
            if (found == null)
            {
                foreach (Transform child in backpackInstance.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "ParentUI")
                    {
                        found = child;
                        break;
                    }
                }
            }

            if (found != null)
            {
                parentUI = found.gameObject;
                Debug.Log("✅ ParentUI found and cached.");
            }
            else
            {
                Debug.LogWarning("❌ Failed to find ParentUI in backpack prefab.");
            }
        }

    }

    public void OpenBackpack()
    {
        gameObject.SetActive(true);
        StartCoroutine(DelayedOpenUI());
    }

    private IEnumerator DelayedOpenUI()
    {
        yield return null; // 等待 1 帧，确保 UI 元素已激活 & 渲染链稳定

        if (parentUI != null)
        {
            parentUI.SetActive(true);
            Debug.Log("🎒 Backpack UI actually activated.");
        }

        // 刷新显示内容（可选加一帧）
        PlayerInventory playerInv = FindObjectOfType<PlayerInventory>();
        if (playerInv != null)
        {
            playerInv.RefreshUI();
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