using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CodexUIController : MonoBehaviour
{
    [Header("Herbs to Show in Codex")]
    public List<ItemData> knownHerbs;

    [Header("Codex Entry UI")]
    public GameObject codexPanel;
    public GameObject detailPanel;

    public GameObject entryButtonPrefab;  // ✅ 新增：用来动态生成按钮
    public Transform codexEntryContainer; // ✅ 新增：条目按钮的容器（Vertical Layout）

    [Header("Detail View UI")]
    public Image herbImage;
    public TMP_Text detailTitle;
    public TMP_Text detailDescription;
    public TMP_Text herbLocation;
    public Button returnButton;

    private ItemData currentItem;

    void Start()
    {
        codexPanel.SetActive(true);
        detailPanel.SetActive(false);

        GenerateCodexEntries();


        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToCodex);
    }

    public void ShowDetails(ItemData herb)
    {
        currentItem = herb;
        codexPanel.SetActive(false);
        detailPanel.SetActive(true);

        herbImage.sprite = herb.herbLargeImage;
        detailTitle.text = herb.itemName;
        detailDescription.text = herb.description;
        herbLocation.text = herb.growthLocation;
    }


    public void GenerateCodexEntries()
    {
        Debug.Log($"[Codex] GenerateCodexEntries()  knownHerbs.Count = {knownHerbs.Count}");

        // 先清空旧条目
        foreach (Transform child in codexEntryContainer)
            Destroy(child.gameObject);

        foreach (ItemData herb in knownHerbs)
        {
            if (herb == null) continue;   // 忽略 null

            GameObject entry = Instantiate(entryButtonPrefab, codexEntryContainer);

            //use transform.find to get the child components
            TMP_Text title = entry.transform.Find("herbTitle")?.GetComponent<TMP_Text>();
            Image icon = entry.transform.Find("herbIcon")?.GetComponent<Image>();
            Button btn = entry.transform.Find("detailButton")?.GetComponent<Button>();

            if (title != null) title.text = herb.itemName;
            if (icon != null) icon.sprite = herb.itemIcon;

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    // ① 先打开详情
                    ShowDetails(herb);

                    // ② 再把新手提示气泡关掉
                    ClinicManager cm = FindObjectOfType<ClinicManager>();
                    if (cm != null)
                    {
                        cm.HideCodexInstructionBubble();
                    }
                });
            }
            else
            {
                Debug.LogWarning($"[Codex] ❌ 预制体上找不到 detailButton ▶ {herb.itemName}");
            }
        }
    }

    public void AddNewEntries(List<ItemData> newHerbs)
    {
        // 先把已有的和新增的合并（避免重复）
        List<ItemData> updatedList = new List<ItemData>(knownHerbs);

        foreach (var herb in newHerbs)
        {
            if (!updatedList.Contains(herb))  // 避免重复添加
                updatedList.Add(herb);
        }

        knownHerbs = updatedList;  // 更新为 List
        GenerateCodexEntries();              // 重新生成条目
    }

    // ①-2  在 SetKnownHerbs 里过滤 null
    public void SetKnownHerbs(List<ItemData> newHerbs)
    {
        if (newHerbs == null || newHerbs.Count == 0) return;

        knownHerbs.Clear();
        foreach (var h in newHerbs)
            if (h != null)                       // <-- NEW
                knownHerbs.Add(h);

        GenerateCodexEntries();
    }

    public void ReturnToCodex()
    {
        detailPanel.SetActive(false);
        codexPanel.SetActive(true);
    }
}
