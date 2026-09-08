using UnityEngine;

/// <summary>
/// DEV-ONLY: Spawns a test character directly, CharacterSlotManager's save/load flow 
/// entirely. For testing movement/combat/skills in isolation. Disable devModeEnabled 
/// (or delete this component) to fall back to the real slot-select -> spawn flow.
/// </summary>
public class DevBootstrap : MonoBehaviour
{
    [SerializeField] private bool devModeEnabled = true;
    [SerializeField] private GameObject playerPrefab; // same prefab CharacterSlotManager uses
    [SerializeField] private RaceDefinitionSO devTestRace; // e.g. Terran, for a quick default

    private void Start()
    {
        if (!devModeEnabled) return;
        HandleGameStarted();
    }

    private void HandleGameStarted()
    {
        SpawnDevCharacter();
    }

    private void SpawnDevCharacter()
    {
        var go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        // Feed it a throwaway, non-persisted save data object — same LoadFromSaveData path
        // CharacterSlotManager uses for a real character, just with fabricated dev values
        // instead of anything read from disk/DB.
        var devSave = new CharacterSaveData
        {
            isEmpty = false,
            characterName = "DevTester",
            raceId = devTestRace != null ? devTestRace.raceId : "",
            currentTierIndex = 0,
            level = 1,
            currentXP = 0,
            trainedStrengthBonus = 0,
            trainedKiBonus = 0,
            trainedKiDefenseBonus = 0,
            trainedDefenseBonus = 0,
            unlockedWorldTriggerIds = System.Array.Empty<string>(),
            knownSkillIds = null, // CharacterSkillState grants Basic Attack automatically regardless
            posX = 0f,
            posY = 0f,
            lastSceneOrZoneId = "dev_test"
        };

        var progression = go.GetComponent<CharacterProgressionState>();
        progression.LoadFromSaveData(devSave);

        var skillState = go.GetComponent<CharacterSkillState>();
        KeybindManager.Instance.SetBinding("Slot2", "ki_blast");
        skillState.GrantSkill("ki_blast");
    }
}