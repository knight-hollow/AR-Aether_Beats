using UnityEngine;
using UnityEngine.UI;

public class HealthHeartsUI : MonoBehaviour
{
    [Header("Heart Images (size = Max HP)")]
    public Image[] hearts;                 // 10个心Image，按顺序拖进来

    [Header("Sprites")]
    public Sprite heartFull;
    public Sprite heartEmpty;

    [Header("HP")]
    public int maxHp = 10;

    private int hp;

    public int HP => hp;

    void Awake()
    {
        hp = maxHp;
        Refresh();
    }

    public void SetHP(int value)
    {
        hp = Mathf.Clamp(value, 0, maxHp);
        Refresh();
    }

    public void ResetHP()
    {
        hp = maxHp;
        Refresh();
    }

    public void LoseOneHeart()
    {
        SetHP(hp - 1);
    }

    private void Refresh()
    {
        if (hearts == null || hearts.Length == 0) return;

        // 如果你拖了10个心，这里会按顺序点亮/熄灭
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            bool filled = i < hp;
            hearts[i].sprite = filled ? heartFull : heartEmpty;
            hearts[i].enabled = true;
        }
    }
}
