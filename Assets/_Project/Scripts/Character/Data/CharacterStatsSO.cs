using UnityEngine;

/// <summary>
/// Defines a character's starting stat values and the per-level automatic growth formula
/// applied to all 6 core stats when the character levels up via XP.
/// One shared asset (not per-race) — race identity is expressed via RaceDefinitionSO
/// multipliers applied on top of these base numbers.
/// </summary>
[CreateAssetMenu(menuName = "Progression/Character Stats", fileName = "CharacterStats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Starting Values (at Level 1, base race form)")]
    public float startingPowerLevel = 100f; // doubles as starting max HP
    public float startingStrength = 10f;
    public float startingKi = 10f;
    public float startingKiDefense = 10f;
    public float startingDefense = 10f;
    public float startingSpeed = 5f;   // tiles per second at base

    [Header("Fatigue")]
    public float maxFatigue = 100f;
    [Tooltip("Fatigue gained per training action tick — actual per-source amounts are set on the training source itself; this is a sane default.")]
    public float defaultFatiguePerTrainingTick = 5f;
    [Tooltip("Fatigue reduced per second while actively Resting.")]
    public float restRecoveryPerSecond = 10f;
    [Tooltip("Fatigue level at/above which further training is blocked entirely.")]
    public float fatigueTrainingBlockThreshold = 100f;

    [Header("Per-Level Automatic Growth")]
    [Tooltip("Flat amount each stat grows per level-up. Simple linear model for now — swap for a curve later if flat growth feels wrong once playtested.")]
    public float powerLevelGrowthPerLevel = 15f;
    public float strengthGrowthPerLevel = 2f;
    public float kiGrowthPerLevel = 2f;
    public float kiDefenseGrowthPerLevel = 1.5f;
    public float defenseGrowthPerLevel = 1.5f;
    public float speedGrowthPerLevel = 0.1f;   // small growth; speed shouldn't scale as dramatically as combat stats (Or maybe at all)
}