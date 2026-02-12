using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioSource bgmSource;

    [Header("Debug")]
    public bool autoPlayOnStart = false;

    // 音乐开始的 DSP 时间（用于高精度判定）
    private double dspStartTime;
    private bool isPlaying = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (autoPlayOnStart)
            PlayBGM();
    }

    /// <summary>
    /// 播放音乐（从头）
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSource == null || bgmSource.clip == null)
        {
            Debug.LogError("[AudioManager] BGM AudioSource / Clip 未设置");
            return;
        }

        bgmSource.Stop();
        bgmSource.time = 0f;

        dspStartTime = AudioSettings.dspTime;
        bgmSource.Play();

        isPlaying = true;
    }

    /// <summary>
    /// 当前音乐时间（秒）——给判定系统用
    /// </summary>
    public float MusicTime
    {
        get
        {
            if (!isPlaying) return 0f;
            return (float)(AudioSettings.dspTime - dspStartTime);
        }
    }

    public void Pause()
    {
        if (!isPlaying) return;
        bgmSource.Pause();
        isPlaying = false;
    }

    public void Resume()
    {
        if (bgmSource == null) return;
        bgmSource.UnPause();
        dspStartTime = AudioSettings.dspTime - bgmSource.time;
        isPlaying = true;
    }

    public void Stop()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
        isPlaying = false;
    }

    public bool IsPlaying => isPlaying;
}
