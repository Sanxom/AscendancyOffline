using UnityEngine;

/// <summary>
/// A learnable skill/ability. Same shape as the old AttackDefinitionSO, but skills are now
/// learned individually per-character (via leveling, NPCs, or quests) rather than bundled
/// as fixed per-race kits. isBasicAttack marks the one special always-known skill.
/// </summary>
[CreateAssetMenu(menuName = "Combat/Skill Definition", fileName = "NewSkillDefinition")]
public class SkillDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public string skillId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Basic Attack")]
    [Tooltip("If true, every character knows this skill from creation — no learning required. Should be exactly one skill in the whole game.")]
    public bool isBasicAttack = false;

    [Header("Costs & Timing")]
    public float kiCost = 0f; // Basic Attack should be 0 — it's a physical punch, not ki-based
    public float cooldown = 1.5f;

    [Header("Damage Scaling")]
    [Tooltip("Damage = damageBase + (Strength * strengthScaling) + (Ki stat * kiScaling). Basic Attack: strengthScaling only.")]
    public float damageBase = 0f;
    public float strengthScaling = 1f;
    public float kiScaling = 0f;

    [Header("Projectile (leave projectilePrefab null for melee/instant skills like Basic Attack)")]
    public GameObject projectilePrefab;
    public ProjectileBehaviorSO behavior;
    public float speed = 20f;
    [Tooltip("Melee range check radius, used only when projectilePrefab is null.")]
    public float meleeRange = 1.2f;

    [Header("Multi-Projectile")]
    [Min(1)] public int projectileCount = 1;
    [Min(0f)] public float intraShotDelay = 0f;

    [Header("Targeting")]
    public TargetAcquisitionMode targetAcquisition = TargetAcquisitionMode.None;
    public float targetSearchRadius = 15f;
    [Range(0f, 180f)] public float targetSearchConeAngle = 45f;

    [Header("Learning (ignored for Basic Attack)")]
    public SkillUnlockType unlockType = SkillUnlockType.LevelThreshold;
    public int requiredLevel;
    [Tooltip("Stable id an NPC/quest system grants when this skill is taught/rewarded.")]
    public string requiredGrantId;

    /// <summary>Computes this skill's damage given the caster's current stats.</summary>
    public float CalculateDamage(float strength, float ki)
    {
        return damageBase + (strength * strengthScaling) + (ki * kiScaling);
    }
}

public enum SkillUnlockType
{
    LevelThreshold,   // auto-known once character reaches requiredLevel
    Granted           // only known if explicitly granted (NPC teaches it / quest reward) via requiredGrantId
}