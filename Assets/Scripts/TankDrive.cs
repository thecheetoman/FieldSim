using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankDrive : MonoBehaviour
{
    private Vector2 movementInput;
    public Rigidbody rigid;
    public WheelCollider FL, FR, ML, MR, BL, BR;
    public float drivespeed, rotationspeed;
    public float brakeForce = 3000f;

    [Header("Audio Settings")]
    public AudioSource engineAudioSource;
    [Tooltip("The physical speed (m/s) at which audio reaches max pitch and volume.")]
    public float topSpeedForAudio = 10f;
    [Tooltip("Idle pitch frequency.")]
    public float minPitch = 0.6f;
    [Tooltip("Max pitch frequency when accelerating/driving.")]
    public float maxPitch = 1.6f;
    public float audioSmoothSpeed = 8f;

    private void Start()
    {
        if (rigid == null)
        {
            rigid = GetComponent<Rigidbody>();
        }

        // Initialize audio source on start instead of OnMove so it doesn't restart on every input frame
        if (engineAudioSource != null)
        {
            engineAudioSource.loop = true;
            engineAudioSource.volume = 0f;
            engineAudioSource.pitch = minPitch;

            if (!engineAudioSource.isPlaying && engineAudioSource.clip != null)
            {
                engineAudioSource.Play();
            }
        }
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    private void Update()
    {
        UpdateEngineAudio();
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            FL.brakeTorque = brakeForce;
            FR.brakeTorque = brakeForce;
            ML.brakeTorque = brakeForce;
            MR.brakeTorque = brakeForce;
            BL.brakeTorque = brakeForce;
            BR.brakeTorque = brakeForce;

            // stop motors
            ResetMotorTorque();
            return;
        }
        else
        {
            // reset brake torque when not shooting
            ResetBrakeTorque();
        }

        float leftSideTorque = (movementInput.y * drivespeed) + (movementInput.x * rotationspeed);
        float rightSideTorque = (movementInput.y * drivespeed) - (movementInput.x * rotationspeed);

        FL.motorTorque = leftSideTorque;
        ML.motorTorque = leftSideTorque;
        BL.motorTorque = leftSideTorque;

        FR.motorTorque = rightSideTorque;
        MR.motorTorque = rightSideTorque;
        BR.motorTorque = rightSideTorque;
    }

    private void UpdateEngineAudio()
    {
        if (engineAudioSource == null || rigid == null) return;

        // Calculate speed ratio directly from Rigidbody's current physical velocity
        float currentSpeed = rigid.velocity.magnitude;
        float speedRatio = Mathf.Clamp01(currentSpeed / topSpeedForAudio);

        // If braking, force speed ratio to zero (silent)
        if (Input.GetKey(KeyCode.Space))
        {
            speedRatio = 0f;
        }

        // Calculate target volume (0 when stopped, ramps up to maxVolume when moving)
        float targetVolume = Mathf.Lerp(0f, 1f, speedRatio);
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

        // Smoothly ramp volume and pitch
        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, targetVolume, Time.deltaTime * audioSmoothSpeed);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * audioSmoothSpeed);
    }

    private void ResetBrakeTorque()
    {
        FL.brakeTorque = 0f;
        FR.brakeTorque = 0f;
        ML.brakeTorque = 0f;
        MR.brakeTorque = 0f;
        BL.brakeTorque = 0f;
        BR.brakeTorque = 0f;
    }

    private void ResetMotorTorque()
    {
        FL.motorTorque = 0f;
        ML.motorTorque = 0f;
        BL.motorTorque = 0f;
        FR.motorTorque = 0f;
        MR.motorTorque = 0f;
        BR.motorTorque = 0f;
    }
}