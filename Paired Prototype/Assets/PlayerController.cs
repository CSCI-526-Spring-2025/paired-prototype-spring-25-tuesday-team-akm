using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;      // Speed of the player
    public float jumpForce = 10f;     // Force applied for the jump
    private Rigidbody2D rb;           // Rigidbody2D component for physics-based movement
    public bool isGrounded;          // Check if the player is grounded to prevent double-jumping

    public Transform groundCheck;     // Transform to check if the player is on the ground
    public LayerMask groundLayer;     // Ground layer mask for ground detection

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input for horizontal movement (WASD or Arrow keys)
        float moveX = Input.GetAxisRaw("Horizontal");  // -1 for A, 1 for D

        // Create a Vector2 for movement direction
        Vector2 movement = new Vector2(moveX, 0f).normalized;

        // Apply movement to the Rigidbody2D component
        rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);

        // Check if the player is on the ground before allowing jumping
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        // Jump when the player presses the W key and is grounded
        if (isGrounded && Input.GetKeyDown(KeyCode.W))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);  // Apply vertical velocity for jumping
        }
    }
}
