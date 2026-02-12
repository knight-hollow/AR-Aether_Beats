using UnityEngine;

public class NoteMover : MonoBehaviour
{
    [Header("自动设置 - 无需修改")]
    public Vector3 startPos;
    public Vector3 endPos;
    public double spawnTime;
    public double hitTime;
    public int trackIndex;

    void Start()
    {
        if (RhythmGameManager.Instance != null)
        {
            RhythmGameManager.Instance.RegisterNote(trackIndex, this);
        }
    }

    void Update()
    {
        double currentTime = AudioSettings.dspTime;
        double totalDuration = hitTime - spawnTime;
        
        // 计算当前飞行进度
        float progress = (float)((currentTime - spawnTime) / totalDuration);

        if (progress <= 1.2f) // 允许飞过一点点
        {
            transform.position = Vector3.Lerp(startPos, endPos, progress);
        }
        else
        {
            // === 飞太远了，视为 Miss ===
            if (RhythmGameManager.Instance != null)
            {
                // [新增] 告诉管理器：我漏掉了，请重置 Combo
                RhythmGameManager.Instance.ReportMiss(trackIndex);
            }
            
            // 销毁自己
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (RhythmGameManager.Instance != null)
        {
            RhythmGameManager.Instance.RemoveNote(trackIndex, this);
        }
    }
}