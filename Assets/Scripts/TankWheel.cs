using UnityEngine;

public class TankWheel : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;

    void Update()
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);

        wheelMesh.position = pos;
        // multiply by your mesh's default local rotation offset (e.g., 90 degrees on Y or Z)
        wheelMesh.Rotate(wheelCollider.rpm / 60f * 360f * Time.deltaTime, 0f, 0f, Space.Self);
    }
}