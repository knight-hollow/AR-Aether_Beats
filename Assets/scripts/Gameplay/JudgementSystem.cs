using System.Collections.Generic;
using UnityEngine;

public class JudgementSystem : MonoBehaviour
{
    [Header("Windows (seconds)")]
    public float perfectWindow = 0.08f;
    public float goodWindow = 0.15f;
    public float badWindow = 0.25f;

    [Header("Score")]
    public int perfectScore = 100;
    public int goodScore = 60;
    public int badScore = 30;

    [Header("Refs")]
    public GameController game;

    private readonly List<Note>[] laneNotes = new List<Note>[4];

    private float inputLockUntil = 0f;
    public float inputLockSeconds = 0.08f;

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
            laneNotes[i] = new List<Note>();

        if (game == null) game = FindObjectOfType<GameController>(true);
    }

    public void Register(Note note)
    {
        if (note == null) return;
        int lane = Mathf.Clamp(note.laneIndex, 0, 3);
        if (!laneNotes[lane].Contains(note))
            laneNotes[lane].Add(note);
    }

    public void Unregister(Note note)
    {
        if (note == null) return;
        int lane = Mathf.Clamp(note.laneIndex, 0, 3);
        laneNotes[lane].Remove(note);
    }

    public bool CanAcceptLaneInput() => Time.time >= inputLockUntil;
    public bool CanAcceptLaneInput(int lane) => CanAcceptLaneInput();

    public void OnLanePressed(int lane)
    {
        // ✅ 暂停/全局暂停时不判定不扣血
        if (GameController.GlobalPause) return;
        if (game != null && game.isPaused) return;

        if (game != null && !game.inputEnabled) return;

        lane = Mathf.Clamp(lane, 0, 3);
        inputLockUntil = Time.time + inputLockSeconds;

        if (laneNotes[lane].Count == 0)
        {
            LoseHp(1);
            return;
        }

        Note note = laneNotes[lane][0];
        float t = Mathf.Abs(note.TimeToJudge());

        if (t <= perfectWindow)
        {
            AddScore(perfectScore);

            // ✅ 新增：只记录次数，不改变任何玩法逻辑
            if (ARRhythm.Core.GameManager.Instance != null)
                ARRhythm.Core.GameManager.Instance.AddPerfect();

            note.Hit();
        }
        else if (t <= goodWindow)
        {
            AddScore(goodScore);

            if (ARRhythm.Core.GameManager.Instance != null)
                ARRhythm.Core.GameManager.Instance.AddGood();

            note.Hit();
        }
        else if (t <= badWindow)
        {
            AddScore(badScore);

            // bad 也算“成功命中但较差”，你要的统计只有 good，所以归到 good
            if (ARRhythm.Core.GameManager.Instance != null)
                ARRhythm.Core.GameManager.Instance.AddGood();

            note.Hit();
        }
        else
        {
            LoseHp(1);
        }
    }

    public void OnNoteMissed(Note note)
    {
        if (GameController.GlobalPause) return;
        if (game != null && game.isPaused) return;
        LoseHp(1);
    }

    private void AddScore(int delta)
    {
        if (game != null) game.AddScore(delta);
    }

    private void LoseHp(int amount)
    {
        if (game != null) game.LoseHp(amount);
    }
}
