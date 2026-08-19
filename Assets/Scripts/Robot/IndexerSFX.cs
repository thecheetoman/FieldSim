using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class IndexerSFX : MonoBehaviour
{
    [Header("Audio Setup")]
    [SerializeField] private AudioSource indexerAudioSource;
    [SerializeField] private AudioClip indexerLoopClip;

    [Header("Volume Control")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 0.5f; // Scales overall loudness
    [Range(0f, 1f)][SerializeField] private float normalVolume = 0.4f; // Volume when running normally
    [Range(0f, 1f)][SerializeField] private float activeVolume = 0.6f; // Volume when shooting

    [Header("Pitch Control")]
    [Tooltip("Base pitch when the indexer is running normally.")]
    [SerializeField] private float basePitch = 1.0f;

    [Tooltip("Target pitch when shooting (Space is pressed).")]
    [SerializeField] private float activePitch = 1.25f;

    [Tooltip("How smoothly pitch and volume transition between states.")]
    [SerializeField] private float transitionSpeed = 8.0f;

    [Header("Input Key")]
    [SerializeField] private KeyCode shootKey = KeyCode.Space;

    private bool isRobotEnabled = false;
    private float targetPitch;
    private float targetVolume;

    private void Awake()
    {
        if (indexerAudioSource == null)
        {
            indexerAudioSource = GetComponent<AudioSource>();
        }

        // Setup AudioSource for continuous loop
        indexerAudioSource.clip = indexerLoopClip;
        indexerAudioSource.loop = true;
        indexerAudioSource.playOnAwake = false;
        indexerAudioSource.volume = 0f; // Start silent until robot is enabled

        targetPitch = basePitch;
        targetVolume = normalVolume * masterVolume;
        indexerAudioSource.pitch = basePitch;

        indexerAudioSource.Play();
    }

    private void OnEnable()
    {
        // Subscribe to robot state changes from GameManager
        GameManager.OnRobotStateChanged += HandleRobotStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        GameManager.OnRobotStateChanged -= HandleRobotStateChanged;
    }

    private void Update()
    {
        // Only process pitch & volume changes if the robot is active
        if (isRobotEnabled)
        {
            if (Input.GetKey(shootKey))
            {
                targetPitch = activePitch;
                targetVolume = activeVolume * masterVolume;
            }
            else
            {
                targetPitch = basePitch;
                targetVolume = normalVolume * masterVolume;
            }
        }
        else
        {
            targetVolume = 0f;
        }

        // Smoothly interpolate current pitch and volume to their target values
        indexerAudioSource.pitch = Mathf.Lerp(indexerAudioSource.pitch, targetPitch, Time.deltaTime * transitionSpeed);
        indexerAudioSource.volume = Mathf.Lerp(indexerAudioSource.volume, targetVolume, Time.deltaTime * transitionSpeed);
    }

    private void HandleRobotStateChanged(bool enabledState)
    {
        isRobotEnabled = enabledState;

        if (!isRobotEnabled)
        {
            targetPitch = basePitch;
            targetVolume = 0f;
        }
    }
}