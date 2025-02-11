using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{  
    public float FollowSpeed = 2f;
    public float fixedYPosition = 1.5f; // Fixed y-position
    public Transform target;

    public Transform leftWall;  // Assign the left wall object in the Inspector
    public Transform rightWall; // Assign the right wall object in the Inspector
    private float minX;
    private float maxX;

    void Start()
    {
        float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;

        minX = leftWall.position.x + cameraHalfWidth;
        maxX = rightWall.position.x - cameraHalfWidth;
    }

    void Update()
    {
        float targetX = Mathf.Lerp(transform.position.x, target.position.x, FollowSpeed * Time.deltaTime);
        float clampedX = Mathf.Clamp(targetX, minX, maxX);  // Clamp x within the boundaries

        Vector3 newPosition = new Vector3(clampedX, fixedYPosition, -10f);
        transform.position = newPosition;

        // Vector3 newPosition = new Vector3(target.position.x, fixedYPosition, -10f);
        // transform.position = Vector3.Lerp(transform.position, newPosition, FollowSpeed * Time.deltaTime);
    }
}

