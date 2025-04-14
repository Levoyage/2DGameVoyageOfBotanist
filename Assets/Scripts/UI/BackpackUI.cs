using System.Collections.Generic;
using UnityEngine;

public class BackPackUI : MonoBehaviour
{
    public GameObject parentUI;

    public List<SlotUI> slotuiList;

    private void Start()
    {
        InitUI();
        parentUI.SetActive(false); // Hide backpack by default
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBackpack();
        }
    }

    void InitUI()
    {
        slotuiList = new List<SlotUI>(new SlotUI[24]);
        SlotUI[] slotuiArray = GetComponentsInChildren<SlotUI>(true); // add 'true' to include inactive children

        foreach (SlotUI slotUI in slotuiArray)
        {
            slotuiList[slotUI.index] = slotUI;
        }
    }

    public void ToggleBackpack()
    {
        if (parentUI != null)
        {
            parentUI.SetActive(!parentUI.activeSelf);
        }
    }

    public void CloseBackpack()
    {
        parentUI.SetActive(false);
    }
}
