using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameOverManager : MonoBehaviour
{
    public static string lastSceneName = "Stage1"; 

    [Header("タイトルシーンの名前")]
    public string titleSceneName = "Title";

    private void Update()
    {
        if (Keyboard.current != null)
        {
            // Rキーでリトライ
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Retry();
            }

            // Tキーでタイトルへ戻る
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                GoToTitle();
            }
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(lastSceneName);
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
