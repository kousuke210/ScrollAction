using UnityEngine;

public class LiftBlock : MonoBehaviour
{
    [Header("リフトブロックの移動速度")]
    public float speed = 2.0f;

    [Header("リフトの移動範囲")]
    public float RangeX = 10.0f;

    private Vector3 startPosition;

    void Start()
    {
        // 初期位置を記憶
        startPosition = transform.position;
    }

    void Update()
    {
        // 時間経過に合わせて左右（X軸）に往復移動させる
        float newX = startPosition.x + Mathf.Sin(Time.time * speed) * RangeX;
        transform.position = new Vector3(newX, startPosition.y, startPosition.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤーが乗った時、リフトの子オブジェクトにして一緒に移動させる
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // プレイヤーが降りた時、子オブジェクトを解除する
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}