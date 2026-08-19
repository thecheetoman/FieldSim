using System.Collections;
using UnityEngine;

public class RSL : MonoBehaviour
{
    [SerializeField] private Material RSLOn;
    [SerializeField] private Material RSLOff;
    [SerializeField] private float flashInterval = 0.5f; // time between blinks

    private MeshRenderer meshRenderer;
    private Material[] materialsCache;
    private Coroutine strobeCoroutine;
    private bool isOn = false;
    // track robot enabled state
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
            startRSL();
        }
        else
        {
            stopRSL();
        }
    }
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        materialsCache = meshRenderer.materials;

        startRSL();
    }
    public void startRSL()
    {
        // Prevent starting multiple loops at the same time
        if (strobeCoroutine == null)
        {
            strobeCoroutine = StartCoroutine(StrobeRoutine());
        }
    }
    public void stopRSL()
    {
        if (strobeCoroutine != null)
        {
            StopCoroutine(strobeCoroutine);
            strobeCoroutine = null; 

            // after stopping the rsl, the robot is enabled, so the rsl should be on
            materialsCache[0] = RSLOn;
            meshRenderer.materials = materialsCache;
            isOn = false;
        }
    }

    IEnumerator StrobeRoutine()
    {
        while (true)
        {
            // toggle the state
            isOn = !isOn;

            // swap index 0(i think this is the strobing part of the RSL)
            materialsCache[0] = isOn ? RSLOn : RSLOff;

            // reassign
            meshRenderer.materials = materialsCache;

            // wait for the itnerval before toggling again
            yield return new WaitForSeconds(flashInterval);
        }
    }
}
