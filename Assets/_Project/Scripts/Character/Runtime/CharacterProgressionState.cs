using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterSkillState))]
[RequireComponent(typeof(CombatStats))]
public class CharacterProgressionState : MonoBehaviour
{
    [Header("Race Registry")]
    [SerializeField] private RaceDefinitionSO[] allRaces;
    [SerializeField] private CharacterStatsSO baseCharacterStats;
    [SerializeField] private LevelCurveSO levelCurve;

    private RaceDefinitionSO currentRace;
    private CombatStats stats;
    private CharacterSkillState skillState;

    private FixedString32Bytes raceId = new();
    private int currentTierIndex;
    private int level;
    private long currentXP;

    // How much of each stat has been gained via DIRECT training (machines/quests/self-training),
    // separate from the automatic per-level growth — tracked so respec/recalculation doesn't lose this progress.
    private float trainedStrengthBonus, trainedKiBonus, trainedKiDefenseBonus, trainedDefenseBonus;

    private readonly HashSet<string> earnedWorldTriggers = new();

    public event Action<int, int> OnTierChanged;
    public event Action<long, long> OnXPChanged;
    public event Action<int, int> OnLevelChanged;
    public event Action<RaceTier> OnTierUnlockAvailable;

    public RaceDefinitionSO CurrentRace => currentRace;
    public int CurrentTierIndex => currentTierIndex;
    public int Level => level;
    public long CurrentXP => currentXP;

    private void Awake()
    {
        stats = GetComponent<CombatStats>();
        skillState = GetComponent<CharacterSkillState>();
    }

    public void LoadFromSaveData(CharacterSaveData save)
    {
        currentRace = FindRace(save.raceId);
        if (currentRace == null)
        {
            Debug.LogError($"Unknown raceId '{save.raceId}' in save data.");
            return;
        }

        raceId = save.raceId;
        currentTierIndex = save.currentTierIndex;
        level = save.level;
        currentXP = save.currentXP;
        trainedStrengthBonus = save.trainedStrengthBonus;
        trainedKiBonus = save.trainedKiBonus;
        trainedKiDefenseBonus = save.trainedKiDefenseBonus;
        trainedDefenseBonus = save.trainedDefenseBonus;

        earnedWorldTriggers.Clear();
        if (save.unlockedWorldTriggerIds != null)
            foreach (var t in save.unlockedWorldTriggerIds) earnedWorldTriggers.Add(t);

        skillState.LoadKnownSkills(save.knownSkillIds);

        RecalculateStats();
    }

    public void WriteToSaveData(CharacterSaveData save)
    {
        save.raceId = raceId.Value.ToString();
        save.currentTierIndex = currentTierIndex;
        save.level = level;
        save.currentXP = currentXP;
        save.trainedStrengthBonus = trainedStrengthBonus;
        save.trainedKiBonus = trainedKiBonus;
        save.trainedKiDefenseBonus = trainedKiDefenseBonus;
        save.trainedDefenseBonus = trainedDefenseBonus;

        var triggers = new string[earnedWorldTriggers.Count];
        earnedWorldTriggers.CopyTo(triggers);
        save.unlockedWorldTriggerIds = triggers;

        save.knownSkillIds = skillState.GetKnownSkillIds();
    }

    /// <summary> Call from combat kills, quests, training machines, self-training — any XP source.</summary>
    public void GrantXP(long amount)
    {
        long newTotalXP = currentXP + amount;
        currentXP = newTotalXP;

        var (newLevel, _) = levelCurve.ResolveLevel(newTotalXP);
        if (newLevel != level)
        {
            level = newLevel;
            RecalculateStats();
            skillState.CheckLevelUnlocks(newLevel);  // auto-learn any level-gated skills just reached
        }

        CheckForAvailableTierUnlock();
    }

    /// <summary>
    /// Called by a training machine/quest/self-training source to raise a specific stat
    /// directly, separate from the automatic per-level growth. Caller is responsible for its own
    /// Fatigue/cooldown gating (via CombatStats.CanTrain) before calling this.
    /// </summary>
    public void ApplyDirectStatTraining(TrainableStat stat, float amount)
    {
        switch (stat)
        {
            case TrainableStat.Strength: trainedStrengthBonus += amount; break;
            case TrainableStat.Ki: trainedKiBonus += amount; break;
            case TrainableStat.KiDefense: trainedKiDefenseBonus += amount; break;
            case TrainableStat.Defense: trainedDefenseBonus += amount; break;
        }

        RecalculateStats();
    }

    /// <summary>
    /// Recomputes final stat values from: base stats + per-level automatic growth + direct training bonuses,
    /// then applies race/tier multipliers on top, and pushes the result into CombatStats.
    /// Single source of truth for "what are this character's stats right now" — called after any
    /// level-up, direct training, tier change, or respec.
    /// </summary>
    private void RecalculateStats()
    {
        if (currentRace == null) return;

        int levelsGained = level - 1;

        float rawPowerLevel = baseCharacterStats.startingPowerLevel + baseCharacterStats.powerLevelGrowthPerLevel * levelsGained;
        float rawStrength = baseCharacterStats.startingStrength + baseCharacterStats.strengthGrowthPerLevel * levelsGained + trainedStrengthBonus;
        float rawKi = baseCharacterStats.startingKi + baseCharacterStats.kiGrowthPerLevel * levelsGained + trainedKiBonus;
        float rawKiDefense = baseCharacterStats.startingKiDefense + baseCharacterStats.kiDefenseGrowthPerLevel * levelsGained + trainedKiDefenseBonus;
        float rawDefense = baseCharacterStats.startingDefense + baseCharacterStats.defenseGrowthPerLevel * levelsGained + trainedDefenseBonus;
        float rawSpeed = baseCharacterStats.startingSpeed + baseCharacterStats.speedGrowthPerLevel * levelsGained; // No direct-training bonus for Speed (not trainable via machines per current design; revisit if that changes)

        var (healthMult, kiMult, damageMult, defenseMult, kiDefenseMult, speedMult) = currentRace.GetEffectiveMultipliers(currentTierIndex);

        stats.SetCoreStats(
            rawPowerLevel * healthMult,
            rawStrength * damageMult,
            rawKi * kiMult,
            rawKiDefense * kiDefenseMult,
            rawDefense * defenseMult,
            rawSpeed * speedMult
        );
    }

    private void CheckForAvailableTierUnlock()
    {
        int nextTierIndex = currentTierIndex + 1;
        if (nextTierIndex > currentRace.MaxTierIndex) return;

        var nextTier = currentRace.GetTier(nextTierIndex);
        if (nextTier == null) return;

        if (IsUnlockSatisfied(nextTier))
            OnTierUnlockAvailable?.Invoke(nextTier);
    }

    private bool IsUnlockSatisfied(RaceTier tier)
    {
        bool levelOk = level >= tier.requiredLevel;
        bool powerLevelOk = stats.MaxPowerLevel >= tier.requiredPowerLevel;
        bool triggerOk = tier.unlockType != UnlockRequirementType.LevelThreshold
            && earnedWorldTriggers.Contains(tier.requiredTriggerId);

        return tier.unlockType switch
        {
            UnlockRequirementType.LevelThreshold => levelOk && powerLevelOk,
            UnlockRequirementType.WorldTrigger => triggerOk,
            UnlockRequirementType.LevelAndWorldTrigger => levelOk && powerLevelOk && triggerOk,
            _ => false
        };
    }

    public bool TryAdvanceTier(int targetTierIndex)
    {
        if (targetTierIndex != currentTierIndex + 1) return false;

        var tier = currentRace.GetTier(targetTierIndex);
        if (tier == null || !IsUnlockSatisfied(tier)) return false;

        currentTierIndex = targetTierIndex;
        RecalculateStats();
        return true;
    }

    public bool TryRespecToTier(int targetTierIndex, Func<int, bool> spendCurrencyCallback)
    {
        if (targetTierIndex < 0 || targetTierIndex > currentTierIndex) return false;

        var tier = currentRace.GetTier(targetTierIndex);
        int cost = tier?.respecCost ?? 0;
        if (!spendCurrencyCallback(cost)) return false;

        currentTierIndex = targetTierIndex;
        RecalculateStats();
        return true;
    }

    public void GrantWorldTrigger(string triggerId)
    {
        earnedWorldTriggers.Add(triggerId);
        CheckForAvailableTierUnlock();
    }

    private RaceDefinitionSO FindRace(string id)
    {
        foreach (var race in allRaces) if (race.raceId == id) return race;
        return null;
    }
}

public enum TrainableStat { Strength, Ki, KiDefense, Defense }