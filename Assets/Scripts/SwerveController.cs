using UnityEngine;
using UnityEngine.InputSystem;

public class SwerveController : MonoBehaviour
{
    private Vector2 moveInput;
    private float rotateInput;
    public Rigidbody rb;

    [Header("Settings")]
    public float maxSpeed = 4.0f; // meters per second
    public float maxAngularSpeed = 180.0f; // degrees per second

    // called when movement inputs, assigned via project settings
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        // moveInput.y = robot Forward/Backward
        // moveInput.x = robot Left/Right
    }

    public void OnRotate(InputValue value)
    {
        rotateInput = value.Get<float>();
        // rotateInput = robot Rotation rate
    }

    private void FixedUpdate()
    {
        // get raw robot velocity
        float fieldVx = moveInput.x * maxSpeed; // Positive = Field Right (+X)
        float fieldVz = moveInput.y * maxSpeed; // Positive = Field Forward (+Z)
        float omega = rotateInput * maxAngularSpeed; // Degrees per second

        // field oriented robot rotation
        // convert field relative movement to robot relative movement. This u
        float headingAngleRad = transform.eulerAngles.y * Mathf.Deg2Rad;

        // rotation matrix something for field centric rotation
        float robotVx = fieldVx * Mathf.Cos(headingAngleRad) - fieldVz * Mathf.Sin(headingAngleRad);
        float robotVz = fieldVx * Mathf.Sin(headingAngleRad) + fieldVz * Mathf.Cos(headingAngleRad);

        // apply local velocity
        Vector3 localVelocity = new Vector3(-robotVz, -robotVx, rb.velocity.y);
        rb.velocity = transform.TransformDirection(localVelocity);

        // apply rotation
        rb.angularVelocity = new Vector3(0f, omega * Mathf.Deg2Rad, 0f);
    }
}