using System;
using UnityEngine;

namespace ARRhythm.Core
{
    // 关卡资源配置：对应 Resources 路径（不写后缀）
    [Serializable]
    public class LevelResources
    {
        public LevelType level = LevelType.Tutorial;

        [Tooltip("Resources 路径，例如：Charts/tutorial")]
        public string chartPath = "Charts/tutorial";

        [Tooltip("Resources 路径，例如：Music/tutorial")]
        public string musicPath = "Music/tutorial";

        [Tooltip("可选：该关卡默认BPM（如果你谱面里已经写了可不填）")]
        public float bpm = 0f;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ====== State / Level ======
        public GameState State { get; private set; } = GameState.Boot;
        public LevelType CurrentLevel { get; private set; } = LevelType.Tutorial;

        // ====== Run Data ======
        public int Score { get; private set; }
        public int HP { get; private set; } = MaxHP;
        public int MissCount { get; private set; } = 0;

        // ✅ 新增：命中统计（不改变原逻辑，只是记录）
        public int PerfectCount { get; private set; } = 0;
        public int GoodCount { get; private set; } = 0;

        public const int MaxHP = 10;
        public const int MaxMissAllowed = 10; // 允许最多10次miss，第11次才失败（见 IsFail）

        // ====== Events (UI订阅更方便) ======
        public event Action<LevelType> OnLevelSelected;
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHpChanged;
        public event Action<int> OnMissChanged;
        public event Action OnRunReset;

        [Header("Level Resources Mapping")]
        [Tooltip("在 Inspector 填 3 条：Tutorial/Medium/Hard 对应 Charts/xxx 和 Music/xxx")]
        public LevelResources[] levelResources = new LevelResources[]
        {
            new LevelResources{ level = LevelType.Tutorial, chartPath = "Charts/tutorial", musicPath = "Music/tutorial" },
            new LevelResources{ level = LevelType.Medium,   chartPath = "Charts/medium",   musicPath = "Music/medium"   },
            new LevelResources{ level = LevelType.Hard,     chartPath = "Charts/hard",     musicPath = "Music/hard"     },
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // =========================
        // State
        // =========================
        public void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(State);
        }

        // =========================
        // Level Select
        // =========================
        public void SelectLevel(LevelType level)
        {
            CurrentLevel = level;
            OnLevelSelected?.Invoke(CurrentLevel);

            // ✅ 推荐：选关就重置本局数据（避免上一次残留）
            ResetRunData();
        }

        public LevelResources GetCurrentLevelResources()
        {
            return GetLevelResources(CurrentLevel);
        }

        public LevelResources GetLevelResources(LevelType level)
        {
            if (levelResources != null)
            {
                for (int i = 0; i < levelResources.Length; i++)
                {
                    if (levelResources[i] != null && levelResources[i].level == level)
                        return levelResources[i];
                }
            }

            // 兜底：返回第一条
            if (levelResources != null && levelResources.Length > 0)
                return levelResources[0];

            return null;
        }

        // =========================
        // Run Data
        // =========================
        public void ResetRunData()
        {
            Score = 0;
            HP = MaxHP;
            MissCount = 0;

            // ✅ 新增：重置统计
            PerfectCount = 0;
            GoodCount = 0;

            OnRunReset?.Invoke();
            OnScoreChanged?.Invoke(Score);
            OnHpChanged?.Invoke(HP);
            OnMissChanged?.Invoke(MissCount);
        }

        public void AddScore(int delta)
        {
            Score += delta;
            if (Score < 0) Score = 0;
            OnScoreChanged?.Invoke(Score);
        }

        // ✅ 新增：统计接口
        public void AddPerfect() => PerfectCount += 1;
        public void AddGood() => GoodCount += 1;

        public void AddMiss()
        {
            MissCount += 1;
            OnMissChanged?.Invoke(MissCount);
            Damage(1);
        }

        public void Damage(int amount)
        {
            HP -= amount;
            if (HP < 0) HP = 0;
            OnHpChanged?.Invoke(HP);
        }

        /// <summary>
        /// 失败条件：HP<=0 或 MissCount > MaxMissAllowed（超过10次才失败）
        /// 如果你想“达到10次就失败”，改成 >=
        /// </summary>
        public bool IsFail()
        {
            return HP <= 0 || MissCount > MaxMissAllowed;
        }

        // =========================
        // Asset Loading (Resources)
        // =========================
        public TextAsset LoadCurrentChart()
        {
            var res = GetCurrentLevelResources();
            if (res == null) return null;
            return Resources.Load<TextAsset>(res.chartPath);
        }

        public AudioClip LoadCurrentMusic()
        {
            var res = GetCurrentLevelResources();
            if (res == null) return null;
            return Resources.Load<AudioClip>(res.musicPath);
        }
    }
}
