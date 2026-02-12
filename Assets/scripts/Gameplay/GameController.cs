using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;
using ARRhythm.Core;

public class GameController : MonoBehaviour
{
    public bool isPaused { get; private set; } = false;
    public static bool GlobalPause { get; private set; } = false;

    // ✅ 关键：暂停期间 dspTime 仍会走，所以我们记录暂停累计，提供“校正后的 dsp time”
    public static double PausedDspTotal { get; private set; } = 0.0;
    private static double _pauseDspStart = 0.0;

    public static double NowDspCorrected => AudioSettings.dspTime - PausedDspTotal;

    [Header("Refs")]
    [SerializeField] private ChartSpawner chartSpawner;
    [SerializeField] private AudioSource bgm;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private HealthHeartsUI heartsUI;

    [Header("Gameplay")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int countdownSeconds = 3;

    public bool inputEnabled { get; private set; } = false;

    private int hp;
    private int score;

    private bool ended = false;
    private SceneFlow flow;

    private const string LEVEL_DB_RES_PATH = "LevelDatabase";

    private void Awake()
    {
        // 每次进入 GameScene，重置暂停累计（避免上局残留）
        PausedDspTotal = 0.0;
        _pauseDspStart = 0.0;
        isPaused = false;
        GlobalPause = false;
        Time.timeScale = 1f;

        flow = FindObjectOfType<SceneFlow>(true);

        hp = maxHp;
        score = 0;
        RefreshUI();
    }

    private void Start()
    {
        if (chartSpawner == null) chartSpawner = FindObjectOfType<ChartSpawner>(true);
        if (bgm == null) bgm = FindObjectOfType<AudioSource>(true);

        LevelType level = LevelType.Tutorial;
        if (GameManager.Instance != null) level = GameManager.Instance.CurrentLevel;

        var db = Resources.Load<ScriptableObject>(LEVEL_DB_RES_PATH);
        if (db == null)
        {
            Debug.LogError($"[GameController] Can't load Resources/{LEVEL_DB_RES_PATH}.asset");
            return;
        }

        var cfg = FindConfigByLevelType(db, level);
        if (cfg == null)
        {
            Debug.LogError($"[GameController] Can't find LevelConfig for level={level}. Check LevelDatabase content/field names.");
            DumpPublicFields(db, "LevelDatabase");
            return;
        }

        AudioClip music = GetAny<AudioClip>(cfg,
            "Music", "music", "musicClip", "bgm", "Bgm", "BGM", "bgmClip", "Audio", "audioClip");

        TextAsset chart = GetAny<TextAsset>(cfg,
            "ChartJson", "chartJson", "Chart", "chart", "chartText", "chartAsset", "notesJson", "SongChart");

        Debug.Log($"[GameController] Level={level} cfg={cfg.name} music={(music ? music.name : "NULL")} chart={(chart ? chart.name : "NULL")}");

        if (bgm != null)
        {
            bgm.clip = music;
            bgm.playOnAwake = false;
            bgm.Stop();
        }

        if (chartSpawner == null)
        {
            Debug.LogError("[GameController] chartSpawner is null in scene.");
            return;
        }

        chartSpawner.Configure(chart, bgm, this);
        chartSpawner.ResetSpawning();

        StartCoroutine(CoCountdownThenStart());
    }

    private IEnumerator CoCountdownThenStart()
    {
        inputEnabled = false;

        if (countdownText != null) countdownText.gameObject.SetActive(true);

        if (chartSpawner != null) chartSpawner.ResetSpawning();
        if (bgm != null) bgm.Stop();

        int t = Mathf.Max(1, countdownSeconds);
        for (int i = t; i >= 1; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.gameObject.SetActive(false);
        }

        inputEnabled = true;
        chartSpawner.StartWithMusic();
    }

    private void Update()
    {
        if (ended) return;

        // 失败：HP 归零（不改变你原本扣血，只是补上跳转）
        if (hp <= 0)
        {
            EndGame(success: false);
            return;
        }

        // 胜利：刷完谱面且音乐结束（不改变刷怪/播放逻辑，只是检测到就结算）
        if (inputEnabled && !isPaused && chartSpawner != null && bgm != null)
        {
            if (chartSpawner.IsFinished && !bgm.isPlaying)
            {
                EndGame(success: true);
            }
        }
    }

    // ---------------- Pause / Resume ----------------

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        GlobalPause = true;

        // ✅ 记录暂停起点（用于 dsp 补偿）
        _pauseDspStart = AudioSettings.dspTime;

        inputEnabled = false;
        if (bgm != null) bgm.Pause();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        // ✅ 把暂停这段 dsp 时长累加进 PausedDspTotal
        PausedDspTotal += (AudioSettings.dspTime - _pauseDspStart);

        isPaused = false;
        GlobalPause = false;

        Time.timeScale = 1f;
        if (bgm != null) bgm.UnPause();
        inputEnabled = true;
    }

    public void ForceUnpause()
    {
        if (isPaused)
        {
            PausedDspTotal += (AudioSettings.dspTime - _pauseDspStart);
        }

        isPaused = false;
        GlobalPause = false;
        Time.timeScale = 1f;
        inputEnabled = true;
    }

    // ---------------- Score / HP ----------------

    public void AddScore(int delta)
    {
        score = Mathf.Max(0, score + delta);

        // ✅ 同步到 GameManager（只同步数据，不改变玩法）
        if (GameManager.Instance != null) GameManager.Instance.AddScore(delta);

        RefreshUI();
    }

    public void LoseHp(int amount)
    {
        int dmg = Mathf.Abs(amount);
        hp = Mathf.Clamp(hp - dmg, 0, maxHp);

        // ✅ 你的 Miss 统一走扣血：这里顺手记 Miss 次数（不改变扣血逻辑）
        if (GameManager.Instance != null)
        {
            for (int i = 0; i < dmg; i++)
                GameManager.Instance.AddMiss();
        }

        RefreshUI();

        if (hp <= 0) Debug.Log("GAME OVER");
    }

    private void RefreshUI()
    {
        if (scoreText != null) scoreText.text = score.ToString("0000");
        if (heartsUI != null)
        {
            heartsUI.maxHp = maxHp;
            heartsUI.SetHP(hp);
        }
    }

    // ---------------- End Game -> ResultScene ----------------

    private void EndGame(bool success)
    {
        if (ended) return;
        ended = true;

        inputEnabled = false;

        // 收尾：停止刷怪 & 停音乐（不改原过程，只是在结算时收）
        if (chartSpawner != null) chartSpawner.StopSpawning();
        if (bgm != null) bgm.Stop();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(success ? GameState.Success : GameState.Fail);
        }

        if (flow == null) flow = FindObjectOfType<SceneFlow>(true);
        if (flow != null) flow.GoResult(success);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(SceneFlow.ResultScene);
    }

    // ---------------- Reflection helpers (unchanged) ----------------

    private ScriptableObject FindConfigByLevelType(ScriptableObject db, LevelType level)
    {
        object listObj = GetAny<object>(db,
            "Levels", "levels", "LevelConfigs", "levelConfigs", "Configs", "configs", "List", "list", "Items", "items");

        if (listObj == null) return null;

        IEnumerable enumerable = listObj as IEnumerable;
        if (enumerable == null) return null;

        foreach (var item in enumerable)
        {
            if (item == null) continue;

            object ltObj = GetAny<object>(item,
                "LevelType", "levelType", "difficulty", "Difficulty", "Diff", "diff");

            if (ltObj == null) continue;

            if (ltObj is LevelType ltEnum)
            {
                if (ltEnum.Equals(level)) return item as ScriptableObject;
            }
            else
            {
                string s = ltObj.ToString();
                if (string.Equals(s, level.ToString(), StringComparison.OrdinalIgnoreCase))
                    return item as ScriptableObject;
            }
        }
        return null;
    }

    private static T GetAny<T>(object obj, params string[] names)
    {
        if (obj == null) return default;

        Type type = obj.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var n in names)
        {
            var p = type.GetProperty(n, flags);
            if (p != null && typeof(T).IsAssignableFrom(p.PropertyType))
            {
                object v = p.GetValue(obj);
                if (v != null) return (T)v;
            }

            var f = type.GetField(n, flags);
            if (f != null && typeof(T).IsAssignableFrom(f.FieldType))
            {
                object v = f.GetValue(obj);
                if (v != null) return (T)v;
            }
        }
        return default;
    }

    private static void DumpPublicFields(object obj, string tag)
    {
        if (obj == null) return;
        var t = obj.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var fields = t.GetFields(flags);

        Debug.Log($"[{tag}] type={t.Name} fields:");
        foreach (var f in fields)
            Debug.Log($"[{tag}]  - {f.Name} : {f.FieldType.Name}");
    }
}
