using UnityEngine;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SlotAutoBinder : MonoBehaviour
{
    public Transform slotGrid; // 拖入 SlotGrid
    public BackPackUI backPackUI; // 拖入 BackPackUI


    [ContextMenu("Auto Bind Slots")]
    public void AutoBindSlots()
    {
        if (backPackUI == null || slotGrid == null)
        {
            Debug.LogError("❌ SlotAutoBinder: BackPackUI or SlotGrid is not assigned!");
            return;
        }

        // 清空 slotuiList
        backPackUI.slotuiList.Clear();

        for (int i = 0; i < slotGrid.childCount; i++)
        {
            Transform slot = slotGrid.GetChild(i);

            // 🛠 确保 SlotUI 组件不会重复添加
            SlotUI slotUI = slot.GetComponent<SlotUI>();
            if (slotUI == null)
                slotUI = slot.gameObject.AddComponent<SlotUI>();

            slotUI.index = i;
            slotUI.icon = slot.Find("Image")?.GetComponent<Image>();
            slotUI.countText = slot.Find("count")?.GetComponent<TextMeshProUGUI>();

            // 🛠 额外检查：如果 icon 为空，就禁用 Image 防止显示白色图片
            if (slotUI.icon == null)
            {
                Debug.LogWarning($"⚠️ Slot {i} 没有绑定 Image，可能会显示白色图片");
            }
            else
            {
                slotUI.icon.enabled = false; // 确保 slot 为空时不显示白色
            }

            backPackUI.slotuiList.Add(slotUI);
        }

        Debug.Log("✅ AutoBindSlots 完成！");
    }

}
