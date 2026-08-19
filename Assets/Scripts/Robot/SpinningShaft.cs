using UnityEngine;
using UnityEngine.Events;

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

    [Header("Audio Settings (Optional)")]
    [Tooltip("Drag an AudioSource component here if you want sound effects.")]
    public AudioSource motorAudioSource;

    [Tooltip("Should the audio pitch change based on the rotation speed?")]
    public bool pitchShiftWithSpeed = true;
    public float minPitch = 0.5f;
    public float maxPitch = 1.5f;

    [Tooltip("Target volume when at full speed.")]
    [Range(0f, 1f)] public float maxVolume = 1f;

    [Header("Optional Events")]
    [Tooltip("Called once when the shaft reaches max speed.")]
    public UnityEvent OnMaxSpeedReached;

    [Tooltip("Called once immediately when the shaft begins ramping down (key released).")]
    public UnityEvent OnRampDownStarted;

    private float currentSpeed = 0f;
    private bool isSpinning = false;

    // state tracking flags to ensure events trigger once per state transition
    private bool hasTriggeredMaxSpeed = false;
    private bool hasTriggeredRampDown = true;

    public enum RotationDirection
    {
        Forward,
        Reverse
    }

    private void Start()
    {
        // setup default audio parameters if an AudioSource is assigned to this
        if (motorAudioSource != null)
        {
            motorAudioSource.loop = true;
            motorAudioSource.volume = 0f;

            if (!motorAudioSource.isPlaying && motorAudioSource.clip != null)
            {
                motorAudioSource.Play();
            }
        }
    }
    
    private bool isRobotEnabled = true;

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
            isSpinning = false;
        }
    }
    void Update()
    {
        // check if the key is being held down
        isSpinning = isRobotEnabled && Input.GetKey(rotationKey);

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

        // update audio source
        UpdateAudio();

        // check for speed events
        CheckSpeedEvents();
    }

    private void CheckSpeedEvents()
    {
        // check if reached max speed
        if (Mathf.Approximately(currentSpeed, maxSpinSpeed) && !hasTriggeredMaxSpeed)
        {
            hasTriggeredMaxSpeed = true;
            hasTriggeredRampDown = false; //reset ramp down flag for next stop spinning, idk

            OnMaxSpeedReached?.Invoke();
        }
        // check if key was released and shaft is still spinning
        else if (!isSpinning && !hasTriggeredRampDown && currentSpeed > 0f)
        {
            hasTriggeredRampDown = true;
            hasTriggeredMaxSpeed = false; // reset max speed flag for next spool up

            OnRampDownStarted?.Invoke();
        }
    }

    private void UpdateAudio()
    {
        if (motorAudioSource == null) return;

        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpinSpeed);

        motorAudioSource.volume = Mathf.Lerp(0f, maxVolume, speedRatio);

        if (pitchShiftWithSpeed)
        {
            motorAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
        }
    }
}