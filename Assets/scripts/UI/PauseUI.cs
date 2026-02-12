using UnityEngine;
using ARRhythm.Core;

public class PauseUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject pausePanel;     // 拖 PausePanel（一般就是自己）
    public GameController game;       // 拖 GameController（不拖也会自动找）
    public SceneFlow flow;            // 拖 SceneFlow（不拖也会自动找）

    private void Awake()
    {
        if (pausePanel == null) pausePanel = gameObject;
        if (game == null) game = FindObjectOfType<GameController>(true);
        if (flow == null) flow = FindObjectOfType<SceneFlow>(true);

        // 初始隐藏
        pausePanel.SetActive(false);
    }

    // 给 PauseButton 用
    public void OpenPause()
    {
        pausePanel.SetActive(true);
        game?.PauseGame();
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        game?.ResumeGame();
    }

    public void Restart()
    {
        // 先确保解除暂停（防止 timeScale 卡住）
        game?.ForceUnpause();

        // 用 SceneFlow 统一重开（会 ResetRunData + Countdown + Load GameScene）
        if (flow != null) flow.StartGame();
        else UnityEngine.SceneManagement.SceneManager.LoadScene(SceneFlow.GameScene);
    }

    public void QuitToMenu()
    {
        game?.ForceUnpause();

        if (flow != null) flow.GoMainMenu();
        else UnityEngine.SceneManagement.SceneManager.LoadScene(SceneFlow.MainMenuScene);
    }
}
