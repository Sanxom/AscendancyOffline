using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatStats : MonoBehaviour
{
    [Header("Base Stats (from data, applied on spawn)")]
    [SerializeField] private CharacterStatsSO baseStats;

    // Power Level IS max/current HP
    private float currentPowerLevel;
    private float maxPowerLevel;
    private float currentKi;
    private float maxKi;
    private float strength;
    private float kiDefense;
    private float defense;
    private float fatigue;
    private float speed;

    private float currentKiRegenPerSecond;

    private readonly Dictionary<string, float> cooldownExpiryByAttackId = new();

    public event Action<float, float> OnPowerLevelChanged;
    public event Action<float, float> OnKiChanged;
    public event Action<float, float> OnFatigueChanged;
    public event Action<ulong> OnDied;

    private void Awake()
    {
        maxPowerLevel = baseStats.startingPowerLevel;
        currentPowerLevel = baseStats.startingPowerLevel;
        maxKi = baseStats.startingKi;
        currentKi = baseStats.startingKi;
        strength = baseStats.startingStrength;
        kiDefense = baseStats.startingKiDefense;
        defense = baseStats.startingDefense;
        fatigue = 0f;
        speed = baseStats.startingSpeed;
        currentKiRegenPerSecond = 5f; // placeholder flat rate — could scale off max Ki later
    }

    private void Update()
    {
        if (currentPowerLevel <= 0f) return;
        if (currentKi >= maxKi) return;
        if (currentKiRegenPerSecond <= 0f) return;

        currentKi = Mathf.Min(maxKi, currentKi + currentKiRegenPerSecond * Time.deltaTime);
    }

    // --- Ki (attacks drain current; max grows via stat gains) ---
    public bool CanAffordKi(float amount) => currentKi >= amount;
    public void SpendKi(float amount) => currentKi = Mathf.Max(0f, currentKi - amount);
    public void RestoreKi(float amount) => currentKi = Mathf.Min(maxKi, currentKi + amount);

    // --- Cooldowns ---
    public bool IsAttackOffCooldown(string attackId, float currentTime)
    {
        if (!cooldownExpiryByAttackId.TryGetValue(attackId, out float expiry)) return true;
        return currentTime >= expiry;
    }
    public void SetAttackCooldown(string attackId, float expiryTime) => cooldownExpiryByAttackId[attackId] = expiryTime;

    // --- Power Level / HP ---
    public void ApplyDamage(float amount, ulong attackerId)
    {
        if (currentPowerLevel <= 0f) return;

        currentPowerLevel = Mathf.Max(0f, currentPowerLevel - amount);
        if (currentPowerLevel <= 0f) OnDied?.Invoke(attackerId);
    }

    public void ApplyHealing(float amount)
    {
        if (currentPowerLevel <= 0f) return;
        currentPowerLevel = Mathf.Min(maxPowerLevel, currentPowerLevel + amount);
    }

    public void ResetForRespawn()
    {
        currentPowerLevel = maxPowerLevel;
        currentKi = maxKi;
        cooldownExpiryByAttackId.Clear();
    }

    // --- Fatigue ---
    public bool CanTrain() => fatigue < baseStats.fatigueTrainingBlockThreshold;
    public void AddFatigue(float amount) => fatigue = Mathf.Min(baseStats.maxFatigue, fatigue + amount);
    public void ReduceFatigue(float amount) => fatigue = Mathf.Max(0f, fatigue - amount);

    /// <summary>Called by CharacterProgressionState when stats grow (level-up or direct training).</summary>
    public void SetCoreStats(float newMaxPowerLevel, float newStrength, float newMaxKi, float newKiDefense, float newDefense, float newSpeed)
    {
        float powerRatio = maxPowerLevel > 0 ? currentPowerLevel / maxPowerLevel : 1f;
        maxPowerLevel = newMaxPowerLevel;
        currentPowerLevel = maxPowerLevel * powerRatio;
        strength = newStrength;
        kiDefense = newKiDefense;
        defense = newDefense;
        float kiRatio = maxKi > 0 ? currentKi / maxKi : 1f;
        maxKi = newMaxKi;
        currentKi = maxKi * kiRatio;
        speed = newSpeed;
    }

    public float CurrentPowerLevel => currentPowerLevel;
    public float MaxPowerLevel => maxPowerLevel;
    public float Strength => strength;
    public float CurrentKi => currentKi;
    public float MaxKi => maxKi;
    public float KiDefense => kiDefense;
    public float Defense => defense;
    public float Fatigue => fatigue;
    public float Speed => speed;
}