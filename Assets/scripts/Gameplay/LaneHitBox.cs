using UnityEngine;

public class LaneHitBox : MonoBehaviour
{
    [Range(0,3)]
    public int laneIndex = 0;

    public JudgementSystem judge;

    void Awake()
    {
        if (judge == null) judge = FindObjectOfType<JudgementSystem>();
    }

    // ✅ 给 XR Simple Interactable / MRTK 的 OnClick 事件绑这个
    public void Press()
    {
        if (judge == null) return;
        judge.OnLanePressed(laneIndex);
    }

    // ✅ 备用：如果你用最普通的鼠标点（Editor里测试）
    void OnMouseDown()
    {
        Press();
    }
}
