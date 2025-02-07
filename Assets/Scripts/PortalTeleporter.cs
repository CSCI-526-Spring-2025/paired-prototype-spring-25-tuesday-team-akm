// using UnityEngine;

// public class PortalTeleporter : MonoBehaviour
// {
//     public Transform teleportDestination;  // Target location for teleportation

//     // This method is called when another collider enters the trigger
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         // Check if the object entering the destinationPortal is the box sprite
//         if (other.CompareTag("Player") || other.CompareTag("Box"))
//         {
//             // Teleport the box sprite to the teleport destination
//             other.transform.position = teleportDestination.position;
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    private HashSet<GameObject> objectsInPortal = new HashSet<GameObject>();

    [SerializeField] private Transform teleportDestination;  // Target location for teleportation

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the object is already in the portal, return
        if(objectsInPortal.Contains(collision.gameObject))
        {
            return;
        }

        // Check if the destination object also has this script attached to it, if so add to hashset
        if(teleportDestination.TryGetComponent(out PortalTeleporter destinationPortal))
        {
            destinationPortal.objectsInPortal.Add(collision.gameObject);
        }

        // Teleport the object to the teleport destination
        collision.transform.position = teleportDestination.position;

        // Reset the object's velocity if it has a Rigidbody2D
        if (collision.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Remove the object from the hashset when it exits the portal
        objectsInPortal.Remove(collision.gameObject);
    }
}