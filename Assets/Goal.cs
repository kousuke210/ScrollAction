using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("遷移させる次のステージのシーン名")]
    public string nextSceneName = "Stage2";

    [Header("クリア時に出すエフェクトPrefab（任意）")]
    public GameObject clearEffectPrefab;

    private bool isCleared = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared || !other.CompareTag("Player")) return;

        ClearStage();
    }

    private void ClearStage()
    {
        isCleared = true;
        Debug.Log("★ STAGE CLEAR! ★");

        //エフェクト生成（登録されている場合）
        if (clearEffectPrefab != null)
        {
            Instantiate(clearEffectPrefab, transform.position, Quaternion.identity);
        }

        // SceneManager.LoadScene(nextSceneName);
    }
}