using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Shooting & Capture Settings")]
    public float bulletSpeed = 10f;               // Speed at which the bullet travels.
    public float captureRange = 100f;             // (Not used directly now, but kept for reference.)

    [Header("Trajectory Visualization")]
    public LineRenderer trajectoryLine;           // Displays an aim arrow from the gun.

    [Header("Dynamic Aiming Settings")]
    public float dynamicAimMaxDistance = 100f;      // Maximum distance for raycast (and bullet travel).
    public float aimArrowLength = 5f;               // Fixed length of the drawn aim arrow.

    [Header("Release Settings")]
    public float releaseOffset = 0.5f;              // How far ahead of the gun the captured object is placed on release.
    public float releaseForce = 5f;                 // Impulse force applied when releasing an object.

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;               // Assign your bullet prefab (a small green dot with a trigger collider).

    // For capturing and releasing objects.
    private GameObject capturedObject = null;
    private Rigidbody2D capturedRigidbody = null;

    void Start()
    {
        // Ensure the gun is positioned on the z = 0 plane.
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        // Debug.Log("Gun position forced to: " + transform.position);

        // Get or assign the LineRenderer.
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponent<LineRenderer>();
            if (trajectoryLine == null)
            {
                Debug.LogError("No LineRenderer found! Please attach one to the Gun GameObject.");
                return;
            }
        }

        // Configure the LineRenderer using "Sprites/Default" for 2D visibility.
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.material.color = Color.red;
        trajectoryLine.startWidth = 0.3f;
        trajectoryLine.endWidth = 0.3f;
        trajectoryLine.startColor = Color.red;
        trajectoryLine.endColor = Color.red;
        trajectoryLine.sortingLayerName = "Default";
        trajectoryLine.sortingOrder = 1000;
    }

    void Update()
    {
        // Always update the dynamic trajectory (aim arrow) so it shows a fixed-length line from the gun toward the mouse.
        DrawDynamicTrajectory();

        // Left mouse click:
        // • If no object is currently captured, fire a bullet.
        // • If an object is captured, release it.
        if (Input.GetMouseButtonDown(0))
        {
            if (capturedObject == null)
                FireBullet();
            else
                ReleaseObject();
        }
    }

    /// <summary>
    /// Draws a fixed-length aim arrow from the gun toward the mouse pointer.
    /// The arrow stops early if an obstacle is detected within the aim arrow length.
    /// </summary>
    void DrawDynamicTrajectory()
    {
        // The starting point is the gun's position.
        Vector3 startPos = transform.position;
        startPos.z = 0f;

        // Convert the current mouse position from screen space to world space.
        Vector3 mouseScreenPos = Input.mousePosition;
        float distanceFromCamera = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        mouseScreenPos.z = distanceFromCamera;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Debug: Print the computed mouse world position.
        // Debug.Log("Mouse World Position: " + mouseWorldPos);

        // Compute the normalized direction from the gun to the mouse.
        Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)startPos).normalized;
        // Update the gun's orientation so its right side points toward the mouse.
        transform.right = direction;

        // Compute the desired endpoint based on a fixed aim arrow length.
        Vector3 desiredEnd = startPos + (Vector3)direction * aimArrowLength;
        // Perform a raycast along the direction (up to the aim arrow length) to check for obstacles.
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, aimArrowLength);
        Vector3 endPos = desiredEnd;
        endPos.z = 0f;

        // Debug: Print the start and end positions of the line.
        // Debug.Log("Line Start: " + startPos + " | Line End: " + endPos);

        // Update the LineRenderer.
        trajectoryLine.positionCount = 2;
        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, endPos);
    }

    /// <summary>
    /// Fires a bullet from the gun in the direction it is facing.
    /// The bullet is a small green dot that travels along the aim ray.
    /// </summary>
    void FireBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab is not assigned!");
            return;
        }
        // Spawn the bullet slightly ahead of the gun.
        Vector3 spawnPos = transform.position + transform.right * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.speed = bulletSpeed;
            bulletScript.maxDistance = dynamicAimMaxDistance;
            // Initialize the bullet with the gun's current right direction and pass a reference to this GunController.
            bulletScript.Initialize(transform.right, this);
        }
    }

    /// <summary>
    /// Called by the bullet when it collides with a valid target (tag "Box").
    /// This method captures the target object.
    /// </summary>
    public void CaptureWithBullet(GameObject target)
    {
        if (target == null) return;
        capturedObject = target;
        capturedRigidbody = capturedObject.GetComponent<Rigidbody2D>();
        if (capturedRigidbody != null)
        {
            capturedRigidbody.velocity = Vector2.zero;
            capturedRigidbody.angularVelocity = 0f;
            capturedRigidbody.isKinematic = true;
        }
        // Hide the object.
        SpriteRenderer sr = capturedObject.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;
        Collider2D col = capturedObject.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        // Debug.Log("Box captured via bullet: " + capturedObject.name);
    }

    /// <summary>
    /// Releases the captured object by re-enabling its visuals and physics,
    /// placing it ahead of the gun, and applying an impulse force.
    /// </summary>
    void ReleaseObject()
    {
        if (capturedObject != null)
        {
            SpriteRenderer sr = capturedObject.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = true;
            Collider2D col = capturedObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
            capturedObject.transform.position = transform.position + transform.right * releaseOffset;
            capturedObject.transform.parent = null;
            if (capturedRigidbody != null)
            {
                capturedRigidbody.isKinematic = false;
                capturedRigidbody.velocity = Vector2.zero;
                Vector2 aimDir = transform.right;
                capturedRigidbody.AddForce(aimDir * releaseForce * 1.5f, ForceMode2D.Impulse);
            }
            // Debug.Log("Box released: " + capturedObject.name);
            capturedObject = null;
            capturedRigidbody = null;
        }
    }
}
