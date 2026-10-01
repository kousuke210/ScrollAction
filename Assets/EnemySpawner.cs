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
    public float minXPosition = 10.0f;
    
    [Tooltip("X軸の出現終了位置（どこまで右に出るか）")]
    public float maxXPosition = 50.0f;

    [Header("地面・重なり回避設定")]
    [Tooltip("地形を探索するためにレイ（光線）を飛ばし始める高さ")]
    public float raycastStartY = 15.0f;

    [Tooltip("地面からどれくらい浮かせて生成するか（スライムの原点に合わせる）")]
    public float spawnYOffset = 0.1f;

    [Tooltip("他のスライムや壁とどれくらい離すか（半径）")]
    public float overlapRadius = 1.0f;

    [Tooltip("地面や障害物として判定するレイヤー")]
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
        Coin[] allCoins = FindObjectsOfType<Coin>();
        Vector3 basePosition = (spawnPoint != null) ? spawnPoint.position : transform.position;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = Vector3.zero;
            bool positionFound = false;

            // すべての敵をランダムな位置に生成
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
                {
                    float randomX = Random.Range(minXPosition, maxXPosition);
                    
                    Vector3 rayStart = new Vector3(randomX, basePosition.y + raycastStartY, 0f);
                    if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 50f, obstacleLayer, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }

                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("End"))
                    {
                        continue;
                    }

                    Vector3 candidatePos = hit.point + Vector3.up * spawnYOffset;

                    Vector3 leftCheckStart = candidatePos + Vector3.left * overlapRadius + Vector3.up;
                    Vector3 rightCheckStart = candidatePos + Vector3.right * overlapRadius + Vector3.up;
                    if (!Physics.Raycast(leftCheckStart, Vector3.down, 2f, obstacleLayer, QueryTriggerInteraction.Ignore) ||
                        !Physics.Raycast(rightCheckStart, Vector3.down, 2f, obstacleLayer, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }

                    bool isTooCloseToOtherSlime = false;
                    foreach (Vector3 pos in spawnedPositions)
                    {
                        if (Vector3.Distance(candidatePos, pos) < overlapRadius * 2f)
                        {
                            isTooCloseToOtherSlime = true;
                            break;
                        }
                    }
                    if (isTooCloseToOtherSlime) continue;

                    bool isTooCloseToCoin = false;
                    foreach (Coin coin in allCoins)
                    {
                        if (Vector3.Distance(candidatePos, coin.transform.position) < overlapRadius * 2f)
                        {
                            isTooCloseToCoin = true;
                            break;
                        }
                    }
                    if (isTooCloseToCoin) continue;

                    bool hitWall = false;
                    Collider[] colliders = Physics.OverlapSphere(candidatePos + Vector3.up * (overlapRadius * 0.5f), overlapRadius * 0.8f, obstacleLayer);
                    foreach (var col in colliders)
                    {
                        if (col != hit.collider)
                        {
                            if (!col.isTrigger || col.GetComponent<Coin>() != null)
                            {
                                hitWall = true;
                                break;
                            }
                        }
                    }

                    if (hitWall) continue;

                    spawnPosition = candidatePos;
                    positionFound = true;
                    break; 
                }

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
