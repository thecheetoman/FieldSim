using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public string targetTag = "Player";
    private Transform target;

    [Header("Follow Settings")]
    [Tooltip("Offset position relative to the player in world space.")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Tooltip("How smoothly the camera catches up to the player position. Lower values = faster/snappier.")]
    public float smoothTime = 0.25f;

    [Tooltip("How fast the camera rotates when flipped.")]
    public float rotationSpeed = 5f;

    private Vector3 currentVelocity = Vector3.zero;
    private bool isFlipped = false;
    private Quaternion targetRotation;

    // store the original pitch (X angle) so it never gets modified
    private float pitchX;

    void Start()
    {
        // Find the target object by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);

        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"CamFollow: No object found with tag '{targetTag}'.");
        }

        // Save initial pitch angle (e.g., 24) and starting rotation
        pitchX = transform.eulerAngles.x;
        targetRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // toggle cam direction when tab pressed
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isFlipped = !isFlipped;

            float targetY = transform.eulerAngles.y + 180f;
            targetRotation = Quaternion.Euler(pitchX, targetY, 0f);
        }

        Vector3 currentOffset = offset;
        if (isFlipped)
        {
            currentOffset.x = -offset.x;
            currentOffset.z = -offset.z;
        }

        Vector3 targetPosition = target.position + currentOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}