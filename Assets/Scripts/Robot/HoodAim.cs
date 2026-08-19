using UnityEngine;

public class HoodAim : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Pivot Settings")]
    [Tooltip("The transform responsible for hood pitch.")]
    [SerializeField] private Transform hoodPivot;

    [Tooltip("Select which local axis controls the hood angle.")]
    [SerializeField] private RotationAxis activeAxis = RotationAxis.Z;

    [Header("Hood Angle Range")]
    [Tooltip("Hood angle when closest to target.")]
    [SerializeField] private float minHoodAngle = 0f;

    [Tooltip("Hood angle when furthest from target.")]
    [SerializeField] private float maxHoodAngle = 45f;

    [Tooltip("Invert calculated angle (negate value). Defaults to true.")]
    [SerializeField] private bool invertAngle = true;

    [Header("Distance Range")]
    [Tooltip("Distance (in meters) corresponding to minHoodAngle.")]
    [SerializeField] private float minDistance = 2f;

    [Tooltip("Distance (in meters) corresponding to maxHoodAngle.")]
    [SerializeField] private float maxDistance = 10f;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationSpeed = 180f; // Degrees per second
    [SerializeField] private bool isSmooth = true;
    [SerializeField] private float smoothSpeed = 10f;

    [Tooltip("Manual angle offset (in degrees) added to the calculated angle.")]
    [SerializeField] private float angleOffset = 0f;

    private Transform currentTarget;
    private Transform hubTarget;
    private Transform passLeftTarget;
    private Transform passRightTarget;

    private bool isRobotEnabled = false;

    private void OnEnable()
    {
        GameManager.OnRobotStateChanged += HandleRobotStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnRobotStateChanged -= HandleRobotStateChanged;
    }

    private void Start()
    {
        GameObject hubObj = GameObject.FindWithTag("Hub");
        if (hubObj != null) hubTarget = hubObj.transform;

        GameObject passLeftObj = GameObject.FindWithTag("Passleft");
        if (passLeftObj != null) passLeftTarget = passLeftObj.transform;

        GameObject passRightObj = GameObject.FindWithTag("Passright");
        if (passRightObj != null) passRightTarget = passRightObj.transform;

        currentTarget = hubTarget;
    }

    private void Update()
    {
        if (!isRobotEnabled) return;

        HandleInput();

        if (currentTarget != null && hoodPivot != null)
        {
            AlignHoodToTarget();
        }
    }

    private void HandleRobotStateChanged(bool enabledState)
    {
        isRobotEnabled = enabledState;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (hubTarget != null) currentTarget = hubTarget;
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            if (passLeftTarget != null) currentTarget = passLeftTarget;
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            if (passRightTarget != null) currentTarget = passRightTarget;
        }
    }

    private void AlignHoodToTarget()
    {
        // Measure horizontal distance to target
        Vector3 distanceVector = currentTarget.position - hoodPivot.position;
        distanceVector.y = 0f;
        float distance = distanceVector.magnitude;

        // Map distance linearly between minDistance and maxDistance
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float calculatedAngle = Mathf.Lerp(minHoodAngle, maxHoodAngle, t);

        float finalAngle = calculatedAngle + angleOffset;

        // Invert if bool is set (defaults to true)
        if (invertAngle)
        {
            finalAngle = -finalAngle;
        }

        Vector3 currentLocalEuler = hoodPivot.localEulerAngles;
        Quaternion targetRotation = hoodPivot.localRotation;

        // Apply calculated angle to selected axis while preserving others
        switch (activeAxis)
        {
            case RotationAxis.X:
                targetRotation = Quaternion.Euler(finalAngle, currentLocalEuler.y, currentLocalEuler.z);
                break;

            case RotationAxis.Y:
                targetRotation = Quaternion.Euler(currentLocalEuler.x, finalAngle, currentLocalEuler.z);
                break;

            case RotationAxis.Z:
                targetRotation = Quaternion.Euler(currentLocalEuler.x, currentLocalEuler.y, finalAngle);
                break;
        }

        if (isSmooth)
        {
            hoodPivot.localRotation = Quaternion.Slerp(hoodPivot.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            hoodPivot.localRotation = Quaternion.RotateTowards(hoodPivot.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}