using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GamePiece : MonoBehaviour
{
    private Rigidbody rb;
    private Collider[] colliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    /// <summary>
    /// Snaps the ball into a slot, disables physics, and disables colliders.
    /// </summary>
    public void Capture(Transform targetSlot)
    {
        transform.SetParent(targetSlot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        rb.isKinematic = true;

        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    /// <summary>
    /// Teleports ball to launch point, re-enables physics/colliders, and launches it.
    /// </summary>
    public void Launch(Vector3 launchPosition, Quaternion launchRotation, Vector3 launchVelocity)
    {
        transform.SetParent(null);
        transform.position = launchPosition;
        transform.rotation = launchRotation;

        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        rb.isKinematic = false;
        rb.velocity = launchVelocity;
    }
}