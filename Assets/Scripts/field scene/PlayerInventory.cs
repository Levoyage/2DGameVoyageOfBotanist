using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public GameObject inventoryUI;     // UI面板本体（可选，用于显示控制）
    public Transform slotGrid;         // 格子容器（SlotLight）

    public List<SlotData> slots = new List<SlotData>(); // 存储背包格子数据
    private BackPackUI backpackUI;     // 背包 UI 控制器

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 若想保留它在多个 scene 中
    }

    private void Start()
    {
        InitializeSlots();

        // 不再强制查找 UI，而是等 BackpackSystemManager 来注册
        if (backpackUI != null)
        {
            UpdateInventoryUI();
        }
        else
        {
            Debug.Log("⏳ Waiting for BackPackUI to be registered...");
        }
    }

    /// <summary>
    /// 消耗 / 移除背包中的物品
    /// </summary>
    /// <param name="item">要移除的 ItemData</param>
    /// <param name="amount">数量（默认 1）</param>
    /// <returns>移除成功则返回 true，失败（数量不足 / 没找到）返回 false</returns>
    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            Debug.LogWarning("⚠️ RemoveItem 参数非法。");
            return false;
        }

        SlotData slot = FindSlotForItem(item);
        if (slot == null || slot.quantity < amount)
        {
            Debug.LogWarning($"❌ 无法移除 {amount} × {item.itemName} —— 数量不足或不存在。");
            return false;
        }

        slot.quantity -= amount;

        // 如果数量耗尽就清空格子
        if (slot.quantity <= 0)
        {
            slot.ClearSlot();
        }

        UpdateInventoryUI();
        Debug.Log($"🗑️ Consumed {amount} × {item.itemName}");
        return true;
    }

    /// <summary>
    /// 初始化 24 格空格子
    /// </summary>
    private void InitializeSlots()
    {
        slots.Clear();
        for (int i = 0; i < 24; i++)
        {
            slots.Add(new SlotData(null, 0));
        }
    }

    /// <summary>
    /// 添加植物/物品
    /// </summary>
    public void AddPlant(ItemData plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("🌿 Tried to add null item.");
            return;
        }

        SlotData slot = FindSlotForItem(plant);
        if (slot != null)
        {
            slot.quantity++;
        }
        else
        {
            foreach (var s in slots)
            {
                if (s.IsEmpty())
                {
                    s.item = plant;
                    s.quantity = 1;
                    break;
                }
            }
        }

        UpdateInventoryUI();
        Debug.Log($"✅ Collected: {plant.itemName}");
    }

    /// <summary>
    /// 查询已有植物格
    /// </summary>
    private SlotData FindSlotForItem(ItemData plant)
    {
        foreach (var slot in slots)
        {
            if (slot.item == plant)                    // ① 引用相等
                return slot;

            if (slot.item != null && plant != null &&  // ② 名字相等
                slot.item.itemName == plant.itemName)
                return slot;
        }
        return null;
    }

    /// <summary>
    /// 查询指定物品数量
    /// </summary>
    public int GetPlantCount(ItemData plant)
    {
        SlotData slot = FindSlotForItem(plant);
        return slot != null ? slot.quantity : 0;
    }

    /// <summary>
    /// 清空背包
    /// </summary>
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }

        Debug.Log("\ud83e\uddfa Inventory cleared.");
        if (backpackUI != null)
        {
            UpdateInventoryUI();
        }
        else
        {
            Debug.Log("\u23f3 UI not ready yet during ClearInventory()");
        }
    }

    /// <summary>
    /// 同步 UI 显示
    /// </summary>
    private void UpdateInventoryUI()
    {
        if (backpackUI == null)
        {
            backpackUI = FindObjectOfType<BackPackUI>();
            if (backpackUI == null)
            {
                Debug.LogWarning("❌ Cannot find BackPackUI to update.");
                return;
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < backpackUI.slotuiList.Count)
            {
                backpackUI.slotuiList[i].SetItem(slots[i].item, slots[i].quantity);
            }
        }
    }

    /// <summary>
    /// 通用添加接口
    /// </summary>
    public void AddItem(ItemData item)
    {
        AddPlant(item); // 简化逻辑
    }

    /// <summary>
    /// 提供外部强制刷新 UI 的方法（例如 ClinicScene 进入时）
    /// </summary>
    public void RefreshUI()
    {
        Debug.Log("\ud83d\udce6 Refreshing backpack UI via PlayerInventory.");
        UpdateInventoryUI();
    }

    public void SetBackpackUI(BackPackUI backpackUI)
    {
        this.backpackUI = backpackUI;
        RefreshUI();
    }
}
