using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("生成するCoinのPrefab")]
    public GameObject coinPrefab;

    [Header("生成するコインの枚数")]
    public int coinCount = 10;

    [Header("コイン出現範囲の始点（最小座標）")]
    public Vector3 minSpawnPosition = new Vector3(0f, 0.8f, 0f);

    [Header("コイン出現範囲の終点（最大座標）")]
    public Vector3 maxSpawnPosition = new Vector3(100f, 3f, 0f);

    [Header("重なり防止設定")]
    public float checkRadius = 0.5f;

    [Tooltip("避けるべき対象のレイヤー（CoinやStageなど）")]
    public LayerMask obstacleLayer;

    [Tooltip("位置被り時の最大再試行回数")]
    public int maxAttemptsPerCoin = 100;

    void Start()
    {
        SpawnCoins();
    }

    void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("Coin Prefabがセットされていません。");
            return;
        }

        int spawnedCount = 0; // 実際に生成できたコインの数

        for (int i = 0; i < coinCount; i++)
        {
            int attempts = 0;
            bool success = false;

            while (attempts < maxAttemptsPerCoin && !success)
            {
                attempts++;

                // 決めた座標から座標までの範囲でランダムに
                float randomX = Random.Range(minSpawnPosition.x, maxSpawnPosition.x);
                float randomY = Random.Range(minSpawnPosition.y, maxSpawnPosition.y);
                float randomZ = Random.Range(minSpawnPosition.z, maxSpawnPosition.z);

                Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

                bool isOverlapping = Physics.CheckSphere(spawnPosition, checkRadius, obstacleLayer);

                // 重なっていない場合のみ生成
                if (!isOverlapping)
                {
                    Instantiate(coinPrefab, spawnPosition, coinPrefab.transform.rotation);
                    spawnedCount++;
                    success = true;
                }
            }

            if (!success)
            {
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Vector3 center = (minSpawnPosition + maxSpawnPosition) / 2f;
        Vector3 size = new Vector3(
            Mathf.Abs(maxSpawnPosition.x - minSpawnPosition.x),
            Mathf.Abs(maxSpawnPosition.y - minSpawnPosition.y),
            Mathf.Abs(maxSpawnPosition.z - minSpawnPosition.z)
        );
        Gizmos.DrawCube(center, size);
        Gizmos.DrawWireCube(center, size);
    }
}