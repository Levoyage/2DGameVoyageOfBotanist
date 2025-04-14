using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public GameObject inventoryUI;         // Inventory UI 面板（可控制开关）
    public Transform slotGrid;             // Slot 容器（包含所有 SlotLight）

    public List<SlotData> slots = new List<SlotData>(); // 储存格子数据
    private BackPackUI backpackUI;                      // 背包 UI 管理器

    void Start()
    {
        InitializeSlots();
        backpackUI = FindObjectOfType<BackPackUI>(); // 查找 UI 管理器
    }

    /// <summary>
    /// 初始化所有空格子
    /// </summary>
    private void InitializeSlots()
    {
        slots.Clear();

        for (int i = 0; i < 24; i++) // 默认 24 个背包格子
        {
            slots.Add(new SlotData(null, 0));
        }
    }

    /// <summary>
    /// 添加植物
    /// </summary>
    public void AddPlant(ItemData plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("🌿 AddPlant was called with null ItemData.");
            return;
        }

        SlotData targetSlot = FindSlotForItem(plant);

        if (targetSlot != null)
        {
            targetSlot.quantity++;
        }
        else
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty())
                {
                    slots[i].item = plant;
                    slots[i].quantity = 1;
                    break;
                }
            }
        }

        Debug.Log($"✅ Collected: {plant.itemName}");
        UpdateInventoryUI();
    }

    /// <summary>
    /// 查找已有植物对应的 Slot
    /// </summary>
    private SlotData FindSlotForItem(ItemData plant)
    {
        foreach (var slot in slots)
        {
            if (slot.item == plant)
            {
                return slot;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取某种植物的数量
    /// </summary>
    public int GetPlantCount(ItemData plant)
    {
        SlotData slot = FindSlotForItem(plant);
        return slot != null ? slot.quantity : 0;
    }

    /// <summary>
    /// 清空所有背包数据
    /// </summary>
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }

        Debug.Log("🧺 Inventory cleared.");
        UpdateInventoryUI();
    }

    /// <summary>
    /// 更新 UI 上的图标和数量
    /// </summary>
    private void UpdateInventoryUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            SlotData data = slots[i];

            if (i < backpackUI.slotuiList.Count)
            {
                SlotUI slotUI = backpackUI.slotuiList[i];
                slotUI.SetItem(data.item, data.quantity);
            }
        }
    }


}
