using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [Header("资源引用")]
    // [修改] 这里变成了数组，可以放多个不同的怪物
    public GameObject[] notePrefabs; 
    
    public Transform[] spawnPoints; // 拖入 Spawn_0 ~ Spawn_3
    public Transform[] hitZones;    // 拖入 Lane_0 ~ Lane_3

    [Header("测试设置")]
    public float flyTime = 2.0f; 

    void Update()
    {
        // 按空格键，在第0轨道生成
        if (Input.GetKeyDown(KeyCode.Space)) SpawnNote(0);
        // 按 1, 2, 3 键分别在其他轨道生成
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnNote(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnNote(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnNote(3);
    }

    public void SpawnNote(int trackIndex)
    {
        // [新增] 安全检查：防止数组越界或为空
        if (trackIndex >= notePrefabs.Length || notePrefabs[trackIndex] == null)
        {
            Debug.LogError($"错误：轨道 {trackIndex} 没有配置对应的怪物预制体！");
            return;
        }

        // [修改] 根据轨道编号 (trackIndex) 从数组里取出对应的怪物
        GameObject prefabToUse = notePrefabs[trackIndex];

        // 1. 实例化选中的怪物
        GameObject obj = Instantiate(prefabToUse);
        
        // 2. 获取组件并赋值 (逻辑不变)
        NoteMover mover = obj.GetComponent<NoteMover>();
        if (mover != null)
        {
            mover.trackIndex = trackIndex;
            mover.startPos = spawnPoints[trackIndex].position;
            mover.endPos = hitZones[trackIndex].position;

            double now = AudioSettings.dspTime;
            mover.spawnTime = now;
            mover.hitTime = now + flyTime;
        }
        else
        {
            Debug.LogError("错误：你的新怪物预制体上忘记挂 'NoteMover' 脚本了！");
        }
    }
}