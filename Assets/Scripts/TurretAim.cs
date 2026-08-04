using UnityEngine;

public class TurretAim : MonoBehaviour
{
    [Header("Pivot Settings")]
    [Tooltip("The transform that rotates on the Z axis.")]
    [SerializeField] private Transform yawPivot;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationSpeed = 360f; // Degrees per second
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
        // Subscribe to robot state changed event from GameManager
        GameManager.OnRobotStateChanged += HandleRobotStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from event when component/object is disabled to prevent memory leaks
        GameManager.OnRobotStateChanged -= HandleRobotStateChanged;
    }

    private void Start()
    {
        // Cache target references using tags
        GameObject hubObj = GameObject.FindWithTag("Hub");
        if (hubObj != null) hubTarget = hubObj.transform;

        GameObject passLeftObj = GameObject.FindWithTag("Passleft");
        if (passLeftObj != null) passLeftTarget = passLeftObj.transform;

        GameObject passRightObj = GameObject.FindWithTag("Passright");
        if (passRightObj != null) passRightTarget = passRightObj.transform;

        // Default to Hub target on start
        currentTarget = hubTarget;
    }

    private void Update()
    {
        // Only allow targeting and input when robot is enabled
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
        else if (Input.GetKeyDown(KeyCode.J))
        {
            if (passLeftTarget != null) currentTarget = passLeftTarget;
        }
        else if (Input.GetKeyDown(KeyCode.L))
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

        float baseAngle = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg;
        float finalZAngle = baseAngle + zOffsetAngle;

        Vector3 currentLocalEuler = yawPivot.localEulerAngles;
        Quaternion targetRotation = Quaternion.Euler(currentLocalEuler.x, currentLocalEuler.y, finalZAngle);

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