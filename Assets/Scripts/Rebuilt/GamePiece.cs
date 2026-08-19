using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GamePiece : MonoBehaviour
{
    private Rigidbody rb;
    private Collider[] colliders;

    // Tracks if this specific game piece was launched from a valid/legal area
    public bool WasShotFromLegalZone { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void Capture(Transform targetSlot)
    {
        transform.SetParent(targetSlot);
        transform.localPosition = Vector3.zero;

        rb.isKinematic = true;

        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    // Added shooterPosition parameter to evaluate validity based on robot location
    public void Launch(Vector3 launchPosition, Quaternion launchRotation, Vector3 launchVelocity, Vector3 shooterPosition)
    {
        // 1. Evaluate legal shot criteria AT THE MOMENT OF LAUNCH
        WasShotFromLegalZone = shooterPosition.x >= 3.429f;

        // 2. Perform regular launch logic
        transform.SetParent(null);
        transform.position = launchPosition;
        transform.rotation = launchRotation;

        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        rb.isKinematic = false;
        rb.velocity = launchVelocity; // Note: Use rb.velocity in older Unity versions
    }
}