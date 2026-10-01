using UnityEngine;
using StarterAssets;

public class SlimeEnemy : MonoBehaviour
{
    [Header("踏まれた時の跳ね返る力")]
    public float bounceForce = 5f;

    [Header("やられた時のエフェクト（任意）")]
    public GameObject defeatEffect;

    [Header("踏みつけ判定の高さ（スライムの高さに応じて調整）")]
    public float stompThreshold = 0.5f;

    [Header("倒した時の獲得スコア")]
    public int scoreValue = 300;

    private bool isDead = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            ThirdPersonController player = other.GetComponent<ThirdPersonController>();
            
            // プレイヤーがスライムより少し高い位置にいるか判定（以前うまく動いていた0.1fに戻します）
            bool isAbove = other.transform.position.y > transform.position.y + 0.1f;

            // 「空中にいる（ジャンプや落下中）」かつ「上から接触した」場合のみ踏みつけ判定
            if (isAbove && player != null && !player.Grounded)
            {
                player.Bounce(bounceForce);
                Defeat();
            }
            else
            {
                // それ以外（地上を走ってぶつかった、ジャンプ中だけど横からぶつかった等）はダメージ
                Debug.Log("横から接触");
                
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(transform.position);
                }
            }
        }
    }

    private void Defeat()
    {
        isDead = true;

        // スコアを加算する
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        if (defeatEffect != null)
        {
            Instantiate(defeatEffect, transform.position, Quaternion.identity);
        }

        Destroy(transform.root.gameObject);

    }
}