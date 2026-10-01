using UnityEngine;
using StarterAssets;

public class SlimeEnemy : MonoBehaviour
{
    [Header("踏まれた時の跳ね返る力")]
    public float bounceForce = 5f;

    private AudioSource[] audioSources;

    private void Start()
    {
        // Bodyなどの子オブジェクトに付けられたAudioSourceをすべて取得
        // [0] がやられた時の音、[1] がダメージ音
        audioSources = GetComponentsInChildren<AudioSource>();
    }

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
                
                if (audioSources != null && audioSources.Length > 1)
                {
                    audioSources[1].Play(); // 2つ目（ダメージ音）を再生
                }
                
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

        if (audioSources != null && audioSources.Length > 0 && audioSources[0] != null)
        {
            AudioSource defeatAudioSource = audioSources[0];
            defeatAudioSource.Play();
            // 音が鳴り終わるまで待ってからオブジェクトを削除する
            float delay = defeatAudioSource.clip != null ? defeatAudioSource.clip.length : 1f;
            Destroy(transform.root.gameObject, delay);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }

        // 削除されるまでの間、当たり判定と見た目（子オブジェクト）を消して見えなくする
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

    }
}