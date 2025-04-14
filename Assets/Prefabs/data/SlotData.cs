[System.Serializable]
public class SlotData
{
    public ItemData item;  
    public int quantity;   

    public SlotData(ItemData newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }

    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }

    public void ClearSlot()
    {
        item = null;
        quantity = 0;
    }
}
