using UnityEngine;

/// <summary>
/// Defines the XP-to-level curve for the game. A single shared asset (not per-race) —
/// level is a universal character stat; race/tier is a separate axis of progression.
/// </summary>
[CreateAssetMenu(menuName = "Progression/Level Curve", fileName = "LevelCurve")]
public class LevelCurveSO : ScriptableObject
{
    [Tooltip("Max level obtainable.")]
    public int maxLevel = 100;

    [Header("Curve Shape")]
    [Tooltip("Base XP required to go from level 1 to level 2.")]
    public long baseXPRequirement = 100;

    [Tooltip("Exponential growth factor per level. 1.0 = linear, higher = steeper late-game grind.")]
    public float growthExponent = 1.15f;

    /// <summary>XP required to advance FROM the given level TO the next one.</summary>
    public long GetXPRequiredForLevel(int level)
    {
        if (level >= maxLevel) return long.MaxValue; // can't level past max
        return (long)(baseXPRequirement * Mathf.Pow(level, growthExponent));
    }

    /// <summary>Given a total accumulated XP, returns the resulting level and leftover XP within that level.</summary>
    public (int level, long xpIntoLevel) ResolveLevel(long totalXP)
    {
        int level = 1;
        long remaining = totalXP;

        while (level < maxLevel)
        {
            long required = GetXPRequiredForLevel(level);
            if (remaining < required) break;

            remaining -= required;
            level++;
        }

        return (level, remaining);
    }
}