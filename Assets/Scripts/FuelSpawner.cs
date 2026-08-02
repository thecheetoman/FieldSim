using UnityEngine;

public class FuelSpawner : MonoBehaviour
{
    [Header("Prefab & Parent Setup")]
    [Tooltip("The Fuel or GamePiece prefab you want to spawn.")]
    public GameObject fuelPrefab;

    [Tooltip("Optional: Transform to hold all spawned fuel objects. If left empty, it creates a new container.")]
    public Transform containerParent;

    [Header("Grid Array Dimensions")]
    public int countX = 3;
    public int countY = 2;
    public int countZ = 3;

    [Header("Spacing")]
    public Vector3 spacing = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("Offsets & Jitter (Optional)")]
    [Tooltip("Applies a small random position shift to each piece so they don't look perfectly stiff.")]
    public bool addRandomJitter = false;
    public Vector3 maxJitter = new Vector3(0.02f, 0.02f, 0.02f);

    /// <summary>
    /// Right-click the component title in the Inspector and select 'Spawn Fuel Array' to run this in Edit Mode.
    /// </summary>
    [ContextMenu("Spawn Fuel Array")]
    public void SpawnFuelArray()
    {
        if (fuelPrefab == null)
        {
            Debug.LogError("FuelSpawner: No Fuel Prefab assigned!", this);
            return;
        }

        // Set up container
        if (containerParent == null)
        {
            GameObject container = new GameObject("Fuel_Container");
            container.transform.position = transform.position;
            containerParent = container.transform;
        }

        int totalSpawned = 0;

        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    // Calculate local position in the grid centered around the spawner
                    Vector3 localPos = new Vector3(
                        (x - (countX - 1) * 0.5f) * spacing.x,
                        y * spacing.y,
                        (z - (countZ - 1) * 0.5f) * spacing.z
                    );

                    // Add slight random offset if enabled
                    if (addRandomJitter)
                    {
                        localPos += new Vector3(
                            Random.Range(-maxJitter.x, maxJitter.x),
                            Random.Range(-maxJitter.y, maxJitter.y),
                            Random.Range(-maxJitter.z, maxJitter.z)
                        );
                    }

                    Vector3 worldPos = transform.TransformPoint(localPos);

                    // Instantiate fuel piece
                    GameObject newFuel = Instantiate(fuelPrefab, worldPos, transform.rotation, containerParent);
                    newFuel.name = $"{fuelPrefab.name}_{x}_{y}_{z}";

                    totalSpawned++;
                }
            }
        }

        Debug.Log($"Successfully spawned {totalSpawned} fuel pieces under '{containerParent.name}'!", containerParent);
    }

    [ContextMenu("Clear Container Children")]
    public void ClearContainer()
    {
        if (containerParent == null) return;

        // Destroy children cleanly in Editor mode
        for (int i = containerParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(containerParent.GetChild(i).gameObject);
        }

        Debug.Log("Cleared spawned fuel objects from container.");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw editor gizmo visualization of where the grid will spawn
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.4f); // Transparent yellow

        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    Vector3 localPos = new Vector3(
                        (x - (countX - 1) * 0.5f) * spacing.x,
                        y * spacing.y,
                        (z - (countZ - 1) * 0.5f) * spacing.z
                    );

                    Vector3 worldPos = transform.TransformPoint(localPos);
                    Gizmos.DrawWireSphere(worldPos, 0.075f);
                }
            }
        }
    }
}