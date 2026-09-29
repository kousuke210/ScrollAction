using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NextStage : MonoBehaviour
{
    [Header("遷移先のシーン名")]
    public string nextSceneName = "Stage2";
    private void Update()
    {
        // スペースキーが押されたらゲーム開始
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Nextstage();
        }
    }

    public void Nextstage()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
