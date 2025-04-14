using UnityEngine;
using UnityEngine.UI;        
using TMPro;                 


public class SlotUI : MonoBehaviour
{
    public int index;
    public Image icon; // 拖入 Image
    public TextMeshProUGUI countText; // 拖入 count

    public void SetItem(ItemData item, int quantity)
    {
        if (item != null)
        {
            icon.sprite = item.itemIcon;  
            icon.enabled = true;
            countText.text = quantity.ToString();
        }
        else
        {
            icon.enabled = false;
            countText.text = "";
        }
    }
}