using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] zombiePrefabs;
    public Transform[] spawnPoints;
    public int zombiesToSpawn = 5;

    public void SpawnZombies()
    {
        WaveManager.Instance.currentZombiesAlive = zombiesToSpawn;

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefabToSpawn = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
            Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
    }
}