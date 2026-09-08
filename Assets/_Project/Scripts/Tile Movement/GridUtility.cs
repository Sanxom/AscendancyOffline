using UnityEngine;

/// <summary>
/// Static utility for converting between world position and tile coordinates, and defining
/// the 8 valid movement directions. TILE_SIZE is the authoritative "how big is one tile in
/// world units" constant — change this in one place if your tile art doesn't match 1 unit/tile.
/// </summary>
public static class GridUtility
{
    public const float TILE_SIZE = 1f;

    public static Vector2Int WorldToTile(Vector2 worldPos) =>
        new(Mathf.RoundToInt(worldPos.x / TILE_SIZE), Mathf.RoundToInt(worldPos.y / TILE_SIZE));

    public static Vector2 TileToWorld(Vector2Int tile) =>
        new(tile.x * TILE_SIZE, tile.y * TILE_SIZE);

    /// <summary>The 8 valid step directions, in a fixed order used for facing-direction indexing.</summary>
    public static readonly Vector2Int[] EightDirections =
    {
        new(0, 1),   // N
        new(1, 1),   // NE
        new(1, 0),   // E
        new(1, -1),  // SE
        new(0, -1),  // S
        new(-1, -1), // SW
        new(-1, 0),  // W
        new(-1, 1),  // NW
    };

    /// <summary>Snaps an arbitrary Vector2 input (e.g. raw WASD combination) to the nearest of the 8 grid directions.</summary>
    public static Vector2Int SnapToEightDirection(Vector2 rawInput)
    {
        if (rawInput.sqrMagnitude < 0.01f) return Vector2Int.zero;

        int x = rawInput.x > 0.3f ? 1 : (rawInput.x < -0.3f ? -1 : 0);
        int y = rawInput.y > 0.3f ? 1 : (rawInput.y < -0.3f ? -1 : 0);
        return new Vector2Int(x, y);
    }
}