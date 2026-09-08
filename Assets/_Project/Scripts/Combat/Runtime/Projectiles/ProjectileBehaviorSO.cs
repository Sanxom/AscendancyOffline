using UnityEngine;

/// <summary>
/// Strategy for how a projectile's DIRECTION evolves each tile step. Move() no longer
/// mutates position directly (that's now the projectile's own tile-stepping loop) —
/// it just decides/updates proj.Direction before the next step is taken.
/// </summary>
public abstract class ProjectileBehaviorSO : ScriptableObject
{
    /// <summary>Called once on spawn, server-only, for any per-instance setup (e.g. picking a scatter angle).</summary>
    public virtual void Initialize(Projectile proj) { }

    /// <summary>
    /// Called before each tile step. Set proj.Direction (via proj.SetDirection) to whatever
    /// direction the NEXT step should move in. stepDuration is provided for behaviours that
    /// need timing context (currently unused by the three built-in behaviours, but kept for
    /// future use — e.g. a behaviour that changes direction only every N steps).
    /// </summary>
    public abstract void Move(Projectile proj, float stepDuration);
}