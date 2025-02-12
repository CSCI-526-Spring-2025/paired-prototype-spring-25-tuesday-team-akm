using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;       // How fast the bullet travels.
    public float maxDistance = 100f; // Maximum distance before the bullet is destroyed.
    
    private Vector3 startPos;
    private Vector2 direction;
    private GunController gunController;

    public void Initialize(Vector2 dir, GunController gc)
    {
        direction = dir.normalized;
        gunController = gc;
        startPos = transform.position;
    }

    // New method to update the bullet's travel direction.
    public void SetPosition(Vector2 newPosition, bool isReverseDirection)
    {
        startPos = newPosition;
        if(isReverseDirection){
            direction = -direction;
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        if (Vector3.Distance(startPos, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If the bullet collides with a portal, let the portal handle it.
        if (other.GetComponent<PortalTeleporter>() != null)
        {
            return;
        }
        
        if (other.CompareTag("Box"))
        {
            gunController.CaptureWithBullet(other.gameObject);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
