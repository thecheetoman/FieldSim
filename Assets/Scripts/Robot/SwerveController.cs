using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwerveController : MonoBehaviour
{
    [System.Serializable]
    public class ModuleContact
    {
        [HideInInspector] public Transform contactTransform;
        [HideInInspector] public SphereCollider contactCollider;
    }

    [Header("Module Contact Points")]
    [Tooltip("Local offset from the robot root for each corner's contact sphere. Y should sit near chassis-bottom height, roughly where a real wheel would touch the ground.")]
    public Vector3 frontLeftOffset = new Vector3(-0.3f, -0.2f, 0.3f);
    public Vector3 frontRightOffset = new Vector3(0.3f, -0.2f, 0.3f);
    public Vector3 backLeftOffset = new Vector3(-0.3f, -0.2f, -0.3f);
    public Vector3 backRightOffset = new Vector3(0.3f, -0.2f, -0.3f);
    public float moduleRadius = 0.06f;
    public PhysicMaterial moduleMaterial;
    [Tooltip("If your chassis has a BoxCollider, its bottom is raised above the module spheres at Awake() so it doesn't snag on bumps, but stays active for walls, other robots, and to stop the mesh clipping through the ground during a tilt or hit. Recommended over fully disabling colliders.")]
    public bool raiseBodyColliderAboveModules = true;
    [Tooltip("Extra gap (meters) between the raised body collider's bottom and the module spheres' lowest point.")]
    public float bodyColliderClearance = 0.03f;

    [Header("Speeds")]
    public float maxSpeed = 4.5f;
    public float maxAngularSpeed = 360f;
    public float driveForceGain = 20f;
    public float maxForcePerModule = 1500f;

    [Header("Input")]
    public bool fieldRelative = true;
    public float inputDeadzone = 0.05f;

    [Header("Stability (driving over bumps/ramps)")]
    [Tooltip("Local Y offset for the rigidbody's center of mass. Should sit near the floor of the robot. More negative = lower = more tip-resistant, but also suppresses visible tilt when allowTilt is true.")]
    public float centerOfMassHeight = -0.08f;
    [Tooltip("false = X/Z rotation fully locked, tipping is physically impossible. true = robot can pitch/roll slightly while cresting the bump but self-rights via a stabilizing torque.")]
    public bool allowTilt = false;
    [Tooltip("Lower = slower, more visible tilt before it corrects. Higher = snaps upright almost instantly.")]
    public float uprightTorqueGain = 15f;
    [Tooltip("Lower = more rocking/oscillation before it settles. Higher = corrects smoothly with less overshoot.")]
    public float uprightDamping = 2f;
    public float maxTiltAngle = 25f;

    [Header("Audio Settings")]
    public AudioSource engineAudioSource;
    [Range(0f, 1f)] public float maxVolume = 0.8f;
    public float topSpeedForAudio = 4.5f;
    public float minPitch = 0.7f;
    public float maxPitch = 1.4f;
    public float audioSmoothSpeed = 10f;
    public float fadeOutSpeed = 8f;

    public enum GyroZeroMode
    {
        RobotSpawnRotation,
        ReferenceTransform,
        WorldNegativeX
    }

    [Header("Gyro Reference")]
    [Tooltip("RobotSpawnRotation: zero is wherever this robot happened to spawn facing. ReferenceTransform: zero is measured against Gyro Reference (or a tagged object). WorldNegativeX: zero is always the world's -X axis, regardless of spawn rotation or any reference object.")]
    public GyroZeroMode gyroZeroMode = GyroZeroMode.WorldNegativeX;
    [Tooltip("Used when Gyro Zero Mode is ReferenceTransform. If set, heading is measured relative to this Transform's rotation instead of the robot's own rotation at spawn.")]
    public Transform gyroReference;
    [Tooltip("Used when Gyro Zero Mode is ReferenceTransform and Gyro Reference is empty. Looks for a GameObject with this tag at Awake() and uses it as the reference.")]
    public string gyroReferenceTag = "";

    private Rigidbody rb;
    private Quaternion spawnRotation;
    private float gyroTrimDegrees;
    private ModuleContact[] modules;
    private Vector3[] moduleLocalOffsets;
    private bool isRobotEnabled = true;

    private float cachedXInput;
    private float cachedYInput;
    private float cachedRotInput;

    private float currentAudioVolume = 0f;

    private void OnEnable()
    {
        GameManager.OnRobotStateChanged += HandleRobotStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnRobotStateChanged -= HandleRobotStateChanged;
    }

    private void HandleRobotStateChanged(bool enabledState)
    {
        isRobotEnabled = enabledState;

        if (!isRobotEnabled)
        {
            StopAllDriveForces();
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0f, centerOfMassHeight, 0f);
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (!allowTilt)
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (raiseBodyColliderAboveModules)
        {
            RaiseBodyBoxCollidersAboveModules();
        }

        moduleLocalOffsets = new[] { frontLeftOffset, frontRightOffset, backLeftOffset, backRightOffset };
        string[] names = { "Contact_FL", "Contact_FR", "Contact_BL", "Contact_BR" };

        modules = new ModuleContact[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject go = new GameObject(names[i]);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = moduleLocalOffsets[i];

            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.radius = moduleRadius;
            if (moduleMaterial != null) sc.sharedMaterial = moduleMaterial;

            modules[i] = new ModuleContact { contactTransform = go.transform, contactCollider = sc };
        }

        if (gyroZeroMode == GyroZeroMode.ReferenceTransform && gyroReference == null && !string.IsNullOrEmpty(gyroReferenceTag))
        {
            GameObject found = GameObject.FindGameObjectWithTag(gyroReferenceTag);
            if (found != null) gyroReference = found.transform;
        }

        spawnRotation = transform.rotation;
        ZeroGyro();
    }

    private void RaiseBodyBoxCollidersAboveModules()
    {
        float lowestModulePoint = Mathf.Min(
            Mathf.Min(frontLeftOffset.y, frontRightOffset.y),
            Mathf.Min(backLeftOffset.y, backRightOffset.y)
        ) - moduleRadius;

        float desiredBottomY = lowestModulePoint + bodyColliderClearance;
        bool foundBoxCollider = false;

        foreach (var bc in GetComponents<BoxCollider>())
        {
            foundBoxCollider = true;
            float currentBottom = bc.center.y - bc.size.y * 0.5f;
            float currentTop = bc.center.y + bc.size.y * 0.5f;

            if (desiredBottomY > currentBottom)
            {
                float newHeight = Mathf.Max(0.02f, currentTop - desiredBottomY);
                bc.size = new Vector3(bc.size.x, newHeight, bc.size.z);
                bc.center = new Vector3(bc.center.x, currentTop - newHeight * 0.5f, bc.center.z);
            }
        }

        if (!foundBoxCollider && GetComponents<Collider>().Length > 0)
        {
            Debug.LogWarning(
                $"{name}: SwerveDriveController found a non-BoxCollider on the chassis. " +
                "It can't auto-raise it above the module spheres - consider using a BoxCollider, " +
                "or manually shrink/reposition it above the module contact points.", this);
        }
    }

    void Start()
    {
        if (engineAudioSource != null)
        {
            engineAudioSource.loop = true;
            engineAudioSource.playOnAwake = false;
            engineAudioSource.volume = 0f;
            if (!engineAudioSource.isPlaying) engineAudioSource.Play();
        }
    }

    void Update()
    {
        UpdateEngineAudio();

        cachedXInput = -1f * Input.GetAxis("Vertical");
        cachedYInput = Input.GetAxis("Horizontal");

        cachedRotInput = 0f;
        if (Input.GetKey(KeyCode.L)) cachedRotInput = -1f;
        if (Input.GetKey(KeyCode.J)) cachedRotInput = 1f;

        if (Mathf.Abs(cachedXInput) < inputDeadzone) cachedXInput = 0f;
        if (Mathf.Abs(cachedYInput) < inputDeadzone) cachedYInput = 0f;
    }

    private void UpdateEngineAudio()
    {
        if (engineAudioSource == null || rb == null) return;

        if (!isRobotEnabled)
        {
            currentAudioVolume = Mathf.MoveTowards(currentAudioVolume, 0f, Time.deltaTime * fadeOutSpeed);
            engineAudioSource.volume = currentAudioVolume;
            return;
        }

        Vector3 vel = rb.velocity;
        float linearSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
        float angularSpeedRatio = Mathf.Abs(rb.angularVelocity.y) / (maxAngularSpeed * Mathf.Deg2Rad);

        float normalizedLinear = topSpeedForAudio > 0f ? Mathf.Clamp01(linearSpeed / topSpeedForAudio) : 0f;
        float moveRatio = Mathf.Clamp01(normalizedLinear + (angularSpeedRatio * 0.5f));

        if (moveRatio < 0.02f)
        {
            currentAudioVolume = Mathf.MoveTowards(currentAudioVolume, 0f, Time.deltaTime * fadeOutSpeed);
        }
        else
        {
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, moveRatio);
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * audioSmoothSpeed);

            float targetVolume = Mathf.Lerp(0f, maxVolume, moveRatio);
            currentAudioVolume = Mathf.Lerp(currentAudioVolume, targetVolume, Time.deltaTime * audioSmoothSpeed);
        }

        engineAudioSource.volume = currentAudioVolume;
    }

    void FixedUpdate()
    {
        if (isRobotEnabled)
        {
            Drive(cachedXInput, cachedYInput, cachedRotInput);
        }

        if (allowTilt) StabilizeUpright();
    }

    public void Drive(float xSpeed, float ySpeed, float rot)
    {
        if (!isRobotEnabled) return;

        float vx = xSpeed * maxSpeed;
        float vy = ySpeed * maxSpeed;
        float omega = rot * maxAngularSpeed * Mathf.Deg2Rad;

        if (fieldRelative)
        {
            float heading = GetGyroYaw() * Mathf.Deg2Rad;
            float cos = Mathf.Cos(heading);
            float sin = Mathf.Sin(heading);
            float fieldVx = vx * cos - vy * sin;
            float fieldVy = vx * sin + vy * cos;
            vx = fieldVx;
            vy = fieldVy;
        }

        ApplyModuleForces(vx, vy, omega);
    }

    private void ApplyModuleForces(float vx, float vy, float omega)
    {
        if (modules == null) return;

        float perModuleGain = driveForceGain * rb.mass / modules.Length;
        Vector3 comWorld = rb.worldCenterOfMass;

        for (int i = 0; i < modules.Length; i++)
        {
            Vector3 worldPos = modules[i].contactTransform.position;

            Vector3 rel = worldPos - comWorld;
            float rx = rel.x;
            float rz = rel.z;

            float targetVx = vx - omega * rz;
            float targetVz = vy + omega * rx;
            Vector3 targetVel = new Vector3(targetVx, 0f, targetVz);

            Vector3 pointVel = rb.GetPointVelocity(worldPos);
            Vector3 planarPointVel = new Vector3(pointVel.x, 0f, pointVel.z);

            Vector3 velError = targetVel - planarPointVel;
            Vector3 force = Vector3.ClampMagnitude(velError * perModuleGain, maxForcePerModule);

            rb.AddForceAtPosition(force, worldPos, ForceMode.Force);
        }
    }

    private void StopAllDriveForces()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void StabilizeUpright()
    {
        Vector3 correctionAxis = Vector3.Cross(transform.up, Vector3.up);
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        Vector3 torque = correctionAxis * uprightTorqueGain;

        Vector3 angVel = rb.angularVelocity;
        Vector3 yawOnly = Vector3.up * angVel.y;
        Vector3 rollPitchVel = angVel - yawOnly;
        torque -= rollPitchVel * uprightDamping;

        rb.AddTorque(torque, ForceMode.Force);

        if (tiltAngle > maxTiltAngle)
            rb.angularVelocity = yawOnly;
    }

    private float ComputeRawYawDegrees()
    {
        Quaternion baseRotation;
        switch (gyroZeroMode)
        {
            case GyroZeroMode.ReferenceTransform:
                baseRotation = gyroReference != null ? gyroReference.rotation : spawnRotation;
                break;
            case GyroZeroMode.WorldNegativeX:
                baseRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
                break;
            default:
                baseRotation = spawnRotation;
                break;
        }

        Quaternion relative = Quaternion.Inverse(baseRotation) * transform.rotation;
        float yaw = relative.eulerAngles.y;
        if (yaw > 180f) yaw -= 360f;
        return yaw;
    }

    public float GetGyroYaw()
    {
        float yaw = ComputeRawYawDegrees() - gyroTrimDegrees;
        if (yaw > 180f) yaw -= 360f;
        if (yaw < -180f) yaw += 360f;
        return yaw;
    }

    public void ZeroGyro()
    {
        gyroTrimDegrees = ComputeRawYawDegrees();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3[] offsets = { frontLeftOffset, frontRightOffset, backLeftOffset, backRightOffset };
        foreach (var o in offsets)
        {
            Vector3 worldPos = transform.TransformPoint(o);
            Gizmos.DrawWireSphere(worldPos, moduleRadius);
        }
    }
}