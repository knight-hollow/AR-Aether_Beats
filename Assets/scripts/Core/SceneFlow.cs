using UnityEngine;
using UnityEngine.SceneManagement;

namespace ARRhythm.Core
{
    public class SceneFlow : MonoBehaviour
    {
        // Scene names
        public const string BootScene = "BootScene";
        public const string MainMenuScene = "MainMenuScene";
        public const string SelectScene = "SelectScene";
        public const string GameScene = "GameScene";
        public const string ResultScene = "ResultScene";

        private void Start()
        {
            // Boot -> MainMenu
            if (SceneManager.GetActiveScene().name == BootScene)
            {
                GameManager.Instance.SetState(GameState.MainMenu);
                SceneManager.LoadScene(MainMenuScene);
            }
        }

        public void GoMainMenu()
        {
            GameManager.Instance.SetState(GameState.MainMenu);
            SceneManager.LoadScene(MainMenuScene);
        }

        public void GoLevelSelect()
        {
            GameManager.Instance.SetState(GameState.LevelSelect);
            SceneManager.LoadScene(SelectScene);
        }

        public void StartGame()
        {
            GameManager.Instance.ResetRunData();
            GameManager.Instance.SetState(GameState.Countdown);
            SceneManager.LoadScene(GameScene);
        }

        public void GoResult(bool success)
        {
            GameManager.Instance.SetState(success ? GameState.Success : GameState.Fail);
            SceneManager.LoadScene(ResultScene);
        }

        public void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
