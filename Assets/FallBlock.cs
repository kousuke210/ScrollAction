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
    private bool isFalling = false;

    // プレイヤーがトリガーに触れたら呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            // プレイヤーがブロックの中心より上にいる（＝乗っている）か判定
            if (other.transform.position.y > transform.position.y)
            {
                isTriggered = true;
                StartCoroutine(FallCoroutine());
            }
        }
    }

    private IEnumerator FallCoroutine()
    {
        // 指定した時間（fallDelay）だけ待機
        yield return new WaitForSeconds(fallDelay);

        isFalling = true;

        // 落下開始から数秒後にこのブロック自身を削除
        Destroy(gameObject, destroyDelay);
    }

    private void Update()
    {
        if (isFalling)
        {
            // 毎フレーム、下方向に移動させる
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }
    }
}
