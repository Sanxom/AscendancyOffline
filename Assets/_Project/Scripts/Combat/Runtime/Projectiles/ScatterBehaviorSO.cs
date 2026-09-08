using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Behavior/Scatter")]
public class ScatterBehaviorSO : ProjectileBehaviorSO
{
    [Tooltip("Max random deviation from the aim direction, in grid-direction steps (0 = none, 1 = one direction over on the 8-way compass, 2 = two over, etc.)")]
    [Range(0, 3)] public int maxScatterSteps = 1;

    public override void Initialize(Projectile proj)
    {
        int baseIndex = System.Array.IndexOf(GridUtility.EightDirections, proj.Direction);
        if (baseIndex < 0) baseIndex = 0;

        int offset = Random.Range(-maxScatterSteps, maxScatterSteps + 1);
        int scatteredIndex = ((baseIndex + offset) % 8 + 8) % 8; // wrap correctly for negative offsets

        proj.SetDirection(GridUtility.EightDirections[scatteredIndex]);
    }

    public override void Move(Projectile proj, float stepDuration)
    {
        // Direction was set once at spawn via Initialize; stays constant afterward.
    }
}