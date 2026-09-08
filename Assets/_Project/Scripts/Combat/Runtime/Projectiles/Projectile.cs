using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private int maxTileSteps = 20;

    private SkillDefinitionSO skillDef;
    private float damage;
    private int stepsRemaining;
    private float tileSpeed; // world units/second, derived from skill.speed (tiles/second)

    private Vector2Int currentTile;

    private Vector2 position;
    private Vector2 legOrigin;
    private Vector2 legDestination;
    private bool isMoving;

    public Vector2Int Direction { get; private set; }
    public Vector2Int CurrentTile => GridUtility.WorldToTile(position);
    public GameObject HomingTarget { get; private set; }

    public void Initialize(
        SkillDefinitionSO skill, float damage, Vector2Int direction, Vector2Int startTile, GameObject homingTarget = null)
    {
        skillDef = skill;
        this.damage = damage;
        Direction = direction;
        HomingTarget = homingTarget;
        stepsRemaining = maxTileSteps;
        tileSpeed = (skill.speed > 0f ? skill.speed : 5f) * GridUtility.TILE_SIZE;

        position = GridUtility.TileToWorld(startTile);
        transform.position = position;

        currentTile = startTile;

        skillDef.behavior.Initialize(this);

        BeginNextLeg();
    }

    public void SetDirection(Vector2Int newDirection) => Direction = newDirection;

    private void Update()
    {
        UpdateMovement();
            //// Remote/client-side visual: smoothly chase the authoritative tile position.
            //Vector2 target = GridUtility.TileToWorld(currentTile);
            //position = Vector2.MoveTowards(position, target, tileSpeed * Time.deltaTime);
            //transform.position = position;
    }

    private void UpdateMovement()
    {
        if (!isMoving) return;

        position = Vector2.MoveTowards(position, legDestination, tileSpeed * Time.deltaTime);
        transform.position = position; // server's own view (host mode) also renders correctly from this

        if (position == legDestination)
        {
            isMoving = false;

            Vector2Int landedTile = GridUtility.WorldToTile(position);
            currentTile = landedTile; // replicate — triggers remote clients' interpolation target to update

            CheckTileForHit(landedTile);

            stepsRemaining--;
            if (stepsRemaining > 0)
            {
                BeginNextLeg();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void BeginNextLeg()
    {
        // Behaviour SO gets a chance to update Direction before each leg starts — same contract
        // as before (homing re-aims per leg, straight-line/scatter leave Direction unchanged).
        skillDef.behavior.Move(this, tileSpeed);

        Vector2Int fromTile = GridUtility.WorldToTile(position);
        Vector2Int toTile = fromTile + Direction;

        if (TileMapService.Instance != null && !TileMapService.Instance.IsWalkable(toTile))
        {
            Destroy(gameObject); // hit a wall — despawn rather than pass through
            return;
        }

        legOrigin = position;
        legDestination = GridUtility.TileToWorld(toTile);
        isMoving = true;
    }

    private void CheckTileForHit(Vector2Int tile)
    {
        //if (!IsServer) return;

        //var target = CombatantRegistry.FindAtTile(tile);
        //if (target != null && target.TryGetComponent<CombatStats>(out var targetStats))
        //{
        //    HitResolver.ResolveHit(targetStats, skillDef, damage);
        //    Destroy(gameObject);
        //}
    }
}