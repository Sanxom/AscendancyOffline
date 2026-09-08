using UnityEngine;

/// <summary>
/// Data-driven definition for a single attack/skill. New attacks are created
/// entirely as asset instances of this SO — no new code required — by combining
/// base parameters with a ProjectileBehaviorSO strategy for movement.
/// </summary>
[CreateAssetMenu(menuName = "Combat/Attack Definition", fileName = "NewAttackDefinition")]
public class AttackDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable unique id used for cooldown tracking, save data, etc. Don't rename after data ships.")]
    public string attackId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Costs & Timing")]
    public float kiCost = 10f;
    public float cooldown = 1.5f;

    [Header("Projectile")]
    public GameObject projectilePrefab; // must have Collider(isTrigger)
    public ProjectileBehaviorSO behaviour; // Straight / Homing / Scatter strategy
    public float speed = 20f;
    public float damage = 10f;

    [Header("Multi-Projectile (scatter-style attacks)")]
    [Tooltip("How many projectile instances to spawn at once. 1 for a single shot; >1 for a fan/burst.")]
    [Min(1)] public int projectileCount = 1;

    [Tooltip("Delay in seconds between each projectile in a multi-shot attack. 0 = all simultaneous (e.g. scatter fan). >0 = sequenced burst (e.g. Twin Flash).")]
    [Min(0f)] public float intraShotDelay = 0f;

    [Header("Homing (only used if behaviour is a HomingBehaviorSO)")]
    [Tooltip("How targets are acquired for this attack. Ignored for non-homing behaviors.")]
    public TargetAcquisitionMode targetAcquisition = TargetAcquisitionMode.None;
    [Tooltip("Max distance/cone to search for a homing target, if applicable.")]
    public float targetSearchRadius = 15f;
    [Range(0f, 180f)] public float targetSearchConeAngle = 45f;

    [Header("Range / Validation")]
    [Tooltip("Max cast range; 0 = unlimited (e.g. self-targeted or unaimed).")]
    public float maxRange = 0f;
}

public enum TargetAcquisitionMode
{
    None,           // no target needed (straight/scatter attacks)
    NearestInCone,  // pick nearest enemy within targetSearchConeAngle of aim direction
    LockedReticle   // sends an explicit target id
}