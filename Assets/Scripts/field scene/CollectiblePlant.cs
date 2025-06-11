using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectiblePlant : MonoBehaviour
{
    public ItemData plantData;
    public AudioClip pickupSound;
    public float pickupVolume = 1.0f;

    private bool playerInRange = false;
    private GameObject player;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Return)) // Return = Enter
        {
            Collect();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerInRange = true;

            Vector3 offset = new Vector3(0, 1.5f, 0);
            GatherPromptManager.Instance.ShowAt(transform.position + offset);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            GatherPromptManager.Instance.Hide();
        }
    }

    void Collect()
    {
        if (plantData == null)
        {
            Debug.LogError($"❌ {name}: No ItemData assigned!");
            return;
        }

        // 先播放拾取音效
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);

        // 判断当前场景，使用对应的 GameManager
        string scene = SceneManager.GetActiveScene().name;
        bool isCorrect = false;

        if (scene == "FieldScene" && GameManager.Instance != null)
        {
            // FieldScene 用单个 requiredPlant
            var gm = GameManager.Instance;
            if (plantData == gm.requiredPlant)
                isCorrect = true;

            if (isCorrect)
                player.GetComponent<PlayerInventory>()?.AddPlant(plantData);
            else
                gm.ApplyWrongPlantPenalty();
        }
        else if (scene == "FieldScene-1" && GameManager1.Instance != null)
        {
            // FieldScene-1 用数组 requiredPlants
            var gm1 = GameManager1.Instance;
            foreach (var req in gm1.requiredPlants)
            {
                if (plantData == req)
                {
                    isCorrect = true;
                    break;
                }
            }

            if (isCorrect)
                player.GetComponent<PlayerInventory>()?.AddPlant(plantData);
            else
                gm1.ApplyWrongPlantPenalty();
        }
        else
        {
            Debug.LogWarning($"⚠️ No suitable GameManager found in scene '{scene}'.");
        }

        // 隐藏提示并销毁植物
        GatherPromptManager.Instance.Hide();
        Destroy(gameObject);
    }
}
