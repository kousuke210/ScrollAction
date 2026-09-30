using UnityEngine;
using System.Collections;

public class FallBlock : MonoBehaviour
{
    [Header("乗ってから落下するまでの時間（秒）")]
    public float fallDelay = 0.5f;

    [Header("落下の速度")]
    public float fallSpeed = 10.0f;

    [Header("落下後に消滅するまでの時間（秒）")]
    public float destroyDelay = 3.0f;

    private bool isTriggered = false;
    public bool isFalling = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            // プレイヤーが乗っているか判定
            if (other.transform.position.y > transform.position.y)
            {
                isTriggered = true;
                StartCoroutine(FallCoroutine());
            }
        }
    }

    private IEnumerator FallCoroutine()
    {
        yield return new WaitForSeconds(fallDelay);

        isFalling = true;

        yield return new WaitForSeconds(destroyDelay);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isFalling)
        {
            // 毎フレーム、下方向に移動させる
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }
    }

    // ブロックを初期状態に復活させる
    public void Respawn()
    {
        StopAllCoroutines();
        isTriggered = false;
        isFalling = false;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        gameObject.SetActive(true);
    }
}
