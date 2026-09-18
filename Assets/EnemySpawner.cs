using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成する敵のPrefab")]
    public GameObject enemyPrefab;

    [Header("生成位置")]
    public Transform spawnPoint;

    [Header("生成間隔（秒）")]
    public float spawnInterval = 3.0f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if(timer > spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;

        spawnPosition.z = 0f;

        Quaternion spawnRotation = Quaternion.Euler(0f, -90f, 0f);

        Instantiate(enemyPrefab, spawnPosition, spawnRotation); 
    }
}
