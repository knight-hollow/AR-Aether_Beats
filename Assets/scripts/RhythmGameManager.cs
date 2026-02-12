using UnityEngine;
using System.Collections.Generic;
using TMPro; // 引用 TextMeshPro 命名空间

public class RhythmGameManager : MonoBehaviour
{
    public static RhythmGameManager Instance;

    [Header("UI 组件")]
    public TextMeshPro scoreText; // 请在 Inspector 中拖入场景里的 ScoreText (TMP)

    [Header("游戏数据")]
    private int score = 0;       // 当前分数
    private int combo = 0;       // 当前连击

    [Header("判定时间 (秒)")]
    public float perfectTime = 0.05f; // 50ms
    public float greatTime = 0.1f;    // 100ms
    public float goodTime = 0.15f;    // 150ms

    // 存储4个轨道上的音符
    public List<NoteMover>[] activeNotes;

    void Awake()
    {
        Instance = this;
        // 初始化轨道列表
        activeNotes = new List<NoteMover>[4];
        for (int i = 0; i < 4; i++)
        {
            activeNotes[i] = new List<NoteMover>();
        }
    }

    void Start()
    {
        // 游戏开始时刷新一下 UI
        UpdateUI();
    }

    // ---------------- 音符管理 ----------------

    public void RegisterNote(int trackId, NoteMover note)
    {
        if (trackId >= 0 && trackId < 4)
        {
            activeNotes[trackId].Add(note);
        }
    }

    public void RemoveNote(int trackId, NoteMover note)
    {
        if (trackId >= 0 && trackId < 4 && activeNotes[trackId].Contains(note))
        {
            activeNotes[trackId].Remove(note);
        }
    }

    // ---------------- 核心判定逻辑 ----------------

    // 玩家主动点击 (Hit)
    public void CheckHit(int trackId)
    {
        // 如果该轨道没有音符，视为无效点击（也可以在这里加空挥扣分逻辑）
        if (activeNotes[trackId].Count == 0) return;

        // 获取最早生成的那个音符
        NoteMover targetNote = activeNotes[trackId][0];

        // 计算时间误差 (绝对值)
        double currentTime = AudioSettings.dspTime;
        float timeDiff = (float)System.Math.Abs(currentTime - targetNote.hitTime);

        if (timeDiff <= goodTime)
        {
            // === 击中 ===
            string rank = "Good";
            int addScore = 100;

            if (timeDiff <= perfectTime)
            {
                rank = "Perfect!!";
                addScore = 300;
            }
            else if (timeDiff <= greatTime)
            {
                rank = "Great!";
                addScore = 200;
            }

            // 加分 & 连击
            score += addScore;
            combo++;
            
            Debug.Log($"<color=green>击中! [{rank}] +{addScore}分</color>");

            // 更新 UI
            UpdateUI();

            // 销毁音符
            Destroy(targetNote.gameObject);
        }
        else
        {
            // === 点太早了 (算是失误) ===
            Debug.Log("点太早了，断连！");
            ResetCombo();
        }
    }

    // 音符飞出屏幕 (Miss) - 由 NoteMover 调用
    public void ReportMiss(int trackId)
    {
        Debug.Log($"<color=red>Miss! 轨道 {trackId}</color>");
        ResetCombo();
    }

    // ---------------- 辅助方法 ----------------

    void ResetCombo()
    {
        combo = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            // 更新 3D 文字的内容
            scoreText.text = $"Score: {score}\nCombo: {combo}";
        }
    }
}