using UnityEngine;

public class GunController : MonoBehaviour
{
    public float bulletSpeed = 10f;

    public float captureRange = 100f; // Increase if needed for testing

    public int trajectoryPoints = 30;

    public float timeStep = 0.1f;

    // Reference to the existing LineRenderer component.
    public LineRenderer trajectoryLine;

    // For capturing and releasing objects.
    private GameObject capturedObject = null;

    private Rigidbody2D capturedRigidbody = null;

    // A fixed maximum distance for dynamic aiming (adjust as needed)
    public float dynamicAimMaxDistance = 100f;

    void Start()
    {
        // Use the assigned LineRenderer or get it from this GameObject.
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponent<LineRenderer>();
            if (trajectoryLine == null)
            {
                Debug
                    .LogError("No LineRenderer found! Please attach one to the Gun GameObject.");
                return;
            }
        }

        // Configure the LineRenderer (adjust settings as needed).
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.red;
        trajectoryLine.endColor = Color.red;
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.sortingLayerName = "Default"; // Ensure this layer is visible in your camera's culling mask.
        trajectoryLine.sortingOrder = 100; // Set high enough to appear on top.
    }

    void Update()
    {
        // Only allow mouse click actions when the G key is held.
        if (Input.GetKey(KeyCode.G))
        {
            // Right mouse button held: dynamic aiming.
            if (Input.GetMouseButton(1))
            {
                DrawDynamicTrajectory();
            }
            else
            {
                // If G is held but right click is not active,
                // show the full bullet trajectory.
                DrawTrajectory();
            }

            // Process left mouse button clicks only when G is held.
            if (Input.GetMouseButtonDown(0))
            {
                if (capturedObject == null)
                {
                    TryCapture();
                }
                else
                {
                    ReleaseObject();
                }
            }
        }
        else
        {
            // If G is not held, clear the trajectory line.
            trajectoryLine.positionCount = 0;
        }
    }

    // Draws the bullet’s full trajectory (ignoring gravity) from the gun.
    void DrawTrajectory()
    {
        Vector2 startPos2D = transform.position;
        Vector2 aimDir = transform.right; // The gun’s right side is considered forward.
        Vector2 velocity = aimDir * bulletSpeed;
        Vector3[] points = new Vector3[trajectoryPoints];

        // Calculate points along a straight-line path.
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector2 pos2D = startPos2D + velocity * t;

            // Force the z coordinate to 0 so it's visible.
            points[i] = new Vector3(pos2D.x, pos2D.y, 0f);
        }

        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.SetPositions (points);
    }

    /// Draws a two-point dynamic trajectory line while right clicking.
    /// One end is fixed at the gun, the other follows the mouse,
    /// but stops early if a collider is encountered.
    /// Also rotates the gun to face the mouse.
    void DrawDynamicTrajectory()
    {
        // Use the gun's position as a starting point (force z = 0).
        Vector3 startPos = transform.position;
        startPos.z = 0f;

        // Convert the mouse position to world coordinates.
        // We use the gun's z so that the conversion is on the same plane.
        Vector3 gunScreenPos =
            Camera.main.WorldToScreenPoint(transform.position);
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = gunScreenPos.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Determine the direction from the gun to the mouse.
        Vector2 direction =
            ((Vector2) mouseWorldPos - (Vector2) startPos).normalized;

        // Rotate the gun so that its right side points toward the mouse.
        transform.right = direction;

        // Raycast along the direction to check for obstacles.
        RaycastHit2D hit =
            Physics2D.Raycast(startPos, direction, dynamicAimMaxDistance);
        Vector3 endPos =
            startPos + (Vector3)(direction * dynamicAimMaxDistance); // Default endpoint.

        if (hit.collider != null)
        {
            // If an obstacle is hit, set the endpoint to the collision point.
            endPos = hit.point;
            endPos.z = 0f;
        }

        // Optionally, if you prefer the endpoint to exactly match the mouse position when nothing is hit,
        // you can uncomment the following line:
        // else { endPos = mouseWorldPos; }
        // Update the LineRenderer with the fixed start and dynamic endpoint.
        trajectoryLine.positionCount = 2;
        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, endPos);
    }

    /// Attempts to capture an object (with tag "Box") in the direction the gun is pointing.
    void TryCapture()
    {
        // Offset the raycast origin so it starts just ahead of the gun.
        Vector2 offset = transform.right * 0.5f;
        Vector2 startPos = (Vector2) transform.position + offset;
        Vector2 aimDir = transform.right;

        // Use RaycastAll to get every collider along the ray.
        RaycastHit2D[] hits =
            Physics2D.RaycastAll(startPos, aimDir, captureRange);

        RaycastHit2D validHit = new RaycastHit2D();
        foreach (RaycastHit2D hit in hits)
        {
            // Skip the player even if on the same layer.
            if (hit.collider.CompareTag("Player")) continue;

            // Check for a box.
            if (hit.collider.CompareTag("Box"))
            {
                validHit = hit;
                break;
            }
        }

        if (validHit.collider != null)
        {
            Debug
                .Log("Raycast hit valid box: " +
                validHit.collider.gameObject.name);

            capturedObject = validHit.collider.gameObject;
            capturedRigidbody = capturedObject.GetComponent<Rigidbody2D>();
            if (capturedRigidbody != null)
            {
                // Stop any movement and disable physics while held.
                capturedRigidbody.velocity = Vector2.zero;
                capturedRigidbody.angularVelocity = 0f;
                capturedRigidbody.isKinematic = true;
            }

            // Instead of parenting the box to the gun (which makes it move with the gun),
            // disable its visual representation and collider so it “disappears.”
            SpriteRenderer sr = capturedObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
            Collider2D col = capturedObject.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            Debug.Log("Box captured and hidden: " + capturedObject.name);
        }
        else
        {
            Debug.Log("Raycast did not hit any valid box.");
        }
    }

    
    // Releases the captured object by unparenting it and applying an impulse force.
    // Add these public variables to adjust the release offset and force.
    public float releaseOffset = 0.5f; // Reduce the offset to keep the object closer.

    public float releaseForce = 5f; // A lower force to minimize sliding.

    void ReleaseObject()
    {
        if (capturedObject != null)
        {
            // Re-enable the box's visual and collision components.
            SpriteRenderer sr = capturedObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
            }
            Collider2D col = capturedObject.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
            }

            // Position the box slightly ahead of the gun based on releaseOffset.
            capturedObject.transform.position =
                transform.position + transform.right * releaseOffset;
            capturedObject.transform.parent = null;

            if (capturedRigidbody != null)
            {
                // Re-enable physics.
                capturedRigidbody.isKinematic = false;

                // Reset any existing velocity to minimize extra movement.
                capturedRigidbody.velocity = Vector2.zero;

                // Apply an impulse force using the new releaseForce multiplier.
                Vector2 aimDir = transform.right;
                capturedRigidbody
                    .AddForce(aimDir * releaseForce, ForceMode2D.Impulse);
            }

            Debug.Log("Box released: " + capturedObject.name);
            capturedObject = null;
            capturedRigidbody = null;
        }
    }
}
