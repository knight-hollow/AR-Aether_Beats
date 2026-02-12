using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class NoteInfo
{
    public float time;      // 打击时间
    public int trackIndex;  // 轨道
}

[System.Serializable]
public class MapData
{
    public string songName;
    public string difficulty;
    public List<NoteInfo> notes;
}

public class MusicSequencer : MonoBehaviour
{
    [Header("核心引用")]
    public AudioSource musicSource;
    public SimpleSpawner spawner;

    [Header("谱面设置")]
    public string chartFileName = "sun_Hard.json"; // 你的谱面文件名
    public float noteFlyTime = 2.0f; // 怪物飞行时间 (需与Spawner一致)

    [Header("流程控制")]
    public float startDelay = 3.0f;     // 游戏开始前的倒计时 (秒)
    public float ignoreNotesTime = 2.0f; // [关键] 移除谱面中最开始多少秒的音符？

    // 内部变量
    private List<NoteInfo> currentChart;
    private int nextNoteIndex = 0;
    private bool isPlaying = false;
    private double musicDspStartTime; // 音乐开始的绝对时间

    void Start()
    {
        // 1. 加载谱面并清理前3秒的怪
        LoadChart();
        
        // 2. 开始倒计时 (3秒后执行 StartGame)
        Debug.Log($"倒计时 {startDelay} 秒...");
        Invoke("StartGame", startDelay);
    }

    void LoadChart()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, chartFileName);

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            MapData data = JsonUtility.FromJson<MapData>(jsonContent);

            if (data != null)
            {
                currentChart = data.notes;
                
                // [核心修改] 按照你的要求，直接删除前 3 秒 (ignoreNotesTime) 的音符
                // 这样就不会出现怪物突然脸贴脸生成的情况
                int removedCount = currentChart.RemoveAll(n => n.time < ignoreNotesTime);
                
                Debug.Log($"谱面加载完毕。已按要求移除前 {ignoreNotesTime} 秒的 {removedCount} 个音符。剩余音符数: {currentChart.Count}");
            }
        }
        else
        {
            Debug.LogError("错误：找不到谱面文件！请检查 StreamingAssets 文件夹。");
        }
    }

    void StartGame()
    {
        if (currentChart == null) return;

        // [核心修改] 音乐从头开始 (0秒)
        musicSource.time = 0f;
        
        // 获取当前 DSP 时间，并立即播放
        double now = AudioSettings.dspTime;
        musicSource.PlayScheduled(now);
        
        // 记录音乐开始的时间基准
        musicDspStartTime = now;

        isPlaying = true;
        Debug.Log("倒计时结束！音乐开始播放，怪物开始生成。");
    }

    void Update()
    {
        if (!isPlaying || currentChart == null) return;

        // 计算当前音乐播放到了第几秒
        // 公式：当前真实时间 - 音乐开始的那一刻
        double currentSongTime = AudioSettings.dspTime - musicDspStartTime;

        // 检查谱面队列
        while (nextNoteIndex < currentChart.Count)
        {
            NoteInfo note = currentChart[nextNoteIndex];

            // 计算生成时刻
            // 例如：怪物要在 5.0秒 被击中，飞行时间 2.0秒
            // 那么它应该在 3.0秒 时生成
            double spawnTime = note.time - noteFlyTime;

            // 如果当前音乐时间 >= 生成时刻，就生成它
            if (currentSongTime >= spawnTime)
            {
                spawner.SpawnNote(note.trackIndex);
                nextNoteIndex++;
            }
            else
            {
                // 还没到时间，等下一帧
                break;
            }
        }
    }
}
