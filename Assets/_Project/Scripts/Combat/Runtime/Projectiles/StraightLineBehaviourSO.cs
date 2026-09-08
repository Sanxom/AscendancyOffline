using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Behavior/Straight Line")]
public class StraightLineBehaviorSO : ProjectileBehaviorSO
{
    public override void Move(Projectile proj, float stepDuration)
    {
        // Direction never changes for a straight shot — nothing to do here.
        // The projectile's own step loop just keeps applying the same Direction each step.
    }
}