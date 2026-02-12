using UnityEngine;
using ARRhythm.Core;

public class MainMenuUI : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel; // 可选

    private SceneFlow flow;

    void Start()
    {
        flow = FindObjectOfType<SceneFlow>();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OnStartGame()
    {
        flow.GoLevelSelect();
    }

    public void OnOpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnCloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OnQuit()
    {
        flow.QuitApp();
    }
}
