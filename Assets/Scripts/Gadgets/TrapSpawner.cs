using UnityEngine;

public class TrapSpawner : MonoBehaviour
{
    public GameObject trapPrefab; // The prefab to spawn
    public float spawnInterval = 5f; // Time in seconds between spawns
    public int maxTraps = 10; // Maximum number of traps allowed in the scene
    public float spawnRadius = 10f; // Radius around the spawner to spawn traps
    public float minSpawnDistance = 2f; // Minimum distance from the spawner to spawn traps
    public float spawnHeight = 1f; // Height at which traps are spawned
    public float trapLifetime = 10f; // Lifetime of each trap before it is destroyed
    private float nextSpawnTime = 0f; // Time when the next trap will be spawned
    private int currentTrapCount = 0; // Current number of traps in the scene

    public Transform playerTransform;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextSpawnTime && currentTrapCount < maxTraps)
        {
            SpawnTrap();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnTrap()
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject trap = Instantiate(trapPrefab, spawnPos, Quaternion.identity);
        currentTrapCount++;
        Destroy(trap, trapLifetime);
        trap.AddComponent<TrapLifetime>().Init(this, trapLifetime);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 center = playerTransform.position;
        Vector2 randomCircle;
        float distance;
        do
        {
            randomCircle = Random.insideUnitCircle * spawnRadius;
            distance = randomCircle.magnitude;
        } while (distance < minSpawnDistance);

        Vector3 spawnPos = new Vector3(
            center.x + randomCircle.x,
            center.y + spawnHeight,
            center.z + randomCircle.y
        );
        return spawnPos;
    }

    // Called by TrapLifetime when a trap is destroyed
    public void OnTrapDestroyed()
    {
        // maintain the current trap count
        currentTrapCount = Mathf.Max(0, currentTrapCount - 1);
    }
}
