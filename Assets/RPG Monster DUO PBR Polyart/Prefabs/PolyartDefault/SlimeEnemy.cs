using UnityEngine;
using StarterAssets;

public class SlimeEnemy : MonoBehaviour
{
    [Header("踏まれた時の跳ね返る力")]
    public float bounceForce = 5f;

    [Header("やられた時の音")]
    public AudioClip defeatSound;

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
            
            bool isAbove = other.transform.position.y > transform.position.y + 0.1f;

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

        if (defeatSound != null)
        {
            AudioSource.PlayClipAtPoint(defeatSound, transform.position);
        }

        Destroy(transform.root.gameObject);

    }
}