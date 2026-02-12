using System;
using System.Collections.Generic;
using UnityEngine;

public class ChartSpawner : MonoBehaviour
{
    [Header("Prefabs (4 lanes)")]
    public Note[] lanePrefabs = new Note[4];

    [Header("Lane Transforms (size = 4)")]
    public Transform[] spawnPoints = new Transform[4];
    public Transform[] judgePoints = new Transform[4];

    [Header("Parent")]
    public Transform notesRoot;

    [Header("Timing / Move")]
    [Tooltip("怪物从 SpawnPoint 走到 JudgePoint 用多少秒（决定提前多久生成）")]
    public float travelTime = 2.0f;

    [Tooltip("过了 JudgePoint 之后继续走多远再销毁/判 Miss（单位：米）")]
    public float postJudgeDistance = 0.6f;

    [Tooltip("判 Miss 的宽限（秒），超过 (noteTime + missGrace) 还没击中就 Miss")]
    public float missGraceSeconds = 0.25f;

    private GameController game;
    private AudioSource bgm;
    private JudgementSystem judge;

    private TextAsset chartJson;
    private ChartData chart;
    private int nextIndex = 0;

    private bool running = false;
    private double dspStart = 0;

    // ✅ 只读：供 GameController 判断是否通关（不改变任何刷怪逻辑）
    public bool IsFinished => !running;

    [Serializable]
    private class ChartNote
    {
        public float time;
        public int trackIndex;
    }

    [Serializable]
    private class ChartData
    {
        public string songName;
        public string difficulty;
        public List<ChartNote> notes;
    }

    public void Configure(TextAsset chartJson, AudioSource bgm, GameController gameController)
    {
        this.chartJson = chartJson;
        this.bgm = bgm;
        this.game = gameController;

        if (judge == null) judge = FindObjectOfType<JudgementSystem>(true);

        ParseChart();
        ResetSpawning();
    }

    public void ResetSpawning()
    {
        nextIndex = 0;
        running = false;

        if (notesRoot != null)
        {
            for (int i = notesRoot.childCount - 1; i >= 0; i--)
                Destroy(notesRoot.GetChild(i).gameObject);
        }
    }

    public void StartWithMusic()
    {
        if (chart == null) ParseChart();
        if (chart == null || chart.notes == null)
        {
            Debug.LogError("[ChartSpawner] Chart is null / notes missing.");
            return;
        }

        if (bgm == null)
        {
            Debug.LogError("[ChartSpawner] BGM AudioSource is null.");
            return;
        }

        bgm.Stop();
        bgm.time = 0f;

        // ✅ 用“校正后的 dsp time”作为起点（暂停期间不会推进）
        dspStart = GameController.NowDspCorrected;

        bgm.Play();
        running = true;

        Debug.Log($"[ChartSpawner] StartWithMusic. notes={chart.notes.Count}, travelTime={travelTime}");
    }

    public void StopSpawning()
    {
        running = false;
        if (bgm != null) bgm.Stop();
    }

    private void Update()
    {
        // 暂停：不刷怪
        if (GameController.GlobalPause) return;
        if (game != null && game.isPaused) return;

        if (!running) return;
        if (chart == null || chart.notes == null) return;

        // ✅ 关键：songTime 用校正后的 dsp 计算（暂停期间不会“时间跳跃”）
        double songTime = GameController.NowDspCorrected - dspStart;

        while (nextIndex < chart.notes.Count)
        {
            var n = chart.notes[nextIndex];
            double spawnAt = n.time - travelTime;

            if (songTime >= spawnAt)
            {
                SpawnOne(n);
                nextIndex++;
            }
            else break;
        }

        if (nextIndex >= chart.notes.Count && bgm != null && !bgm.isPlaying)
        {
            running = false;
        }
    }

    private void SpawnOne(ChartNote chartNote)
    {
        int lane = chartNote.trackIndex;
        if (lane < 0 || lane >= 4) return;

        Transform sp = (spawnPoints != null && spawnPoints.Length > lane) ? spawnPoints[lane] : null;
        Transform jp = (judgePoints != null && judgePoints.Length > lane) ? judgePoints[lane] : null;
        Note prefab = (lanePrefabs != null && lanePrefabs.Length > lane) ? lanePrefabs[lane] : null;

        if (sp == null || jp == null || prefab == null)
        {
            Debug.LogWarning($"[ChartSpawner] lane {lane} missing refs (spawn/judge/prefab).");
            return;
        }

        if (notesRoot == null) notesRoot = transform;

        Note note = Instantiate(prefab, sp.position, sp.rotation, notesRoot);

        float distance = Vector3.Distance(sp.position, jp.position);
        float speed = (travelTime <= 0.0001f) ? 1f : distance / travelTime;

        note.Init(
            laneIndex: lane,
            spawnPoint: sp,
            judgePoint: jp,
            expectedHitTime: chartNote.time,
            moveSpeed: speed,
            postJudgeDistance: postJudgeDistance,
            missGraceSeconds: missGraceSeconds,
            judgeSystem: judge,
            game: game,
            spawnerDspStart: dspStart // ✅ 传“校正后的起点”
        );
    }

    private void ParseChart()
    {
        chart = null;

        if (chartJson == null)
        {
            Debug.LogWarning("[ChartSpawner] chartJson is null.");
            return;
        }

        try
        {
            chart = JsonUtility.FromJson<ChartData>(chartJson.text);
            if (chart != null && chart.notes != null)
            {
                chart.notes.Sort((a, b) => a.time.CompareTo(b.time));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[ChartSpawner] ParseChart failed: " + e);
        }
    }
}
