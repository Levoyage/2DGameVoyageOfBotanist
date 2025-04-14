using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float detectionRange = 50f;
    public float idleDurationMin = 1f;
    public float idleDurationMax = 2f;
    public float moveDurationMin = 2f;
    public float moveDurationMax = 5f;

    private Transform player;
    private Vector2 moveDirection;
    private Animator animator;
    private Rigidbody2D rb;
    private float moveTimer;
    private float idleTimer;
    private bool isMoving = false;

    private Vector2 lastPosition;
    private float stuckTimer;
    private Vector2[] possibleDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private Vector3 initialPosition;

    public bool canMove = false; // ✅ 新增：由GameManager控制何时开始移动

    public void SaveInitialPosition()
    {
        initialPosition = transform.position;
    }

    public void ResetToInitialPosition()
    {
        transform.position = initialPosition;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        idleTimer = Random.Range(idleDurationMin, idleDurationMax);

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isMoving)
        {
            MoveSnake();
        }
        else
        {
            IdleSnake();
        }
    }

    void MoveSnake()
    {
        moveTimer -= Time.deltaTime;

        rb.velocity = moveDirection * moveSpeed;

        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveY", moveDirection.y);

        if (Vector2.Distance(lastPosition, transform.position) < 0.01f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= 0.5f)
            {
                ChooseAlternativeDirection();
                stuckTimer = 0;
            }
        }
        else
        {
            stuckTimer = 0;
        }

        lastPosition = transform.position;

        if (moveTimer <= 0)
        {
            isMoving = false;
            rb.velocity = Vector2.zero;
            idleTimer = Random.Range(idleDurationMin, idleDurationMax);
        }
    }

    void IdleSnake()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0)
        {
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);

                if (distanceToPlayer <= detectionRange)
                {
                    moveDirection = (player.position - transform.position).normalized;
                }
                else
                {
                    ChooseRandomDirection();
                }
            }
            else
            {
                ChooseRandomDirection();
            }

            moveTimer = Random.Range(moveDurationMin, moveDurationMax);
            isMoving = true;
        }
    }

    void ChooseRandomDirection()
    {
        int direction = Random.Range(0, 4);
        moveDirection = possibleDirections[direction];
    }

    void ChooseAlternativeDirection()
    {
        Vector2 previousDirection = moveDirection;
        Vector2[] alternativeDirections = System.Array.FindAll(possibleDirections, dir => dir != previousDirection);

        int index = Random.Range(0, alternativeDirections.Length);
        moveDirection = alternativeDirections[index];
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ChooseAlternativeDirection();
    }
}
