using System;
using UnityEngine;

[RequireComponent(typeof(CombatStats))]
public class RespawnController : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private Transform[] respawnPoints;

    private CombatStats stats;

    public event Action<float> OnDeathStarted;
    public event Action OnRespawned;

    private void Awake()
    {
        stats = GetComponent<CombatStats>();
    }

    private void Start()
    {
        stats.OnDied += HandleDied;
    }

    private void OnDestroy()
    {
        stats.OnDied -= HandleDied;
    }

    private void HandleDied(ulong attackerClientId)
    {
        SetCombatEnabled(false);
        Invoke(nameof(Respawn), respawnDelay);

        OnDeathStarted?.Invoke(respawnDelay);
    }

    private void Respawn()
    {
        Vector3 spawnPos = GetRespawnPoint();
        transform.position = spawnPos;

        stats.ResetForRespawn();
        SetCombatEnabled(true);

        OnRespawned?.Invoke();
    }

    private void SetCombatEnabled(bool enabled)
    {
        if (TryGetComponent<AttackController>(out var attackController))
            attackController.enabled = enabled;
    }

    private Vector3 GetRespawnPoint()
    {
        if (respawnPoints == null || respawnPoints.Length == 0) return Vector3.zero;
        return respawnPoints[UnityEngine.Random.Range(0, respawnPoints.Length)].position;
    }
}