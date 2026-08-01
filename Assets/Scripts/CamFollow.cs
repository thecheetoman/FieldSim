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

    [Tooltip("How smoothly the camera catches up to the player. Lower values = faster/snappier.")]
    public float smoothTime = 0.25f;

    private Vector3 currentVelocity = Vector3.zero;
    // start is called before the first frame update
    void Start()
    {
        // find the target object by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);

        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"CamFollow: No object found with tag '{targetTag}'.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // calculate target position in world space using the offset
        Vector3 targetPosition = target.position + offset;

        // smoothly move camera to target position, dont change rotation since that feels erally bad
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}
