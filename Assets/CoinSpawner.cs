using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("生成するCoinのPrefab")]
    public GameObject coinPrefab;

    [Header("生成するコインの枚数")]
    public int coinCount = 10;

    [Header("X座標のランダム範囲")]
    public float minX = 0f;
    public float maxX = 100f;

    [Header("コインの配置位置（地面のY・Z座標）")]
    public float spawnY = 0.8f;
    public float spawnZ = 1.16903f;

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

        // 指定枚数分、重ならない位置を探して生成
        for (int i = 0; i < coinCount; i++)
        {
            int attempts = 0;
            bool success = false;

            while (attempts < maxAttemptsPerCoin && !success)
            {
                attempts++;

                float randomX = Random.Range(minX, maxX);

                Vector3 spawnPosition = new Vector3(randomX, spawnY, spawnZ);

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
                Debug.LogWarning($"範囲内に空きスペースが見つからず、コイン {i + 1} 枚目の生成をスキップしました。");
            }
        }

        Debug.Log($"{spawnedCount} 枚のコインを重なりなく配置しました。");
    }

    // Sceneビュー上で判定範囲（球体）を視覚的に確認するための処理
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = new Vector3(minX, spawnY, spawnZ);
        Vector3 end = new Vector3(maxX, spawnY, spawnZ);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, checkRadius);
        Gizmos.DrawWireSphere(end, checkRadius);
    }
}