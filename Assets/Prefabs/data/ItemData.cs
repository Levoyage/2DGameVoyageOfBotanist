using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;        
    public Sprite itemIcon;        
    public GameObject itemPrefab;  
    public int maxStackSize = 99;   
    public Sprite herbLargeImage;  
    [TextArea] public string description; 
    [TextArea] public string growthLocation;  // Herbal uses, e.g., for headaches, detoxification, etc.
}
