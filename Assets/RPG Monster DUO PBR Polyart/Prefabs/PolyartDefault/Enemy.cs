using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("踏みつけ時のプレイヤーの跳ね返り力")]
    public float bounceForce = 8f;

    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手が Player タグを持っているか確認
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーの Rigidbody を取得
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            // 接触点の法線（Normal）を確認して「上から踏まれたか」を判定
            foreach (ContactPoint contact in collision.contacts)
            {
                // contact.normal.y > 0.5f は「衝突が上方向（頭上）から発生した」ことを示す
                // またはプレイヤーが落下中（Velocity Y < 0）の判定を組み合わせる
                if (contact.normal.y < -0.5f || (playerRb != null && playerRb.linearVelocity.y < -0.1f))
                {
                    // 1. プレイヤーを上に跳ね返させる（踏みつけ演出）
                    if (playerRb != null)
                    {
                        Vector3 vel = playerRb.linearVelocity;
                        vel.y = bounceForce; // Y方向の速度を直接上書き
                        playerRb.linearVelocity = vel;
                    }

                    // 2. 敵の死亡処理
                    Die();
                    return;
                }
            }

            // 上からでなければ、プレイヤー側にダメージを与える処理などをここに呼ぶ
            // PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            // if (playerHealth != null) { playerHealth.TakeDamage(); }
        }
    }

    /// <summary>
    /// 敵の死亡処理
    /// </summary>
    private void Die()
    {
        // 必要に応じて倒されたエフェクト生成やSE再生をここに追加
        // Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject); // 敵オブジェクトの削除
    }
}