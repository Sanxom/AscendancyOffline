using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[RequireComponent(typeof(CombatStats))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(CharacterSkillState))]
public class AttackController : MonoBehaviour
{
    [SerializeField] private SkillDefinitionSO[] allSkillsInGame; // full catalog, for id -> SO lookup

    private readonly Dictionary<string, float> localCooldownExpiry = new();

    private CombatStats stats;
    private PlayerMovementController movement;
    private CharacterSkillState skillState;

    public event Action<SkillDefinitionSO> OnAttackRequested;
    public event Action<SkillDefinitionSO> OnAttackRejected;
    public event Action<SkillDefinitionSO, Vector3> OnAttackFired;

    private void Awake()
    {
        stats = GetComponent<CombatStats>();
        movement = GetComponent<PlayerMovementController>();
        skillState = GetComponent<CharacterSkillState>();
    }

    /// <summary>
    /// Call from input, passing the skillId the player has this bound to a key (via their keybind
    /// preferences), NOT a fixed slot index. Pass Vector2Int.zero for aimDirection to default to facing.
    /// </summary>
    public void TryUseSkill(string skillId, Vector2Int aimDirection = default, int targetId = default)
    {
        var skill = FindSkill(skillId);
        if (skill == null) return;

        if (!skillState.KnowsSkill(skillId)) return;

        if (aimDirection == Vector2Int.zero)
        {
            Vector2Int facing = movement.FacingDirection;
            aimDirection = new Vector2Int(facing.x, facing.y);
        }

        if (localCooldownExpiry.TryGetValue(skillId, out float expiry) && Time.time < expiry)
        {
            OnAttackRejected?.Invoke(skill);
            return;
        }

        localCooldownExpiry[skillId] = Time.time + skill.cooldown;
        OnAttackRequested?.Invoke(skill);

        if (stats.CurrentPowerLevel <= 0f)
        {
            RejectAttack(skillId);
            return;
        }

        if (!stats.CanAffordKi(skill.kiCost))
        {
            RejectAttack(skillId);
            return;
        }

        if (!stats.IsAttackOffCooldown(skill.skillId, Time.time))
        {
            RejectAttack(skillId);
            return;
        }

        // TODO: Validate target still exists if we used homing?

        if (skill.kiCost > 0f) stats.SpendKi(skill.kiCost);
        stats.SetAttackCooldown(skill.skillId, Time.time + skill.cooldown);

        if (skill.projectilePrefab != null)
        {
            StartCoroutine(SpawnProjectileSequence(skill, aimDirection));
        }
        else
        {
            ResolveMeleeSkill(skill, aimDirection, targetId);
        }
    }

    /// <summary>Melee/instant skills (e.g. Basic Attack) — no projectile, resolves immediately via a range+cone check.</summary>
    private void ResolveMeleeSkill(SkillDefinitionSO skill, Vector2Int aimDirection, int targetId)
    {
        //var target = CombatantRegistry.FindNearestInCone(
        //    transform.position, aimDirection, skill.meleeRange, 60f, targetId);
        //var target = CombatantRegistry.FindAtTile(aimDirection);

        //if (target != null && target.TryGetComponent<CombatStats>(out var targetStats))
        //{
        //    float damage = skill.CalculateDamage(stats.Strength, 0f); // Basic Attack: kiScaling is 0, so ki param is irrelevant here
        //    HitResolver.ResolveHit(targetStats, skill, damage, targetId);
        //}
    }

    private IEnumerator SpawnProjectileSequence(SkillDefinitionSO skill, Vector2Int aimDirection)
    {
        GameObject homingTarget = null;
        if (skill.behavior is HomingBehaviorSO)
        {
            homingTarget = AcquireTarget(skill, aimDirection);
        }

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle);

        for (int i = 0; i < skill.projectileCount; i++)
        {
            SpawnSingleProjectile(skill, aimDirection, spawnRotation, homingTarget);

            if (skill.intraShotDelay > 0f && i < skill.projectileCount - 1)
            {
                yield return new WaitForSeconds(skill.intraShotDelay);
            }
        }
    }

    private void SpawnSingleProjectile(SkillDefinitionSO skill, Vector2Int direction, Quaternion spawnRotation, GameObject homingTarget)
    {
        Vector2Int spawnTile = movement.CurrentTile + direction; // spawns one tile ahead of the caster, in the aim direction

        var go = Instantiate(skill.projectilePrefab, GridUtility.TileToWorld(spawnTile), spawnRotation);

        float damage = skill.CalculateDamage(stats.Strength, stats.MaxKi);

        var projectile = go.GetComponent<Projectile>();
        projectile.Initialize(skill, damage, direction, spawnTile, homingTarget);
    }

    private GameObject AcquireTarget(SkillDefinitionSO skill, Vector2Int aimDirection)
    {
        return null;
        //return skill.targetAcquisition switch
        //{
        //    //TargetAcquisitionMode.NearestInCone => CombatantRegistry.FindNearestInCone(
        //    //    transform.position, aimDirection, skill.targetSearchRadius, skill.targetSearchConeAngle, OwnerClientId),
        //    //TargetAcquisitionMode.LockedReticle => CombatantRegistry.ValidateLockedTarget(
        //    //    lockedTargetNetworkId, transform.position, skill.targetSearchRadius, OwnerClientId),
        //    //_ => null
        //};
    }

    private Vector3 GetMuzzlePosition()
    {
        Vector2 worldPos = GridUtility.TileToWorld(movement.CurrentTile);   // Reads the grid tile, not transform.position
        Vector2Int facing = movement.FacingDirection;
        return new Vector3(worldPos.x, worldPos.y, 0f) + new Vector3(facing.x, facing.y, 0f) * GridUtility.TILE_SIZE * 0.5f;
    }

    private void NotifyAttackFired(string skillId, Vector3 firePosition)
    {
        var skill = FindSkill(skillId);
        if (skill != null) OnAttackFired?.Invoke(skill, firePosition);
    }

    private void RejectAttack(string skillId)
    {
        localCooldownExpiry[skillId] = Time.time;
        var skill = FindSkill(skillId);
        if (skill != null) OnAttackRejected?.Invoke(skill);
    }

    private SkillDefinitionSO FindSkill(string skillId) => Array.Find(allSkillsInGame, s => s.skillId == skillId);
}