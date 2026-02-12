using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Music Toggle (✓ / X)")]
    [Tooltip("拖: SettingsPanel/Canvas/Music/Checkmark")]
    public GameObject musicCheckmark;
    [Tooltip("拖: SettingsPanel/Canvas/Music/Crossmark")]
    public GameObject musicCrossmark;
    [Tooltip("可选：如果你用 Unity Button 来点 Music，就拖 Music 上的 Button；如果用 XR Activated 可不填")]
    public Button musicButton;

    [Header("Sound Toggle (✓ / X)")]
    [Tooltip("拖: SettingsPanel/Canvas/Sound/Checkmark")]
    public GameObject soundCheckmark;
    [Tooltip("拖: SettingsPanel/Canvas/Sound/Crossmark")]
    public GameObject soundCrossmark;
    [Tooltip("可选：如果你用 Unity Button 来点 Sound，就拖 Sound 上的 Button；如果用 XR Activated 可不填")]
    public Button soundButton;

    [Header("Buttons")]
    public Button saveButton;
    public Button cancelButton;
    public Button closeButton;

    [Header("Back To MainMenu")]
    [Tooltip("拖 MainMenu 场景里挂 MainMenuUI 的对象；不拖也行，会自动 Find")]
    public MainMenuUI mainMenuUI;

    [Header("Optional Audio")]
    [Tooltip("推荐：把你的BGM AudioSource拖进来，musicOn 会控制它 mute")]
    public AudioSource bgmSource;

    // --- Keys ---
    private const string MUSIC_ON_KEY = "SETTING_MUSIC_ON"; // int 0/1
    private const string SOUND_ON_KEY = "SETTING_SOUND_ON"; // int 0/1

    // --- Current state ---
    public bool musicOn = true;
    public bool soundOn = true;

    // --- Cache for Cancel ---
    private bool cachedMusicOn;
    private bool cachedSoundOn;

    private void Start()
    {
        // 保险：忘了拖就自动找
        if (mainMenuUI == null) mainMenuUI = FindObjectOfType<MainMenuUI>(true);

        LoadSettings();
        CacheCurrentValues();
        RefreshUI();
        ApplyAudio();

        // 可选：如果你还是用 Button.onClick（鼠标调试）
        if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
        if (soundButton != null) soundButton.onClick.AddListener(ToggleSound);

        if (saveButton != null) saveButton.onClick.AddListener(OnSave);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
        if (closeButton != null) closeButton.onClick.AddListener(OnCancel);
    }

    // --------------------------
    // Public API (XR/按钮都调用)
    // --------------------------

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        RefreshUI();
        ApplyAudio();
    }
    public void DebugMusicPing()
    {
        Debug.Log("MUSIC PING");
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;
        RefreshUI();
        ApplyAudio();
    }

    public void OnSave()
    {
        PlayerPrefs.SetInt(MUSIC_ON_KEY, musicOn ? 1 : 0);
        PlayerPrefs.SetInt(SOUND_ON_KEY, soundOn ? 1 : 0);
        PlayerPrefs.Save();

        CacheCurrentValues();
        CloseAndBackToMenu();
    }

    public void OnCancel()
    {
        musicOn = cachedMusicOn;
        soundOn = cachedSoundOn;

        RefreshUI();
        ApplyAudio();
        CloseAndBackToMenu();
    }

    // --------------------------
    // Internal
    // --------------------------

    private void CloseAndBackToMenu()
    {
        // 让主菜单负责“主菜单开/设置关”
        if (mainMenuUI != null)
        {
            mainMenuUI.OnCloseSettings();
        }
        else
        {
            // 兜底：至少把自己关掉
            gameObject.SetActive(false);
            Debug.LogWarning("[SettingsUI] mainMenuUI not assigned, only closing settings panel.");
        }
    }

    private void LoadSettings()
    {
        // 默认：开
        musicOn = PlayerPrefs.GetInt(MUSIC_ON_KEY, 1) == 1;
        soundOn = PlayerPrefs.GetInt(SOUND_ON_KEY, 1) == 1;
    }

    private void CacheCurrentValues()
    {
        cachedMusicOn = musicOn;
        cachedSoundOn = soundOn;
    }

    private void RefreshUI()
    {
        if (musicCheckmark != null) musicCheckmark.SetActive(musicOn);
        if (musicCrossmark != null) musicCrossmark.SetActive(!musicOn);

        if (soundCheckmark != null) soundCheckmark.SetActive(soundOn);
        if (soundCrossmark != null) soundCrossmark.SetActive(!soundOn);
    }

    private void ApplyAudio()
    {
        // ✅ Music：最推荐做法是控制 BGM 的 AudioSource
        if (bgmSource != null)
        {
            bgmSource.mute = !musicOn;
        }

        // ✅ Sound：通常应通过 AudioMixer 控制 SFX group
        // 你现在如果还没有 SFX 管理，就先只做 UI 状态切换
        // 以后你把 SFX 都从 AudioManager 播放，我再帮你接一个 SetSfxMute(soundOn) 即可
    }
}
