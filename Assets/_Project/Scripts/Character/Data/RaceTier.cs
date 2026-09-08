using UnityEngine;

/// <summary>
/// One tier within a race's linear progression chain (e.g. Human -> "Awakened Human" -> "Ascended Human").
/// Tier 0 is always the base race form itself.
/// </summary>
[System.Serializable]
public class RaceTier
{
    public string tierId;
    public string displayName;
    public Sprite icon;

    [Header("Stat Multipliers (applied on top of base CharacterStatsSO)")]
    public float healthMultiplier = 1f;
    public float kiMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float kiRegenMultiplier = 1f;
    public float defenseMultiplier = 1f;
    public float kiDefenseMultiplier = 1f;

    [Header("Unlock Requirements")]
    public UnlockRequirementType unlockType = UnlockRequirementType.LevelThreshold;
    public int requiredLevel;
    public float requiredPowerLevel;
    public string requiredTriggerId;

    [Header("Respec")]
    public int respecCost;
}

public enum UnlockRequirementType
{
    LevelThreshold,     // requiredLevel only
    WorldTrigger,        // requiredTriggerId only
    LevelAndWorldTrigger  // both must be satisfied
}