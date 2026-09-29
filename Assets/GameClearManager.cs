using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameClearManager : MonoBehaviour
{
    public static string lastSceneName = "Stage1"; 

    [Header("タイトルシーンの名前")]
    public string titleSceneName = "Title";

    private void Update()
    {
        if (Keyboard.current != null)
        {
            // Tキーでタイトルへ戻る
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                GoToTitle();
            }
        }
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
