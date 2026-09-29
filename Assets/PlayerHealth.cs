using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using StarterAssets;

public class PlayerHealth : MonoBehaviour
{
    [Header("プレイヤーの体力")]
    public int maxLife = 3;
    private int currentLife;

    [Header("UI設定")]
    public TextMeshProUGUI lifeText;

    [Header("ノックバック設定")]
    public float knockbackForce = 15f;

    [Header("ゲームオーバーシーン名")]
    public string gameOverSceneName = "GameOver";

    [Header("落下設定")]
    public float fallThresholdY = -10f;
    private Vector3 lastSafePosition;

    private ThirdPersonController controller;

    void Start()
    {
        currentLife = maxLife;
        controller = GetComponent<ThirdPersonController>();
        lastSafePosition = transform.position;
        UpdateUI();
    }

    void Update()
    {
        if (controller != null && controller.Grounded)
        {
            lastSafePosition = transform.position;
        }

        // 指定したY座標より下に落ちたら落下ダメージ
        if (transform.position.y < fallThresholdY)
        {
            FallDamage();
        }
    }

    private void FallDamage()
    {
        if (currentLife <= 0) return;

        currentLife--;
        UpdateUI();

        if (currentLife <= 0)
        {
            Die();
        }
        else
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            
            transform.position = lastSafePosition + new Vector3(0, 1.0f, 0);
            
            if (cc != null) cc.enabled = true;

            if (controller != null)
            {
                controller.ApplyKnockback(Vector3.zero, 0f);
            }
        }
    }

    public void TakeDamage(Vector3 damageSourcePosition)
    {
        if (currentLife <= 0) return;

        currentLife--;
        UpdateUI();

        if (currentLife <= 0)
        {
            Die();
        }
        else
        {
            // ノックバック処理
            if (controller != null)
            {
                // ダメージ元から遠ざかる方向（左右）
                Vector3 knockbackDir = (transform.position - damageSourcePosition).normalized;
                knockbackDir.y = 0;
                knockbackDir.z = 0;
                
                if (knockbackDir == Vector3.zero) knockbackDir = Vector3.left;

                knockbackDir = knockbackDir.normalized;

                controller.ApplyKnockback(knockbackDir, knockbackForce);
            }
        }
    }

    private void UpdateUI()
    {
        if (lifeText != null)
        {
            // 日本語フォントがないための文字化け（□□になる）を防ぐため、英語に変更
            lifeText.text = $"Life : {currentLife}";
        }
    }

    private void Die()
    {
        GameOverManager.lastSceneName = SceneManager.GetActiveScene().name;

        // ライフが0になったらゲームオーバーシーンへ遷移
        SceneManager.LoadScene(gameOverSceneName);
    }
}
