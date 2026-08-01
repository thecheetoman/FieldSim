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


    public void Capture(Transform targetSlot)
    {
        transform.SetParent(targetSlot);
        transform.localPosition = Vector3.zero;
        //transform.localRotation = Quaternion.identity;

        rb.isKinematic = true;

        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

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