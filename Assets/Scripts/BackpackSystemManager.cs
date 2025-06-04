using UnityEngine;
using System.Collections;

public class BackpackSystemManager : MonoBehaviour
{
    public static BackpackSystemManager Instance;

    [Header("Prefab Reference")]
    public GameObject backpackUIPrefab;

    private GameObject backpackInstance;
    private GameObject parentUI;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeIfNeeded();
    }

    /// <summary>
    /// 确保初始化 Backpack UI 和绑定 PlayerInventory
    /// </summary>
    public void InitializeIfNeeded()
    {
        if (backpackInstance != null) return;

        if (backpackUIPrefab != null)
        {
            backpackInstance = Instantiate(backpackUIPrefab);
            backpackInstance.name = "BackpackUI (Runtime)";
            backpackInstance.SetActive(false);
            DontDestroyOnLoad(backpackInstance);

            Debug.Log("✅ Backpack UI instantiated.");

            // 查找 ParentUI
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
                Debug.LogWarning("❌ Could not find 'ParentUI' in backpack prefab.");
            }

            // 延迟注册 UI 给 PlayerInventory
            StartCoroutine(DelayedRegisterUIToPlayerInventory());
        }
        else
        {
            Debug.LogWarning("⚠️ backpackUIPrefab is not assigned.");
        }
    }

    private IEnumerator DelayedRegisterUIToPlayerInventory()
    {
        yield return new WaitForSeconds(0.1f); // 等待 PlayerInventory 完成 Start()

        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv != null && backpackInstance != null)
        {
            BackPackUI ui = backpackInstance.GetComponent<BackPackUI>();
            if (ui != null)
            {
                inv.SetBackpackUI(ui);
                Debug.Log("📦 Backpack UI registered to PlayerInventory.");
            }
            else
            {
                Debug.LogWarning("❌ backpackInstance 没有 BackPackUI 组件！");
            }
        }
        else
        {
            Debug.LogWarning("❌ PlayerInventory 或 backpackInstance 丢失，无法注册 UI。");
        }
    }

    public void OpenBackpack()
    {
        InitializeIfNeeded();
        StartCoroutine(DelayedOpenUI());
    }

    private IEnumerator DelayedOpenUI()
    {
        yield return null;

        if (backpackInstance != null && !backpackInstance.activeSelf)
        {
            backpackInstance.SetActive(true); // ✅ 激活最外层
        }

        if (parentUI != null && !parentUI.activeSelf)
        {
            parentUI.SetActive(true); // ✅ 同时激活子面板
            Debug.Log("🎒 Backpack UI actually activated.");
        }
        else if (parentUI == null)
        {
            Debug.LogWarning("❌ parentUI is null when trying to open backpack.");
        }

        PlayerInventory playerInv = FindObjectOfType<PlayerInventory>();
        if (playerInv != null)
        {
            playerInv.RefreshUI();
        }
    }


    public void CloseBackpack()
    {
        if (parentUI != null)
            parentUI.SetActive(false);

        if (backpackInstance != null)
        {
            backpackInstance.SetActive(false);
            Debug.Log("🎒 Backpack UI closed.");
        }
    }

    public bool IsBackpackOpen()
    {
        return backpackInstance != null && backpackInstance.activeSelf;
    }
}
