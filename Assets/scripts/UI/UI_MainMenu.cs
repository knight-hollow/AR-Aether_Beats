using UnityEngine;
using ARRhythm.Core;

namespace ARRhythm.UI
{
    public class UI_MainMenu : MonoBehaviour
    {
        private SceneFlow flow;

        [Header("Optional: Settings panel placeholder")]
        public GameObject settingsPanel;

        private void Start()
        {
            flow = FindObjectOfType<SceneFlow>();
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void OnClickStart()
        {
            // 去难度选择
            flow.GoLevelSelect();
        }

        public void OnClickSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        public void OnClickQuit()
        {
            flow.QuitApp();
        }
    }
}
