using UnityEngine;
using ARRhythm.Core;

public class LevelSelectUI : MonoBehaviour
{
    private SceneFlow flow;

    void Start()
    {
        flow = FindObjectOfType<SceneFlow>();
    }

    public void PickTutorial()
    {
        GameManager.Instance.SelectLevel(LevelType.Tutorial);
        flow.StartGame();
    }

    public void PickMedium()
    {
        GameManager.Instance.SelectLevel(LevelType.Medium);
        flow.StartGame();
    }

    public void PickHard()
    {
        GameManager.Instance.SelectLevel(LevelType.Hard);
        flow.StartGame();
    }

    public void BackToMenu()
    {
        flow.GoMainMenu();
    }
}
