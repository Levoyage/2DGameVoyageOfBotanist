using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SlotUI : MonoBehaviour
{
    public int index;
    public Image icon; // 拖入 Image
    public TextMeshProUGUI countText; // 拖入 count

    public ItemData itemData;
    public Button button;

    public void SetItem(ItemData item, int quantity)
    {
        if (item != null)
        {
            icon.sprite = item.itemIcon;
            icon.enabled = true;
            countText.text = quantity.ToString();

            this.itemData = item;
        }
        else
        {
            icon.enabled = false;
            countText.text = "";
        }
    }

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {


        // ✅ 安全调用
        if (TreatmentManager1.Instance != null && itemData != null)
        {
            TreatmentManager1.Instance.OnPlantSelected(itemData);
        }
    }

}