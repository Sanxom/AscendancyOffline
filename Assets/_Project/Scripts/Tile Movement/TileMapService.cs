using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Walkability lookup for the world grid. Placeholder implementation —
/// currently treats every tile as walkable except ones explicitly marked blocked. A real
/// implementation would likely read from Unity's Tilemap component or a level-data asset;
/// this exists so movement validation has something concrete to check against right now.
/// </summary>
public class TileMapService : MonoBehaviour
{
    public static TileMapService Instance { get; private set; }

    [SerializeField] private List<Vector2Int> blockedTiles = new(); // manually authored for now, in absence of a real tilemap/level system

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsWalkable(Vector2Int tile) => !blockedTiles.Contains(tile);
}