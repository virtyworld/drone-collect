using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject resourcePrefab; // Префаб ресурса для спавна
    [SerializeField] private float spawnInterval = 5f; // Интервал между спавном ресурсов в секундах
    [SerializeField] private float spawnRadius = 20f; // Радиус области спавна
    [SerializeField] private Transform dirToSpawn;
    [SerializeField] private bool showSpawnRadius = false; // Показывать ли радиус спавна в редакторе

    private float nextSpawnTime;
    private List<GameObject> resources = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnResource();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnResource()
    {
        // Генерируем случайную позицию в пределах радиуса
        Vector3 randomPosition = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = new Vector3(randomPosition.x, randomPosition.y, randomPosition.z);

        // Создаем ресурс
        GameObject resource = Instantiate(resourcePrefab, spawnPosition, Quaternion.identity);
        resource.transform.SetParent(dirToSpawn);
        resources.Add(resource);

    }

    private void OnDrawGizmos()
    {
        if (showSpawnRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
