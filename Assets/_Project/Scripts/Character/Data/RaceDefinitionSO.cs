using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Race Definition", fileName = "NewRaceDefinition")]
public class RaceDefinitionSO : ScriptableObject
{
    public string raceId;
    public string displayName;
    [TextArea] public string description;
    public Sprite portraitIcon;

    [Header("Base Form Multipliers (tier 0 — the race's innate identity before any transformation)")]
    public float baseHealthMultiplier = 1f;
    public float baseKiMultiplier = 1f;
    public float baseDamageMultiplier = 1f;
    public float baseKiRegenMultiplier = 1f;
    public float baseDefenseMultiplier = 1f;      // NEW
    public float baseKiDefenseMultiplier = 1f;    // NEW

    public RaceTier[] tiers;

    /// <summary>Returns the effective multipliers for a given tier index, including tier 0 (base race).</summary>
    public (float health, float ki, float damage, float kiRegen, float defense, float kiDefense) GetEffectiveMultipliers(int tierIndex)
    {
        if (tierIndex <= 0)
        {
            return (baseHealthMultiplier, baseKiMultiplier, baseDamageMultiplier, baseKiRegenMultiplier, baseDefenseMultiplier, baseKiDefenseMultiplier);
        }

        var tier = GetTier(tierIndex);
        if (tier == null)
        {
            return (baseHealthMultiplier, baseKiMultiplier, baseDamageMultiplier, baseKiRegenMultiplier, baseDefenseMultiplier, baseKiDefenseMultiplier);
        }

        return (tier.healthMultiplier, tier.kiMultiplier, tier.damageMultiplier, tier.kiRegenMultiplier, tier.defenseMultiplier, tier.kiDefenseMultiplier);
    }

    public RaceTier GetTier(int tierIndex)
    {
        if (tierIndex <= 0 || tierIndex > tiers.Length) return null;
        return tiers[tierIndex - 1];
    }

    public int MaxTierIndex => tiers.Length;
}