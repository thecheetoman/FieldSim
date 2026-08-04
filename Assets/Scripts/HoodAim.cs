using UnityEngine;

public class HoodAim : MonoBehaviour
{
    [Header("Pivot Settings")]
    [Tooltip("The transform responsible for hood pitch (rotates on local Z-axis).")]
    [SerializeField] private Transform hoodPivot;

    [Header("Hood Angle Range")]
    [Tooltip("Hood Z-angle when closest to target.")]
    [SerializeField] private float minHoodAngle = 0f;

    [Tooltip("Hood Z-angle when furthest from target.")]
    [SerializeField] private float maxHoodAngle = 45f;

    [Header("Distance Range")]
    [Tooltip("Distance (in meters) corresponding to minHoodAngle.")]
    [SerializeField] private float minDistance = 2f;

    [Tooltip("Distance (in meters) corresponding to maxHoodAngle.")]
    [SerializeField] private float maxDistance = 10f;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationSpeed = 180f; // Degrees per second
    [SerializeField] private bool isSmooth = true;
    [SerializeField] private float smoothSpeed = 10f;

    [Tooltip("Manual angle offset (in degrees) added to the calculated Z angle.")]
    [SerializeField] private float zOffsetAngle = 0f;

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

        float finalZAngle = calculatedAngle + zOffsetAngle;
        finalZAngle = -finalZAngle;

        // Apply only to Z-axis while keeping X and Y unchanged
        Vector3 currentLocalEuler = hoodPivot.localEulerAngles;
        Quaternion targetRotation = Quaternion.Euler(currentLocalEuler.x, currentLocalEuler.y, finalZAngle);

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