using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CombatStats))]
public class PlayerMovementController : MonoBehaviour
{
    private CombatStats stats;

    private Vector2Int currentTile;
    private int facingDirectionIndex;

    private Vector2 position;
    private Vector2 moveOrigin;
    private Vector2 moveDestination;
    private bool isMoving;

    private Vector2Int heldDirection;

    public Vector2Int CurrentTile => GridUtility.WorldToTile(position);
    public Vector2Int FacingDirection => GridUtility.EightDirections[facingDirectionIndex];

    public event Action<Vector2Int, Vector2Int> OnTileChanged;
    public event Action<Vector2Int> OnPredictionCorrected;

    private void Awake()
    {
        stats = GetComponent<CombatStats>();

        position = GridUtility.TileToWorld(currentTile);
        transform.position = position;
    }

    public void SetHeldDirection(Vector2 rawInput)
    {
        heldDirection = GridUtility.SnapToEightDirection(rawInput);
    }

    private void Update()
    {
        // Smoothly chase the tile center. This is intentionally simple interpolation
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float moveSpeed = (stats.Speed > 0f ? stats.Speed : 5f) * GridUtility.TILE_SIZE;

        if (!isMoving)
        {
            if (heldDirection != Vector2Int.zero)
            {
                Vector2Int startTile = GridUtility.WorldToTile(position);
                Vector2Int targetTile = startTile + heldDirection;

                bool walkable = TileMapService.Instance == null || TileMapService.Instance.IsWalkable(targetTile);

                if (walkable)
                {
                    moveOrigin = GridUtility.TileToWorld(startTile);
                    moveDestination = GridUtility.TileToWorld(targetTile);
                    isMoving = true;

                    facingDirectionIndex = Array.IndexOf(GridUtility.EightDirections, heldDirection);
                }
            }
            return;
        }

        position = Vector2.MoveTowards(position, moveDestination, moveSpeed * Time.deltaTime);
        transform.position = position;

        if (position == moveDestination)
        {
            isMoving = false;

            Vector2Int landedTile = GridUtility.WorldToTile(position);
            OnTileChanged?.Invoke(GridUtility.WorldToTile(moveOrigin), landedTile);

            if (heldDirection != Vector2Int.zero)
            {
                UpdateMovement();
            }
        }
    }
}