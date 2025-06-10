using UnityEngine;

public class DebugBackpackInitializer : MonoBehaviour
{
    public GameObject backpackSystemPrefab;

    void Start()
    {
        if (BackpackSystemManager.Instance == null)
        {
            Debug.Log("🧪 Instantiating BackpackSystem manually for debug...");
            Instantiate(backpackSystemPrefab);
        }
    }
}
