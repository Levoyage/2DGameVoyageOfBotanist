using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventory", menuName = "Inventory/InventoryData")]
public class InventoryData : ScriptableObject
{
    public int maxSlots = 24; 
    public List<SlotData> slots = new List<SlotData>();

    public void InitializeInventory()
    {
        if (slots == null)
        {
            slots = new List<SlotData>();
        }

        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new SlotData(null, 0));
        }
    }
}
