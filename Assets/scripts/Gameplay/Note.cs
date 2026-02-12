using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool drawGizmos = false;

    [Header("Facing (解决背对玩家)")]
    [SerializeField] private bool alignFacingToMoveDir = true;
    [SerializeField] private bool invertFacing = false;

    public int laneIndex { get; private set; } = 0;
    public Transform judgePoint { get; set; }

    private Transform spawnPoint;
    private float expectedHitTime;
    private float moveSpeed;
    private float postJudgeDistance;
    private float missGraceSeconds;

    private JudgementSystem judge;
    private GameController game;

    private double spawnerDspStart;

    private bool hit = false;
    private bool missed = false;

    private Vector3 dir;
    private Vector3 judgePos;

    public void Init(
        int laneIndex,
        Transform spawnPoint,
        Transform judgePoint,
        float expectedHitTime,
        float moveSpeed,
        float postJudgeDistance,
        float missGraceSeconds,
        JudgementSystem judgeSystem,
        GameController game,
        double spawnerDspStart
    )
    {
        this.laneIndex = laneIndex;
        this.spawnPoint = spawnPoint;
        this.judgePoint = judgePoint;
        this.expectedHitTime = expectedHitTime;
        this.moveSpeed = moveSpeed;
        this.postJudgeDistance = postJudgeDistance;
        this.missGraceSeconds = missGraceSeconds;
        this.judge = judgeSystem;
        this.game = game;
        this.spawnerDspStart = spawnerDspStart;

        judgePos = (judgePoint != null) ? judgePoint.position : transform.position;

        dir = (judgePos - transform.position);
        dir.y = 0f;
        dir = (dir.sqrMagnitude < 1e-6f) ? transform.forward : dir.normalized;

        if (alignFacingToMoveDir)
        {
            Vector3 lookDir = invertFacing ? -dir : dir;
            if (lookDir.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        hit = false;
        missed = false;

        judge?.Register(this);
    }

    public void Init(int laneIndex, Transform judgePoint, JudgementSystem judgeSystem, float moveSpeed)
    {
        Init(
            laneIndex: laneIndex,
            spawnPoint: null,
            judgePoint: judgePoint,
            expectedHitTime: 0f,
            moveSpeed: moveSpeed,
            postJudgeDistance: 0.6f,
            missGraceSeconds: 0.25f,
            judgeSystem: judgeSystem,
            game: FindObjectOfType<GameController>(true),
            spawnerDspStart: GameController.NowDspCorrected
        );
    }

    private void Update()
    {
        if (GameController.GlobalPause) return;
        if (game != null && game.isPaused) return;

        if (judgePoint == null) return;
        if (game != null && !game.inputEnabled) return;

        transform.position += dir * (moveSpeed * Time.deltaTime);

        float passed = Vector3.Dot(transform.position - judgePos, dir);

        if (!hit && !missed)
        {
            // ✅ 关键：用校正后的 dsp 时间（暂停期间不会推进）
            float songTime = (float)(GameController.NowDspCorrected - spawnerDspStart);

            if (expectedHitTime > 0.0001f)
            {
                if (songTime > expectedHitTime + missGraceSeconds)
                {
                    DoMiss();
                    return;
                }
            }
            else
            {
                if (passed > postJudgeDistance)
                {
                    DoMiss();
                    return;
                }
            }
        }

        if (passed > postJudgeDistance + 0.5f)
        {
            CleanupAndDestroy();
        }
    }

    public float TimeToJudge()
    {
        if (expectedHitTime <= 0.0001f) return 0f;

        // ✅ 同样使用校正后的 dsp 时间
        float songTime = (float)(GameController.NowDspCorrected - spawnerDspStart);
        return expectedHitTime - songTime;
    }

    public void Hit()
    {
        if (hit || missed) return;
        hit = true;

        judge?.Unregister(this);
        CleanupAndDestroy();
    }

    private void DoMiss()
    {
        if (hit || missed) return;
        missed = true;

        judge?.Unregister(this);
        judge?.OnNoteMissed(this);

        CleanupAndDestroy();
    }

    private void CleanupAndDestroy()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        judge?.Unregister(this);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + dir);
    }
}
