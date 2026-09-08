using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Behavior/Homing")]
public class HomingBehaviorSO : ProjectileBehaviorSO
{
    [Tooltip("If the target is lost/dies, keep the last direction rather than snapping erratically.")]
    public bool loseTargetGracefully = true;

    public override void Move(Projectile proj, float stepDuration)
    {
        var target = proj.HomingTarget;

        if (target == null || (loseTargetGracefully))
            return; // keep current Direction — no target to steer toward

        SnapToNearestGridDirection(Vector2Int.zero); // TODO: This may be weird
    }

    /// <summary>Finds which of the 8 grid directions most closely points toward the delta.</summary>
    private Vector2Int SnapToNearestGridDirection(Vector2Int delta)
    {
        Vector2 normalized = new Vector2(delta.x, delta.y).normalized;
        Vector2Int best = GridUtility.EightDirections[0];
        float bestDot = -2f;

        foreach (var dir in GridUtility.EightDirections)
        {
            float dot = Vector2.Dot(normalized, new Vector2(dir.x, dir.y).normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = dir;
            }
        }

        return best;
    }
}