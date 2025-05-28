using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceSpawner : MonoBehaviour
{
    [SerializeField] private SpawnedResource resourcePrefab; // Префаб ресурса для спавна
    [SerializeField] private float spawnInterval = 5f; // Интервал между спавном ресурсов в секундах
    [SerializeField] private float spawnRadius = 15f; // Радиус области спавна
    [SerializeField] private int maxResources = 100;
    [SerializeField] private float obstacleCheckRadius = 1.5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform dirToSpawn;

    private float nextSpawnTime;
    private List<SpawnedResource> spawnedResources = new List<SpawnedResource>();

    public void UpdateSpawnInterval(float spawnsPerSecond)
    {
        Debug.Log("UpdateSpawnInterval: " + spawnsPerSecond);
        // Преобразуем количество спавнов в секунду в интервал между спавнами
        spawnInterval = 1f / spawnsPerSecond;

        // Обновляем время следующего спавна
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextSpawnTime && spawnedResources.Count < maxResources)
        {
            SpawnResource();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnResource()
    {
        Vector3 spawnPosition;
        int maxAttempts = 10;
        int attempts = 0;

        do
        {
            spawnPosition = GetRandomPosition();
            attempts++;
        } while (IsObstacleNearby(spawnPosition) && attempts < maxAttempts);

        if (attempts < maxAttempts)
        {
            SpawnedResource resource = Instantiate(resourcePrefab, spawnPosition, Quaternion.identity);
            resource.transform.SetParent(dirToSpawn);
            resource.isFree = true;
            spawnedResources.Add(resource);
        }
    }

    private Vector3 GetRandomPosition()
    {
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(0f, spawnRadius);
        Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        return transform.position + randomDirection * randomDistance;
    }

    private bool IsObstacleNearby(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, obstacleCheckRadius, obstacleLayer);
        return colliders.Length > 0;
    }


}
