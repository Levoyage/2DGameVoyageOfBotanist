using UnityEngine;

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
            Collect(player);
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

    void Collect(GameObject player)
    {
        if (plantData == null)
        {
            Debug.LogError("❌ CollectiblePlant: No ItemData assigned to " + gameObject.name);
            return;
        }

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (GameManager.Instance != null && GameManager.Instance.requiredPlant != null)
        {
            if (plantData == GameManager.Instance.requiredPlant)
            {
                // ✅ correct plant: add to inventory and play sound
                if (inventory != null)
                {
                    inventory.AddPlant(plantData);
                }

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
                }
            }
            else
            {
                // ❌ wrong plant: play sound and apply penalty
                GameManager.Instance.ApplyWrongPlantPenalty();
            }
        }

        GatherPromptManager.Instance.Hide();
        Destroy(gameObject);
    }

}
