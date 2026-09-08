/// <summary>
/// Static utility for resolving a confirmed projectile hit into
/// actual damage. Deliberately not a singleton/service — just a stateless
/// static call, since it has no persistent state of its own.
/// </summary>
public static class HitResolver
{
    public static void ResolveHit(CombatStats target, SkillDefinitionSO skill, float damage, ulong attackerId)
    {
        if (target == null || skill == null) return;
        target.ApplyDamage(damage, attackerId);

        // Room to grow here later without touching callers:
        // - status effects (e.g. stun/knockback) from AttackDefinitionSO
        // - damage falloff by distance
        // - elemental/type resistance multipliers
        // - combat log / kill-feed event dispatch
    }
}