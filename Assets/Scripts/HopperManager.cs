using System.Collections.Generic;
using UnityEngine;

public class HopperManager : MonoBehaviour
{
    [Header("Keybindings")]
    [SerializeField] private KeyCode intakeKey = KeyCode.E;
    [SerializeField] private KeyCode shootKey = KeyCode.Space;

    [Header("Settings")]
    [SerializeField] private float launchSpeed = 15f;
    [SerializeField] private float minShootDelay = 0.6f;
    [SerializeField] private float maxShootDelay = 1.0f;

    [Header("Shooter Status")]
    [Tooltip("If true, the hopper will shoot automatically as long as keys/delays allow. If false, it waits for an external event (e.g. SpinningShaft) to enable shooting.")]
    [SerializeField] private bool requireShooterEvent = true;
    [SerializeField] private bool isShooterReady = false; // Controlled by UnityEvents!

    [Header("Transforms & Setup")]
    [SerializeField] private List<Transform> storageSlots = new List<Transform>();
    [SerializeField] private Transform launchPoint;

    [Header("Preload Settings")]
    [Tooltip("The GamePiece prefab to spawn when preloading fuel at match start.")]
    [SerializeField] private GameObject fuelPrefab;
    [Tooltip("How many pieces of fuel to start the match loaded with.")]
    [Range(0, 8)][SerializeField] private int preloadCount = 1;

    // fifo queue of currently stored game pieces
    // first in first out
    private List<GamePiece> loadedBalls = new List<GamePiece>();

    // keep track of when the next ball can be shot
    private float nextShootTime = 0f;

    private void Start()
    {
        if (fuelPrefab == null) return;
        int amountToLoad = Mathf.Min(preloadCount, storageSlots.Count);

        for (int i = 0; i < amountToLoad; i++)
        {
            Transform slotTransform = storageSlots[i];

            // instantate prefab at slot position
            GameObject newFuelObject = Instantiate(fuelPrefab, slotTransform.position, slotTransform.rotation);

            if (newFuelObject.TryGetComponent<GamePiece>(out GamePiece ball))
            {
                IntakeBall(ball);
            }
        }
    }
    private void Update()
    {
        // check if shooting contitions are met before shooting
        bool shooterIsAllowed = !requireShooterEvent || isShooterReady;

        if (Input.GetKey(shootKey) && Time.time >= nextShootTime && shooterIsAllowed)
        {
            if (loadedBalls.Count > 0)
            {
                ShootBall();

                // set the next allowed shot time to a random delay
                float randomDelay = Random.Range(minShootDelay, maxShootDelay);
                nextShootTime = Time.time + randomDelay;
            }
        }
    }

    #region Public Event Callbacks (Hook these up to SpinningShaft events)

    public void SetShooterReady()
    {
        isShooterReady = true;
    }

    public void SetShooterNotReady()
    {
        isShooterReady = false;
    }

    #endregion

    private void OnTriggerStay(Collider other)
    {
        if (!Input.GetKey(intakeKey)) return;

        if (loadedBalls.Count >= storageSlots.Count) return;

        if (other.TryGetComponent<GamePiece>(out GamePiece ball))
        {
            if (!loadedBalls.Contains(ball))
            {
                IntakeBall(ball);
            }
        }
    }

    private void IntakeBall(GamePiece ball)
    {
        int targetIndex = loadedBalls.Count;
        Transform slotTransform = storageSlots[targetIndex];

        ball.Capture(slotTransform);
        loadedBalls.Add(ball);
    }

    private void ShootBall()
    {
        if (loadedBalls.Count == 0 || launchPoint == null) return;

        GamePiece ballToShoot = loadedBalls[0];
        loadedBalls.RemoveAt(0);

        Vector3 forceVector = launchPoint.forward * launchSpeed;
        ballToShoot.Launch(launchPoint.position, launchPoint.rotation, forceVector);

        CycleQueue();
    }

    private void CycleQueue()
    {
        for (int i = 0; i < loadedBalls.Count; i++)
        {
            Transform targetSlot = storageSlots[i];
            loadedBalls[i].Capture(targetSlot);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < storageSlots.Count; i++)
        {
            if (storageSlots[i] != null)
            {
                Gizmos.DrawSphere(storageSlots[i].position, 0.075f);
            }
        }

        if (launchPoint != null)
        {
            // Turn green in editor if shooter is ready, yellow if not!
            Gizmos.color = isShooterReady ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(launchPoint.position, 0.1f);

            Vector3 rayEnd = launchPoint.position + (launchPoint.forward * 1.5f);
            Gizmos.DrawLine(launchPoint.position, rayEnd);
            Gizmos.DrawWireSphere(rayEnd, 0.05f);
        }
    }
}