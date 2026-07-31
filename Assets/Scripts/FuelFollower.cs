using System.Collections.Generic;
using UnityEngine;

public class FuelFollower : MonoBehaviour
{
    private Rigidbody rb;
    private List<Transform> waypoints = new List<Transform>();

    private float dynamicMoveSpeed = 6.0f;
    private float dynamicLaunchSpeed = 1.5f;
    public float rollSpeed = 500.0f;

    private int currentWaypointIndex = 0;
    private bool isFollowingPath = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // get transform of all of the points, movement speed through the points and exit speed at the end
    public void StartPathSequence(List<Transform> pathPoints, float moveSpeed, float launchSpeed)
    {
        if (pathPoints == null || pathPoints.Count == 0) return;

        waypoints = pathPoints;
        dynamicMoveSpeed = moveSpeed;     // this variable holds the speed at which the ball moves along the path
        dynamicLaunchSpeed = launchSpeed; // this holds the speed at which the ball is launched out of the path
        currentWaypointIndex = 0; // set to the first waypoint

        // disable physics while on path
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        // disable collider. Useful for if balls leak out of area(kitbot intake), prevents collision during path following
        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider != null)
        {
            ballCollider.enabled = false;
        }

        transform.position = waypoints[0].position;
        isFollowingPath = true;
    }

    private void Update()
    {
        if (!isFollowingPath || waypoints == null || waypoints.Count == 0) return;

        Transform targetPoint = waypoints[currentWaypointIndex];

        // move towards the target position using the movementspeed
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            dynamicMoveSpeed * Time.deltaTime
        );

        // roll animation(you can barely see this idk why im leaving it here tho)
        Vector3 moveDirection = (targetPoint.position - transform.position).normalized;
        if (moveDirection != Vector3.zero)
        {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, moveDirection);
            transform.Rotate(rotationAxis, rollSpeed * Time.deltaTime, Space.World);
        }

        // check distance to the next waypoint, if close enough, move to the next waypoint
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Count)
            {
                isFollowingPath = false;
                LaunchBall(targetPoint.forward, dynamicLaunchSpeed);
            }
        }
    }

    // launch the ball when it reaches the end of the path
    public void LaunchBall(Vector3 launchDirection, float speed)
    {
        isFollowingPath = false;
        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider != null)
        {
            ballCollider.enabled = true;
        }
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = launchDirection.normalized * speed;
        }
    }
}