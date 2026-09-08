using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterSlotManager : MonoBehaviour
{
    [SerializeField] private int slotCountPerAccount = 3;
    [SerializeField] private GameObject playerPrefab;

    private ICharacterSaveRepository saveRepository;

    private void Start()
    {
        saveRepository = new FileCharacterSaveRepository(Application.persistentDataPath + "/CharacterSaves");
    }

    public event Action<CharacterSaveData[]> OnSlotsReceived;
    public event Action<int> OnCharacterCreationNeeded;
    public event Action<string> OnCharacterCreationRejected;

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCountPerAccount) return;

        _ = HandleSelectSlotAsync(slotIndex);
    }

    private async Task HandleSelectSlotAsync(int slotIndex)
    {
        try
        {
            var saveData = await saveRepository.GetSlot(slotIndex);

            if (saveData.isEmpty)
            {
                PromptCharacterCreation(slotIndex);
                return;
            }

            SpawnCharacterFromSave(saveData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"HandleSelectSlotAsync failed for slot {slotIndex}: {ex}");
        }
    }

    public void CreateCharacter(
        int slotIndex, string characterName, string raceId)
    {
        if (slotIndex < 0 || slotIndex >= slotCountPerAccount) return;

        _ = HandleCreateCharacterAsync(slotIndex, characterName, raceId);
    }

    private async Task HandleCreateCharacterAsync(
        int slotIndex, string characterName, string raceId)
    {
        try
        {
            var existing = await saveRepository.GetSlot(slotIndex);
            if (!existing.isEmpty)
            {
                RejectCharacterCreation("Slot already occupied.");
                return;
            }

            var newSave = new CharacterSaveData
            {
                slotIndex = slotIndex,
                isEmpty = false,
                characterName = characterName,
                raceId = raceId,
                currentTierIndex = 0,
                level = 1,
                currentXP = 0,
                unlockedWorldTriggerIds = Array.Empty<string>(),
                posX = 0f,
                posY = 0f,
                lastSceneOrZoneId = "starting_zone"
            };

            await saveRepository.SaveSlot(newSave);
            SpawnCharacterFromSave(newSave);
        }
        catch (Exception ex)
        {
            Debug.LogError($"HandleCreateCharacterAsync failed for slot {slotIndex}: {ex}");
        }
    }

    private void SpawnCharacterFromSave(CharacterSaveData saveData)
    {
        var go = Instantiate(playerPrefab, new Vector3(saveData.posX, saveData.posY, 0f), Quaternion.identity);

        var progression = go.GetComponent<CharacterProgressionState>();
        progression.LoadFromSaveData(saveData);
    }

    private void PromptCharacterCreation(int slotIndex)
    {
        OnCharacterCreationNeeded?.Invoke(slotIndex);
    }

    private void RejectCharacterCreation(string reason)
    {
        OnCharacterCreationRejected?.Invoke(reason);
    }

    [Serializable]
    private class SlotArrayWrapper { public CharacterSaveData[] slots; }
}