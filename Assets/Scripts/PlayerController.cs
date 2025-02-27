using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpPower = 16f;
    private bool isFacingRight = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        }


        // Variable jump if we want it
        // if(InputGetButtonUp(KeyCode.W) || Input.GetButtonUp(KeyCode.Up) && rb.velocity.y > 0f)
        // {
        //     rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        // }


        Flip();
    }

    // 
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    // Check if the player is on the ground
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private void Flip()
    {
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}

// {
//     public float moveSpeed = 5f;      // Speed of the player
//     public float jumpForce = 10f;     // Force applied for the jump
//     private Rigidbody2D rb;           // Rigidbody2D component for physics-based movement
//     private bool isGrounded;          // Check if the player is grounded to prevent double-jumping

//     public Transform groundCheck;     // Transform to check if the player is on the ground
//     public LayerMask groundLayer;     // Ground layer mask for ground detection

//     public bool isTeleporting = false;  // Flag to prevent re-teleportation

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//     }

//     void Update()
//     {
//         // Get input for horizontal movement (WASD or Arrow keys)
//         float moveX = Input.GetAxisRaw("Horizontal");  // -1 for A, 1 for D

//         // Create a Vector2 for movement direction
//         Vector2 movement = new Vector2(moveX, 0f).normalized;

//         // Apply movement to the Rigidbody2D component
//         rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);

//         // Check if the player is on the ground before allowing jumping
//         isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

//         // Jump when the player presses the W key and is grounded
//         if (isGrounded && Input.GetKeyDown(KeyCode.W))
//         {
//             rb.velocity = new Vector2(rb.velocity.x, jumpForce);  // Apply vertical velocity for jumping
//         }
//     }
// }
