using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Minimal JSON-file-per-slot implementation.
/// </summary>
public class FileCharacterSaveRepository : ICharacterSaveRepository
{
    private readonly string savesRootPath;

    public FileCharacterSaveRepository(string rootPath)
    {
        savesRootPath = rootPath;
        Directory.CreateDirectory(savesRootPath);
    }

    public Task<CharacterSaveData[]> GetSlotsForAccount(int slotCount)
    {
        var slots = new CharacterSaveData[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            slots[i] = LoadSlotSync(i) ?? new CharacterSaveData
            {
                slotIndex = i,
                isEmpty = true
            };
        }
        return Task.FromResult(slots);
    }

    public Task<CharacterSaveData> GetSlot(int slotIndex)
    {
        var data = LoadSlotSync(slotIndex) ?? new CharacterSaveData
        {
            slotIndex = slotIndex,
            isEmpty = true
        };
        return Task.FromResult(data);
    }

    public Task SaveSlot(CharacterSaveData data)
    {
        data.lastPlayedUtc = DateTime.UtcNow;
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(GetSlotPath(data.slotIndex), json);
        return Task.CompletedTask;
    }

    public Task DeleteSlot(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private CharacterSaveData LoadSlotSync(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<CharacterSaveData>(File.ReadAllText(path));
    }

    private string GetSlotPath(int slotIndex) =>
        Path.Combine(savesRootPath, $"slot{slotIndex}.json");
}