using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Shooting & Capture Settings")]
    public float bulletSpeed = 10f;
    public float captureRange = 100f; // Increase if needed for testing

    [Header("Trajectory Visualization")]
    public LineRenderer trajectoryLine;

    [Header("Dynamic Aiming Settings")]
    // Maximum distance used for the raycast that stops the line if an obstacle is hit.
    public float dynamicAimMaxDistance = 100f;
    // The fixed length of the aim arrow (line).
    public float aimArrowLength = 1f;

    [Header("Release Settings")]
    public float releaseOffset = 0.5f; // How far ahead of the gun the object appears on release.
    public float releaseForce = 5f;    // Force applied to the released object.

    // For capturing and releasing objects.
    private GameObject capturedObject = null;
    private Rigidbody2D capturedRigidbody = null;

    void Start()
    {
        // Force the gun's z position to 0.
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        Debug.Log("Gun position forced to: " + transform.position);

        // Use the assigned LineRenderer or get it from this GameObject.
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponent<LineRenderer>();
            if (trajectoryLine == null)
            {
                Debug.LogError("No LineRenderer found! Please attach one to the Gun GameObject.");
                return;
            }
        }

        // Configure the LineRenderer using "Sprites/Default" shader for better 2D visibility.
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.material.color = Color.red;
        trajectoryLine.startWidth = 0.3f; // Thicker for visibility.
        trajectoryLine.endWidth = 0.3f;
        trajectoryLine.startColor = Color.red;
        trajectoryLine.endColor = Color.red;
        trajectoryLine.sortingLayerName = "Default"; // Ensure this layer is visible.
        trajectoryLine.sortingOrder = 1000;          // Increase order to draw on top.
    }

    void Update()
    {
        // Always update the dynamic trajectory line so it follows the mouse pointer.
        DrawDynamicTrajectory();

        // Process left mouse button clicks to capture or release an object.
        if (Input.GetMouseButtonDown(0))
        {
            if (capturedObject == null)
                TryCapture();
            else
                ReleaseObject();
        }
    }

    /// <summary>
    /// Draws a dynamic trajectory line from the gun's position in the direction of the mouse pointer.
    /// The line is drawn with a fixed length (aimArrowLength) unless an obstacle is encountered within that range.
    /// </summary>
    void DrawDynamicTrajectory()
    {
        // Start position: the gun's position (ensure z = 0 for 2D).
        Vector3 startPos = transform.position;
        startPos.z = 0f;

        // Get the current mouse position in screen space.
        Vector3 mouseScreenPos = Input.mousePosition;
        // Calculate the distance between the camera and the gun along the z-axis.
        float distanceFromCamera = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        mouseScreenPos.z = distanceFromCamera;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Debug: Print the mouse world position.
        Debug.Log("Mouse World Position: " + mouseWorldPos);

        // Compute the normalized direction from the gun to the mouse pointer.
        Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)startPos).normalized;

        // Update the gun's orientation so its right side points toward the mouse.
        transform.right = direction;

        // Calculate the desired endpoint using the fixed arrow length.
        Vector3 desiredEnd = startPos + (Vector3)direction * aimArrowLength;

        // Optionally perform a raycast along that direction to check for obstacles within the aim arrow length.
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, aimArrowLength);
        Vector3 endPos = desiredEnd;
        endPos.z = 0f;

        // Debug: Print start and end positions.
        Debug.Log("Line Start: " + startPos + " | Line End: " + endPos);

        // Update the LineRenderer with the computed start and end positions.
        trajectoryLine.positionCount = 2;
        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, endPos);
    }

    /// <summary>
    /// Attempts to capture an object (with tag "Box") in the gun's aim direction.
    /// </summary>
    void TryCapture()
    {
        // Offset the raycast origin so it starts just ahead of the gun.
        Vector2 offset = transform.right * 0.5f;
        Vector2 startPos = (Vector2)transform.position + offset;
        Vector2 aimDir = transform.right;

        // Use RaycastAll to get every collider along the ray.
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, aimDir, captureRange);
        RaycastHit2D validHit = new RaycastHit2D();
        foreach (RaycastHit2D hit in hits)
        {
            // Skip the player even if on the same layer.
            if (hit.collider.CompareTag("Player"))
                continue;

            // Check for a box.
            if (hit.collider.CompareTag("Box"))
            {
                validHit = hit;
                break;
            }
        }

        if (validHit.collider != null)
        {
            Debug.Log("Raycast hit valid box: " + validHit.collider.gameObject.name);

            capturedObject = validHit.collider.gameObject;
            capturedRigidbody = capturedObject.GetComponent<Rigidbody2D>();
            if (capturedRigidbody != null)
            {
                // Stop movement and disable physics while the object is captured.
                capturedRigidbody.velocity = Vector2.zero;
                capturedRigidbody.angularVelocity = 0f;
                capturedRigidbody.isKinematic = true;
            }

            // Hide the box by disabling its SpriteRenderer and Collider2D.
            SpriteRenderer sr = capturedObject.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
            Collider2D col = capturedObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            Debug.Log("Box captured and hidden: " + capturedObject.name);
        }
        else
        {
            Debug.Log("Raycast did not hit any valid box.");
        }
    }

    /// <summary>
    /// Releases the captured object by re-enabling its visuals and physics,
    /// positioning it ahead of the gun, and applying an impulse force.
    /// </summary>
    void ReleaseObject()
    {
        if (capturedObject != null)
        {
            // Re-enable the object's visual and collision components.
            SpriteRenderer sr = capturedObject.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = true;
            Collider2D col = capturedObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;

            // Position the object slightly ahead of the gun.
            capturedObject.transform.position = transform.position + transform.right * releaseOffset;
            capturedObject.transform.parent = null;

            if (capturedRigidbody != null)
            {
                // Re-enable physics.
                capturedRigidbody.isKinematic = false;
                capturedRigidbody.velocity = Vector2.zero; // Reset existing velocity.
                Vector2 aimDir = transform.right;
                capturedRigidbody.AddForce(aimDir * releaseForce, ForceMode2D.Impulse);
            }

            Debug.Log("Box released: " + capturedObject.name);
            capturedObject = null;
            capturedRigidbody = null;
        }
    }
}
