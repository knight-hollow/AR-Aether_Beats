using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ARRhythm.Core;
public class ResultUI : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text titleText;
    public TMP_Text scoreText;
    public TMP_Text statsText;

    [Header("Buttons")]
    public Button btnMainMenu;
    public Button btnRetry;
    public Button btnSelect;

    [Header("Flow (optional)")]
    public SceneFlow flow;

    private void Awake()
    {
        // 自动找 SceneFlow（如果你 ResultScene 里有）
        if (flow == null) flow = FindObjectOfType<SceneFlow>(true);

        // 自动按名字找 UI（你不拖也能用）
        AutoBindIfNull();
    }

    private void Start()
    {
        BindButtons();
        RefreshView();
    }

    private void AutoBindIfNull()
    {
        // 文本
        if (titleText == null) titleText = FindTMPByName("TitleText");
        if (scoreText == null) scoreText = FindTMPByName("ScoreText");
        if (statsText == null) statsText = FindTMPByName("StatsText");

        // 按钮（按你截图的名字）
        if (btnMainMenu == null) btnMainMenu = FindButtonByName("MainMenuBtn");
        if (btnRetry == null) btnRetry = FindButtonByName("RetryBtn");
        if (btnSelect == null) btnSelect = FindButtonByName("SelectBtn");
    }

    private TMP_Text FindTMPByName(string name)
    {
        var t = transform.root.Find(name);
        if (t != null) return t.GetComponent<TMP_Text>();

        // 全场景兜底查找
        foreach (var tmp in FindObjectsOfType<TMP_Text>(true))
            if (tmp.name == name) return tmp;

        return null;
    }

    private Button FindButtonByName(string name)
    {
        // 全场景查找
        foreach (var b in FindObjectsOfType<Button>(true))
            if (b.name == name) return b;

        return null;
    }

    private void BindButtons()
    {
        if (btnMainMenu != null)
        {
            btnMainMenu.onClick.RemoveListener(OnMainMenu);
            btnMainMenu.onClick.AddListener(OnMainMenu);
        }

        if (btnRetry != null)
        {
            btnRetry.onClick.RemoveListener(OnRetry);
            btnRetry.onClick.AddListener(OnRetry);
        }

        if (btnSelect != null)
        {
            btnSelect.onClick.RemoveListener(OnSelect);
            btnSelect.onClick.AddListener(OnSelect);
        }
    }

    private void RefreshView()
    {
        if (GameManager.Instance == null)
        {
            if (titleText != null) titleText.text = "Result";
            if (scoreText != null) scoreText.text = "Score: 0";
            if (statsText != null) statsText.text = "Perfect: 0  Good: 0  Miss: 0";
            return;
        }

        var gm = GameManager.Instance;

        bool success = (gm.State == GameState.Success);

        if (titleText != null)
            titleText.text = success ? "You WIN!" : "You LOSE!";

        if (scoreText != null)
            scoreText.text = $"Your Score: {gm.Score}";

        // 下面这三个计数：如果你 GameManager 里没有，也不会崩（会显示 0）
        int perfect = TryGetInt(gm, "PerfectCount");
        int good    = TryGetInt(gm, "GoodCount");
        int miss    = gm.MissCount;

        if (statsText != null)
            statsText.text = $"Perfect: {perfect}  Good: {good}  Miss: {miss}";
    }

    // 反射安全读取（避免你没做 PerfectCount / GoodCount 时直接编译报错）
    private int TryGetInt(object obj, string propOrFieldName)
    {
        if (obj == null) return 0;
        var t = obj.GetType();

        var p = t.GetProperty(propOrFieldName);
        if (p != null && p.PropertyType == typeof(int))
            return (int)p.GetValue(obj);

        var f = t.GetField(propOrFieldName);
        if (f != null && f.FieldType == typeof(int))
            return (int)f.GetValue(obj);

        return 0;
    }

    // ===== Button callbacks =====

    public void OnMainMenu()
    {
        if (flow != null) flow.GoMainMenu();
        else Debug.LogWarning("[ResultUI] SceneFlow not found.");
    }

    public void OnRetry()
    {
        // 重开当前难度：走 SceneFlow.StartGame()
        if (flow != null) flow.StartGame();
        else Debug.LogWarning("[ResultUI] SceneFlow not found.");
    }

    public void OnSelect()
    {
        if (flow != null) flow.GoLevelSelect();
        else Debug.LogWarning("[ResultUI] SceneFlow not found.");
    }
}
