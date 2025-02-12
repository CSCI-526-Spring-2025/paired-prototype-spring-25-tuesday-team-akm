using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    private HashSet<GameObject> objectsInPortal = new HashSet<GameObject>();
    public bool isReverseDirection;

    [SerializeField] private Transform teleportDestination;  // Target location for teleportation

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the object is already in the portal, return.
        if (objectsInPortal.Contains(collision.gameObject))
        {
            return;
        }

        // Check if the colliding object is a bullet.
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            // Teleport the bullet to the destination portal's position.
            collision.transform.position = teleportDestination.position;
            
            // Determine the new desired direction.
            // (Use teleportDestination.right. If that still gives you an undesired result, try -teleportDestination.right.)'
            Debug.Log(teleportDestination.position);
            Debug.Log(teleportDestination.position);
            Vector2 newDir = -teleportDestination.right;
            
            // Update the bullet's internal travel direction.
            // bullet.transform.position = -teleportDestination.position;
            bullet.SetPosition(teleportDestination.position, isReverseDirection);
            // Also update its visual orientation.
            // collision.transform.right = newDir;
            
            // If the bullet has a Rigidbody2D, reset its velocity and disable gravity.
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // rb.velocity = newDir * bullet.speed;
                rb.gravityScale = 0; // Disable gravity so it doesn't fall.
                // Alternatively, you could set its bodyType to Kinematic if you want to completely ignore physics.
                // rb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            // Temporarily disable the bullet's collider so it doesn't immediately re-trigger the portal.
            StartCoroutine(DisableColliderTemporarily(collision, 0.2f));
            return;
        }

        // For non-bullet objects, proceed with normal teleportation.
        if (teleportDestination.TryGetComponent(out PortalTeleporter destinationPortal))
        {
            destinationPortal.objectsInPortal.Add(collision.gameObject);
        }
        collision.transform.position = teleportDestination.position;
        if (collision.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb2))
        {
            rb2.velocity = Vector2.zero;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        objectsInPortal.Remove(collision.gameObject);
    }

    private IEnumerator DisableColliderTemporarily(Collider2D col, float duration)
    {
        col.enabled = false;
        yield return new WaitForSeconds(duration);
        col.enabled = true;
    }
}
