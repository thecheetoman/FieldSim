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

    [Header("Transforms & Setup")]
    [SerializeField] private List<Transform> storageSlots = new List<Transform>();
    [SerializeField] private Transform launchPoint;

    // fifo queue of currently stored game pieces
    // fifo is first in first out. makes realisitc hopper thingy. even with this static stuff
    private List<GamePiece> loadedBalls = new List<GamePiece>();

    // keep track of when the next ball can be shot
    private float nextShootTime = 0f;
    private void Update()
    {
        // handle shooting with a random delay between shots
        if (Input.GetKey(shootKey) && Time.time >= nextShootTime)
        {
            if (loadedBalls.Count > 0)
            {
                ShootBall();

                // set the next allowed shot time to random delay time
                float randomDelay = Random.Range(minShootDelay, maxShootDelay);
                nextShootTime = Time.time + randomDelay;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // we intake when you press the key =o
        if (!Input.GetKey(intakeKey)) return;

        // check if the hopper is full, if so, don't intake any more.
        if (loadedBalls.Count >= storageSlots.Count) return;

        // check if the object is a gamepiece =p
        if (other.TryGetComponent<GamePiece>(out GamePiece ball))
        {
            // dont intake a ball that is already in the hopper, no idea if this is really useful, but it fixed random intaking glitches
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

        // get ball at slot0
        GamePiece ballToShoot = loadedBalls[0];
        loadedBalls.RemoveAt(0);

        // launch the ball using the shooter position
        Vector3 forceVector = launchPoint.forward * launchSpeed;
        ballToShoot.Launch(launchPoint.position, launchPoint.rotation, forceVector);

        // cycle remaining balls to their next respective slot
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

    // in editor visualization, draw the ball as a ball and a simple shooter trajection ray
    private void OnDrawGizmos()
    {
        // draw storage slots as wire spheres
        Gizmos.color = Color.yellow;
        for (int i = 0; i < storageSlots.Count; i++)
        {
            if (storageSlots[i] != null)
            {
                Gizmos.DrawSphere(storageSlots[i].position, 0.075f);
                //Gizmos.DrawWireSphere(storageSlots[i].position, 0.075f);
            }
        }

        // draw launch point position & direction arrow
        if (launchPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(launchPoint.position, 0.1f);

            // Trajectory ray
            Vector3 rayEnd = launchPoint.position + (launchPoint.forward * 1.5f);
            Gizmos.DrawLine(launchPoint.position, rayEnd);
            Gizmos.DrawWireSphere(rayEnd, 0.05f);
        }
    }
}