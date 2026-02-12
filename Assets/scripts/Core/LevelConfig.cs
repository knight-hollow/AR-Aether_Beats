using UnityEngine;
using ARRhythm.Core;

[CreateAssetMenu(menuName = "ARRhythm/Level Config", fileName = "LevelConfig_")]
public class LevelConfig : ScriptableObject
{
    public LevelType levelType;

    [Header("Audio")]
    public AudioClip music;

    [Header("Chart (JSON)")]
    public TextAsset chartJson;

    [Header("Gameplay Params")]
    public float noteSpeed = 2.5f;   // 怪物移动速度（你也可以不用，按你现有note逻辑）
    public float startDelay = 1.0f;  // 倒计时后延迟(可选)
}
