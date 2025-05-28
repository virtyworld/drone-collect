using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the spawning of resources in a 3D space with obstacle avoidance.
/// Resources are spawned within a specified radius and height range, with configurable spawn rates
/// and maximum resource limits. Includes collision detection to prevent spawning inside obstacles.
/// </summary>
public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private SpawnedResource resourcePrefab;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float minSpawnHeight = 10f;
    [SerializeField] private float maxSpawnHeight = 10f;
    [SerializeField] private int maxResources = 100;

    [Header("Obstacle Detection")]
    [SerializeField] private float obstacleCheckRadius = 2f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform dirToSpawn;

    private bool isInitialized;
    private float nextSpawnTime;
    private List<SpawnedResource> spawnedResources = new List<SpawnedResource>();

    /// <summary>
    /// Initializes the spawner and sets up event listeners for spawn interval changes
    /// </summary>
    /// <param name="onResourceSpawnIntervalChanged">Event that triggers when spawn interval needs to be updated</param>
    public void Setup(UnityEvent<float> onResourceSpawnIntervalChanged)
    {
        onResourceSpawnIntervalChanged.AddListener(UpdateSpawnInterval);
        Initialize();
    }

    /// <summary>
    /// Initializes the spawner's internal state and timing
    /// </summary>
    private void Initialize()
    {
        nextSpawnTime = CalculateNextSpawnTime();
        isInitialized = true;
    }

    /// <summary>
    /// Updates the spawn interval based on the desired spawns per second
    /// </summary>
    /// <param name="spawnsPerSecond">Number of resources to spawn per second</param>
    public void UpdateSpawnInterval(float spawnsPerSecond)
    {
        spawnInterval = CalculateSpawnInterval(spawnsPerSecond);
        nextSpawnTime = CalculateNextSpawnTime();
    }

    /// <summary>
    /// Calculates the spawn interval in seconds from spawns per second
    /// </summary>
    private float CalculateSpawnInterval(float spawnsPerSecond)
    {
        return 1f / spawnsPerSecond;
    }

    /// <summary>
    /// Calculates the next spawn time based on current time and interval
    /// </summary>
    private float CalculateNextSpawnTime()
    {
        return Time.time + spawnInterval;
    }

    /// <summary>
    /// Main update loop that handles resource spawning
    /// </summary>
    private void Update()
    {
        if (!ShouldProcessUpdate()) return;

        if (ShouldSpawnResource())
        {
            SpawnResource();
            nextSpawnTime = CalculateNextSpawnTime();
        }
    }

    /// <summary>
    /// Checks if the spawner is initialized and should process updates
    /// </summary>
    private bool ShouldProcessUpdate()
    {
        return isInitialized;
    }

    /// <summary>
    /// Determines if a new resource should be spawned based on timing and capacity
    /// </summary>
    private bool ShouldSpawnResource()
    {
        return Time.time >= nextSpawnTime && HasAvailableResourceSlots();
    }

    /// <summary>
    /// Checks if there are available slots for new resources
    /// </summary>
    private bool HasAvailableResourceSlots()
    {
        return spawnedResources.Count < maxResources;
    }

    /// <summary>
    /// Attempts to spawn a new resource at a valid position
    /// </summary>
    private void SpawnResource()
    {
        Vector3 spawnPosition = FindValidSpawnPosition();
        if (spawnPosition != Vector3.zero)
        {
            CreateResourceAtPosition(spawnPosition);
        }
    }

    /// <summary>
    /// Finds a valid spawn position that is not blocked by obstacles
    /// </summary>
    /// <returns>Valid spawn position or Vector3.zero if no valid position found</returns>
    private Vector3 FindValidSpawnPosition()
    {
        const int maxAttempts = 10;
        int attempts = 0;

        do
        {
            Vector3 position = GetRandomPosition();
            if (!IsObstacleNearby(position))
            {
                return position;
            }
            attempts++;
        } while (attempts < maxAttempts);

        return Vector3.zero;
    }

    /// <summary>
    /// Creates and initializes a new resource at the specified position
    /// </summary>
    /// <param name="position">Position to spawn the resource at</param>
    private void CreateResourceAtPosition(Vector3 position)
    {
        SpawnedResource resource = Instantiate(resourcePrefab, position, Quaternion.identity);
        SetupResource(resource);
        spawnedResources.Add(resource);
    }

    /// <summary>
    /// Sets up a newly created resource with initial properties
    /// </summary>
    /// <param name="resource">The resource to set up</param>
    private void SetupResource(SpawnedResource resource)
    {
        resource.transform.SetParent(dirToSpawn);
        resource.isFree = true;
    }

    /// <summary>
    /// Generates a random position within the spawn area
    /// </summary>
    /// <returns>Random position within spawn parameters</returns>
    private Vector3 GetRandomPosition()
    {
        float randomAngle = GetRandomAngle();
        float randomDistance = GetRandomDistance();
        float randomHeight = GetRandomHeight();

        Vector3 randomDirection = GetDirectionFromAngle(randomAngle);
        Vector3 position = transform.position + randomDirection * randomDistance;
        position.y = randomHeight;

        return position;
    }

    /// <summary>
    /// Generates a random angle in degrees
    /// </summary>
    private float GetRandomAngle()
    {
        return Random.Range(0f, 360f);
    }

    /// <summary>
    /// Generates a random distance within spawn radius
    /// </summary>
    private float GetRandomDistance()
    {
        return Random.Range(0f, spawnRadius);
    }

    /// <summary>
    /// Generates a random height within spawn height range
    /// </summary>
    private float GetRandomHeight()
    {
        return Random.Range(minSpawnHeight, maxSpawnHeight);
    }

    /// <summary>
    /// Converts an angle to a direction vector
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Direction vector</returns>
    private Vector3 GetDirectionFromAngle(float angle)
    {
        return Quaternion.Euler(0, angle, 0) * Vector3.forward;
    }

    /// <summary>
    /// Checks if there are any obstacles near the specified position
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if obstacles are found, false otherwise</returns>
    private bool IsObstacleNearby(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, obstacleCheckRadius, obstacleLayer);
        return colliders.Length > 0;
    }
}
