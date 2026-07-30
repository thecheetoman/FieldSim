using UnityEngine;

public class SpinningShaft : MonoBehaviour
{
    [Header("Target Mesh")]
    public Transform shaftMesh;

    [Header("Shaft Name(use for labeling purposes, doesnt do anything)")]
    public string shaftName;

    [Header("Spin Settings")]
    public float maxSpinSpeed = 500f;
    public float acceleration = 7200f;
    public float deceleration = 14400f;
    public Vector3 rotationAxis = Vector3.forward;

    [Header("Input Settings")]
    public KeyCode rotationKey = KeyCode.Space;
    public RotationDirection direction = RotationDirection.Forward;

    private float currentSpeed = 0f;
    private bool isSpinning = false;

    // enum for rotation direction
    public enum RotationDirection
    {
        Forward,
        Reverse
    }

    void Update()
    {
        // check if the key is being held down
        if (Input.GetKey(rotationKey))
        {
            isSpinning = true;
        }
        else
        {
            isSpinning = false;
        }

        if (shaftMesh == null) return;

        float targetSpeed = isSpinning ? maxSpinSpeed : 0f;
        float rate = isSpinning ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        if (currentSpeed > 0.1f)
        {
            // apply direction multiplier
            float directionMultiplier = direction == RotationDirection.Forward ? 1f : -1f;
            shaftMesh.Rotate(rotationAxis * currentSpeed * directionMultiplier * Time.deltaTime, Space.Self);
        }

        // Debug.Log($"Speed: {currentSpeed:F0} | Spinning: {isSpinning} | Direction: {direction}");
    }
}