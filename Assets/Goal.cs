using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("遷移させるシーン名")]
    public string nextSceneName = "StageClear";

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

        if (clearEffectPrefab != null)
        {
            Instantiate(clearEffectPrefab, transform.position, Quaternion.identity);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}