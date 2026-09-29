using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成する敵のPrefab")]
    public GameObject enemyPrefab;

    [Header("基準となる生成位置（空の場合はこのオブジェクトの位置）")]
    public Transform spawnPoint;

    [Header("生成する総数")]
    public int spawnCount = 5;

    [Header("ランダム出現範囲")]
    [Tooltip("X軸の出現開始位置（例:10なら10より右にしか出ない）")]
    public float minXPosition = 10.0f;
    
    [Tooltip("X軸の出現終了位置（どこまで右に出るか）")]
    public float maxXPosition = 50.0f;
    
    [Tooltip("Y軸（上下）にずらす最大距離")]
    public float randomYRange = 0.0f;

    [Header("重なり回避設定")]
    [Tooltip("他のスライムやオブジェクトとどれくらい離すか（半径）")]
    public float overlapRadius = 1.0f;
    [Tooltip("障害物として判定するレイヤー")]
    public LayerMask obstacleLayer;
    [Tooltip("場所探しの最大やり直し回数")]
    public int maxSpawnAttempts = 15;

    private void Start()
    {
        SpawnAllEnemies();
    }

    private void SpawnAllEnemies()
    {
        if (enemyPrefab == null) return;

        List<Vector3> spawnedPositions = new List<Vector3>();
        Vector3 basePosition = (spawnPoint != null) ? spawnPoint.position : transform.position;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = Vector3.zero;
            bool positionFound = false;

            // 最初の一体目は固定位置 (X:11, Y:0, Z:0)
            if (i == 0)
            {
                spawnPosition = new Vector3(11f, 0f, 0f);
                positionFound = true;
            }
            else
            {
                // 2体目以降はランダムに重ならない場所
                for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
                {
                    // プレイヤーより右(X=10以降)に指定範囲内でランダム配置
                    float randomX = Random.Range(minXPosition, maxXPosition);
                    float randomOffsetY = Random.Range(-randomYRange, randomYRange);
                    
                    Vector3 candidatePos = new Vector3(randomX, basePosition.y + randomOffsetY, 0f);

                    // 1. 他の生成済みスライムと近すぎないかチェック
                    bool isTooCloseToOtherSlime = false;
                    foreach (Vector3 pos in spawnedPositions)
                    {
                        if (Vector3.Distance(candidatePos, pos) < overlapRadius * 2f)
                        {
                            isTooCloseToOtherSlime = true;
                            break;
                        }
                    }

                    if (isTooCloseToOtherSlime) continue; // 近すぎたらやり直し

                    // 2. 他のオブジェクト（壁など）と重なっていないかチェック
                    if (Physics.CheckSphere(candidatePos, overlapRadius, obstacleLayer))
                    {
                        continue; // 重なっていたらやり直し
                    }

                    spawnPosition = candidatePos;
                    positionFound = true;
                    break; 
                }
            }

            // 無事に場所が見つかった場合のみ生成
            if (positionFound)
            {
                Quaternion spawnRotation = Quaternion.Euler(0f, -90f, 0f);
                Instantiate(enemyPrefab, spawnPosition, spawnRotation);
                spawnedPositions.Add(spawnPosition);
            }
            else
            {
                Debug.LogWarning($"{i + 1}体目のスライムは、空き場所が見つからなかったため生成をスキップしました。");
            }
        }
    }
}
