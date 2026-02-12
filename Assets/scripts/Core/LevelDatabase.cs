using UnityEngine;
using ARRhythm.Core;

[CreateAssetMenu(menuName = "ARRhythm/Level Database", fileName = "LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    public LevelConfig[] levels;

    public LevelConfig Get(LevelType type)
    {
        if (levels == null) return null;
        foreach (var lv in levels)
        {
            if (lv != null && lv.levelType == type) return lv;
        }
        return null;
    }
}
