using System.Collections.Generic;
using UnityEngine;

public class IntakePathManager : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode intakeKey = KeyCode.Space;

    [Header("Waypoints Path")]
    public List<Transform> intakePathPoints = new List<Transform>();

    [Header("Path Movement & Launch Settings")]
    [Tooltip("How fast the ball moves ALONG the waypoint path inside the robot")]
    public float pathMoveSpeed = 8.0f;

    [Tooltip("The exit speed specifically for balls exiting THIS path")]
    public float pathLaunchVelocity = 2.0f;

    private List<Collider> ballsInRange = new List<Collider>();

    private void Update()
    {
        if (Input.GetKey(intakeKey))
        {
            ProcessBallsInRange();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GamePiece") && !ballsInRange.Contains(other))
        {
            ballsInRange.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ballsInRange.Contains(other))
        {
            ballsInRange.Remove(other);
        }
    }

    private void ProcessBallsInRange()
    {
        for (int i = ballsInRange.Count - 1; i >= 0; i--)
        {
            Collider ballCollider = ballsInRange[i];

            if (ballCollider != null)
            {
                FuelFollower follower = ballCollider.GetComponent<FuelFollower>();
                if (follower == null)
                {
                    follower = ballCollider.gameObject.AddComponent<FuelFollower>();
                }

                // Pass the path waypoints, the movement speed through the path, AND the exit launch speed
                follower.StartPathSequence(intakePathPoints, pathMoveSpeed, pathLaunchVelocity);

                ballsInRange.RemoveAt(i);
            }
        }
    }
}