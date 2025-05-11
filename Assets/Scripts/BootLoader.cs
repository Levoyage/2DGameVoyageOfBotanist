using UnityEngine;

public class BootLoader : MonoBehaviour
{
    public GameObject gameStateManagerPrefab;

    void Awake()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.Log("✅ BootLoader: Instantiating GameStateManager...");

            if (gameStateManagerPrefab != null)
            {
                GameObject gsm = Instantiate(gameStateManagerPrefab);
                gsm.name = "GameStateManager";
                DontDestroyOnLoad(gsm);
            }
            else
            {
                Debug.LogError("❌ BootLoader: gameStateManagerPrefab not assigned.");
            }
        }
        else
        {
            Debug.Log("✅ BootLoader: GameStateManager already exists.");
        }
    }
}
