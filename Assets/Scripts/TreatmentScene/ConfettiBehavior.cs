using UnityEngine;

/// <summary>
/// Controls confetti falling behavior after successful treatment.
/// Attach this script to each confetti prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ConfettiBehavior : MonoBehaviour
{
    public float lifetime = 3f; // How long the confetti stays before disappearing
    public float fallSpeedMin = -5f; // Minimum Y force (downward)
    public float fallSpeedMax = -7f; // Maximum Y force (downward)
    public float sideForce = 1f;      // Sideways random force
    public float torqueAmount = 5f;  // Rotation randomness

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Make sure gravity is enabled
            rb.gravityScale = 5f;

            // Apply initial random sideways force + downward force
            float forceX = Random.Range(-sideForce, sideForce);
            float forceY = Random.Range(fallSpeedMin, fallSpeedMax);
            rb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);

            // Apply random rotation
            float torque = Random.Range(-torqueAmount, torqueAmount);
            rb.AddTorque(torque, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("[ConfettiBehavior] Missing Rigidbody2D component!");
        }

        // Auto destroy after some time
        Destroy(gameObject, lifetime);
    }
}
