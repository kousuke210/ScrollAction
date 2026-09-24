using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("回転スピード")]
    public float rotateSpeed = 360f;

    [Header("ゲット時の演出設定")]
    public float riseSpeed = 3f;
    public float riseDuration = 0.5f;

    private bool isCollected = false;
    private Collider coinCollider;

    void Start()
    {
        coinCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (!isCollected)
        {
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            StartCoroutine(CollectSequence());
        }
    }

    private IEnumerator CollectSequence()
    {
        isCollected = true;

        if (coinCollider != null)
        {
            coinCollider.enabled = false;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(150);
        }

        float elapsedTime = 0f;

        while (elapsedTime < riseDuration)
        {
            transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

            transform.Rotate(0f, rotateSpeed * 3f * Time.deltaTime, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}