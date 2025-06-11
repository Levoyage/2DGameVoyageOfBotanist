using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Animator animator;

    private Vector2 lastMoveDirection;
    public bool canMove = false;

    private float originalMoveSpeed; // Store the original speed for resetting
    private bool isSpeedBoosted = false;

    // Invincibility state and duration
    private bool isInvincible = false;
    public float invincibilityDuration = 2f;

    // Sprite renderer for flashing effect
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        lastMoveDirection = Vector2.down; // Default direction is down
        originalMoveSpeed = moveSpeed; // Store the original speed
    }

    void Update()
    {
        ProcessInputs();
    }

    void FixedUpdate()
    {
        Move();
    }

    void ProcessInputs()
    {
        if (!canMove)
        {
            moveDirection = Vector2.zero;
            animator.SetBool("isMoving", false);
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        bool isMoving = moveX != 0 || moveY != 0;

        if (isMoving)
        {
            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveY", moveY);
            lastMoveDirection = moveDirection;
        }
        else
        {
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);
        }

        animator.SetBool("isMoving", isMoving);
    }

    void Move()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }

    public void ApplySpeedBoost(float boostAmount, float duration)
    {
        if (isSpeedBoosted) return; // Prevent multiple boosts

        isSpeedBoosted = true;
        moveSpeed *= boostAmount; // Increase speed
        Invoke("ResetSpeed", duration); // Reset speed after duration
    }

    void ResetSpeed()
    {
        moveSpeed = originalMoveSpeed; // Reset to original speed
        isSpeedBoosted = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Snake") && !isInvincible)
        {
            LoseLifeAndTriggerInvincibility();
        }
    }

    private void LoseLifeAndTriggerInvincibility()
    {
        isInvincible = true;

        // 区分场景调用对应 LoseLife()
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "FieldScene" && GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
        else if (scene == "FieldScene-1" && GameManager1.Instance != null)
        {
            GameManager1.Instance.LoseLife();
        }

        StartCoroutine(FlashSprite());
    }

    private IEnumerator FlashSprite()
    {
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle sprite visibility
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        spriteRenderer.enabled = true; // Ensure sprite is visible
        isInvincible = false;
    }
}
