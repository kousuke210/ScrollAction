using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("遷移させるシーン名")]
    public string nextSceneName = "StageClear";

    private bool isCleared = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared || !other.CompareTag("Player")) return;

        ClearStage();
    }

    private void ClearStage()
    {
        isCleared = true;

        SceneManager.LoadScene(nextSceneName);
    }
}