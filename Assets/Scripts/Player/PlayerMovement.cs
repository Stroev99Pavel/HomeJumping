using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private Animator animator;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Wall Jump")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.4f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 14f);
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpControlLockTime = 0.2f;
    private float wallJumpControlLockCounter;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private int facingDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckGround();
        CheckWall();
        WallSlide();
        UpdateAnimations();
        Flip();

        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
        if (isTouchingWall && !isGrounded && moveInput.x != 0)
        {
            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
    }
    private void UpdateAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGrounded", isGrounded);
    }
    private void WallSlide()
    {
        if (isTouchingWall && !isGrounded)
        {
            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
    }

    private void FixedUpdate()
    {
        float xVelocity = moveInput.x * moveSpeed;

        if (isTouchingWall && !isGrounded && moveInput.x == facingDirection)
        {
            xVelocity = 0;
        }

        rb.linearVelocity = new Vector2(xVelocity, rb.linearVelocity.y);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        else if (isTouchingWall && moveInput.x != facingDirection)
        {
            rb.linearVelocity = new Vector2(-facingDirection * wallJumpForce.x, wallJumpForce.y);
            wallJumpControlLockCounter = wallJumpControlLockTime;
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void CheckWall()
    {
        isTouchingWall = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * facingDirection,
            wallCheckDistance,
            wallLayer
        );
    }

    private void Flip()
    {
        if (moveInput.x > 0)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < 0)
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + Vector3.right * facingDirection * wallCheckDistance
            );
        }
    }
}