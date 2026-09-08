using UnityEngine;

/// <summary>
/// DEV-ONLY: logs combat events to the Console so you can confirm skill casts are actually
/// resolving, without needing a HUD yet. Subscribes to AttackController's
/// existing events rather than adding logging inside the combat code itself.
/// </summary>
[RequireComponent(typeof(AttackController))]
[RequireComponent(typeof(CombatStats))]
public class CombatDebugLogger : MonoBehaviour
{
    private AttackController attackController;
    private CombatStats stats;

    private void Awake()
    {
        attackController = GetComponent<AttackController>();
        stats = GetComponent<CombatStats>();
    }

    private void OnEnable()
    {
        attackController.OnAttackRequested += HandleAttackRequested;
        attackController.OnAttackFired += HandleAttackFired;
        attackController.OnAttackRejected += HandleAttackRejected;
    }

    private void OnDisable()
    {
        attackController.OnAttackRequested -= HandleAttackRequested;
        attackController.OnAttackFired -= HandleAttackFired;
        attackController.OnAttackRejected -= HandleAttackRejected;
    }

    private void HandleAttackRequested(SkillDefinitionSO skill)
    {
        Debug.Log($"[Combat] Requested: {skill.displayName} ({skill.skillId})");
    }

    private void HandleAttackFired(SkillDefinitionSO skill, Vector3 firePosition)
    {
        Debug.Log($"[Combat] FIRED: {skill.displayName} at {firePosition} — Strength: {stats.Strength}, Ki: {stats.CurrentKi}/{stats.MaxKi}");
    }

    private void HandleAttackRejected(SkillDefinitionSO skill)
    {
        //Debug.LogWarning($"[Combat] REJECTED: {skill.displayName} — PowerLevel: {stats.CurrentPowerLevel}, Ki: {stats.CurrentKi}/{stats.CanAffordKi(skill.kiCost)}");
    }
}