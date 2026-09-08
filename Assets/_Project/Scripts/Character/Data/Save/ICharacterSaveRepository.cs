using System.Threading.Tasks;

/// <summary>
/// Abstraction over wherever character save data actually lives
/// Swap the implementation without touching any of the slot-selection flow below.
/// </summary>
public interface ICharacterSaveRepository
{
    Task<CharacterSaveData[]> GetSlotsForAccount(int slotCount);
    Task<CharacterSaveData> GetSlot(int slotIndex);
    Task SaveSlot(CharacterSaveData data);
    Task DeleteSlot(int slotIndex);
}