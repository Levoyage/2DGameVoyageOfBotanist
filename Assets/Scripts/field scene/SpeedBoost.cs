using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float boostAmount = 2f; // Speed multiplier
    public float boostDuration = 5f; // Duration of the speed boost
    public AudioClip pickupSound; // Audio clip for the pickup sound

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Check if the player touched the boot
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ApplySpeedBoost(boostAmount, boostDuration);
            }

            // Play the pickup sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject); // Remove the boot from the scene
        }
    }
}
