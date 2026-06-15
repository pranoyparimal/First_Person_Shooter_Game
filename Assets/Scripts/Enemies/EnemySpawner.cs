using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public int maxEnemies = 5;
    
    [Header("Spawn Locations")]
    public Transform[] spawnPoints;
    public float spawnRadius = 10f; // Used if spawnPoints is empty

    private float timer;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Update()
    {
        if (enemyPrefab == null) return;

        // Clean up any destroyed enemies from the list
        activeEnemies.RemoveAll(item => item == null);

        if (activeEnemies.Count >= maxEnemies)
        {
            // Do not spawn if we reached the limit
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPos;
        
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPos = point.position;
        }
        else
        {
            // Random point around the spawner on the XZ plane
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(newEnemy);
    }
}
