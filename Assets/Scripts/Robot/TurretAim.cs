using UnityEngine;

public class TurretAim : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Pivot Settings")]
    [Tooltip("The transform that rotates to face the target.")]
    [SerializeField] private Transform yawPivot;

    [Tooltip("Select which local axis rotates toward the target.")]
    [SerializeField] private RotationAxis activeAxis = RotationAxis.Z;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationSpeed = 360f; // Degrees per second
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

        if (currentTarget != null && yawPivot != null)
        {
            AlignTurretToTarget();
        }
    }

    private void HandleRobotStateChanged(bool enabledState)
    {
        isRobotEnabled = enabledState;
        Debug.Log($"[TurretAim] Robot state updated. Enabled: {isRobotEnabled}");
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (hubTarget != null) currentTarget = hubTarget;
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            if (passLeftTarget != null) currentTarget = passLeftTarget;
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            if (passRightTarget != null) currentTarget = passRightTarget;
        }
    }

    private void AlignTurretToTarget()
    {
        Vector3 worldDirection = currentTarget.position - yawPivot.position;

        Vector3 localDirection = yawPivot.parent != null
            ? yawPivot.parent.InverseTransformDirection(worldDirection)
            : worldDirection;

        Vector3 currentLocalEuler = yawPivot.localEulerAngles;
        Quaternion targetRotation = yawPivot.localRotation;

        switch (activeAxis)
        {
            case RotationAxis.X:
                float xAngle = (Mathf.Atan2(localDirection.z, localDirection.y) * Mathf.Rad2Deg) + angleOffset;
                targetRotation = Quaternion.Euler(xAngle, currentLocalEuler.y, currentLocalEuler.z);
                break;

            case RotationAxis.Y:
                float yAngle = (Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg) + angleOffset;
                targetRotation = Quaternion.Euler(currentLocalEuler.x, yAngle, currentLocalEuler.z);
                break;

            case RotationAxis.Z:
                float zAngle = (Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg) + angleOffset;
                targetRotation = Quaternion.Euler(currentLocalEuler.x, currentLocalEuler.y, zAngle);
                break;
        }

        if (isSmooth)
        {
            yawPivot.localRotation = Quaternion.Slerp(yawPivot.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            yawPivot.localRotation = Quaternion.RotateTowards(yawPivot.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}