using UnityEngine;

public class ClinicSceneBackpackLoader : MonoBehaviour
{
    public GameObject backpackSystemPrefab;

    void Awake()
    {
        if (BackpackSystemManager.Instance == null && backpackSystemPrefab != null)
        {
            GameObject obj = Instantiate(backpackSystemPrefab);
            obj.name = "BackpackSystemManager (Runtime)";
            DontDestroyOnLoad(obj);
            Debug.Log("🧰 Injected BackpackSystemManager for standalone scene test.");
        }
    }
}
