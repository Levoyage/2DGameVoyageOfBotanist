using UnityEngine;
using System.Collections.Generic;



public class PlayerInventory : MonoBehaviour
{
    public GameObject inventoryUI;         // Inventory UI panel (can be toggled)
    public Transform slotGrid;             // Slot container (contains all SlotLight)

    public List<SlotData> slots = new List<SlotData>(); // Store slot data
    private BackPackUI backpackUI;                      // Backpack UI manager

    void Start()
    {
        InitializeSlots();
        backpackUI = FindObjectOfType<BackPackUI>(); // 查找 UI 管理器
    }

    /// <summary>
    /// Initialize all empty slots
    /// </summary>
    private void InitializeSlots()
    {
        slots.Clear();

        for (int i = 0; i < 24; i++) // Default 24 backpack slots
        {
            slots.Add(new SlotData(null, 0));
        }
    }

    /// <summary>
    /// Add plant
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
    /// Find the slot for an existing plant
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
    /// Get the quantity of a specific plant
    /// </summary>
    public int GetPlantCount(ItemData plant)
    {
        SlotData slot = FindSlotForItem(plant);
        return slot != null ? slot.quantity : 0;
    }

    /// <summary>
    /// Clear all backpack data
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
    /// Update the icons and quantities on the UI
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

    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[Inventory] Tried to add null item.");
            return;
        }

        SlotData slot = FindSlotForItem(item);
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
                    s.item = item;
                    s.quantity = 1;
                    break;
                }
            }
        }

        UpdateInventoryUI();
        Debug.Log($"[Inventory] Added item: {item.itemName}");
    }


}
