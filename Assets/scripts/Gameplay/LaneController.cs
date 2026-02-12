using UnityEngine;

public class LaneController : MonoBehaviour
{
    [Header("Lane")]
    public int laneId = 0;

    [Header("Visual")]
    public Color pressColor = Color.yellow;

    private Material myMat;
    private Color originalColor;

    [Header("Refs")]
    public JudgementSystem judge;
    private GameController game;

    // 防止多个Collider导致“卡亮”
    private int overlapCount = 0;

    private void Start()
    {
        if (judge == null) judge = FindObjectOfType<JudgementSystem>(true);
        if (game == null) game = FindObjectOfType<GameController>(true);

        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            myMat = r.material;
            if (myMat.HasProperty("_Color"))
            {
                originalColor = myMat.color;
                myMat.color = originalColor; // 强制初始化为未按下
            }
        }

        overlapCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 🔒 游戏未开始/暂停：完全不响应
        if (game == null || !game.inputEnabled) return;

        // （可选）只允许手触发：如果你给手指尖 collider 设置了 Tag=Hand，就打开这行
        // if (!other.CompareTag("Hand")) return;

        // 🔒 全局输入锁：同一时刻只允许一个 lane 成功（防止多box同时亮）
        if (judge != null && !judge.CanAcceptLaneInput(laneId))
            return;

        overlapCount++;

        // 只在“第一次进入”时触发一次判定 + 变色
        if (overlapCount == 1)
        {
            SetColor(pressColor);
            judge?.OnLanePressed(laneId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 游戏未开始/暂停：不处理
        if (game == null || !game.inputEnabled) return;

        // if (!other.CompareTag("Hand")) return;

        overlapCount = Mathf.Max(0, overlapCount - 1);

        // 所有进入的Collider都离开后才恢复颜色
        if (overlapCount == 0)
        {
            SetColor(originalColor);
        }
    }

    private void SetColor(Color c)
    {
        if (myMat != null && myMat.HasProperty("_Color"))
            myMat.color = c;
    }
}
