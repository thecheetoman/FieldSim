using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubFuelSFX : MonoBehaviour
{
    // get the audio source =0
    public AudioSource audioSource;

    [Header("Randomization Settings")]
    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

    [Header("Impact Force Settings")]
    [Tooltip("Optional: Adjust volume based on how hard the ball impacts.")]
    public bool scaleVolumeWithImpact = true;
    public float minImpactForce = 1f;
    public float maxImpactForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("GamePiece"))
        {
            PlayImpactSFX(collision);
        }
    }

    private void PlayImpactSFX(Collision collision)
    {
        if (audioSource == null) return;

        // pitch randomization
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        // scale volume based on impact force
        if (scaleVolumeWithImpact)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            float volumeRatio = Mathf.InverseLerp(minImpactForce, maxImpactForce, impactForce);
            audioSource.volume = Mathf.Clamp(volumeRatio, 0.2f, 1.0f);
        }

        // play one shot, stops any currently playing audio and plays the new clip
        if (audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}