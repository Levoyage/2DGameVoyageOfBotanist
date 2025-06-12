// File: FieldScene1BackpackLoader.cs
using UnityEngine;

public class FieldScene1BackpackLoader : MonoBehaviour
{
    public GameObject backpackSystemPrefab;

    void Awake()
    {
        if (BackpackSystemManager.Instance == null && backpackSystemPrefab != null)
        {
            GameObject obj = Instantiate(backpackSystemPrefab);
            obj.name = "BackpackSystemManager (Runtime)";
            DontDestroyOnLoad(obj);
            Debug.Log("🧰 Injected BackpackSystemManager for FieldScene-1.");
        }
    }
}
