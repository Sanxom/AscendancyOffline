using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slot-select screen. Subscribes directly to CharacterSlotManager's events.
/// Shows up to slotCountPerAccount slot buttons, populated once OnSlotsReceived fires.
/// </summary>
public class SlotSelectScreen : MonoBehaviour
{
    [SerializeField] private CharacterSlotManager slotManager;
    [SerializeField] private Transform slotButtonContainer;
    [SerializeField] private GameObject slotButtonPrefab; // must have SlotButtonView component

    private CharacterSaveData[] currentSlots;

    private void OnEnable()
    {
        slotManager.OnSlotsReceived += HandleSlotsReceived;
        slotManager.OnCharacterCreationNeeded += HandleCreationNeeded;
        slotManager.OnCharacterCreationRejected += HandleCreationRejected;
    }

    private void OnDisable()
    {
        slotManager.OnSlotsReceived -= HandleSlotsReceived;
        slotManager.OnCharacterCreationNeeded -= HandleCreationNeeded;
        slotManager.OnCharacterCreationRejected -= HandleCreationRejected;
    }

    private void HandleSlotsReceived(CharacterSaveData[] slots)
    {
        currentSlots = slots;
        RebuildSlotButtons();
    }

    private void RebuildSlotButtons()
    {
        foreach (Transform child in slotButtonContainer) Destroy(child.gameObject);

        for (int i = 0; i < currentSlots.Length; i++)
        {
            var saveData = currentSlots[i];
            var buttonGO = Instantiate(slotButtonPrefab, slotButtonContainer);
            var view = buttonGO.GetComponent<SlotButtonView>();
            view.Setup(saveData, OnSlotClicked);
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        slotManager.SelectSlot(slotIndex);
    }

    private void HandleCreationNeeded(int slotIndex)
    {
        // Hand off to the race-picker screen for this empty slot
        gameObject.SetActive(false);
        FindAnyObjectByType<RacePickerScreen>().Open(slotIndex);
    }

    private void HandleCreationRejected(string reason)
    {
        Debug.LogWarning($"Character creation rejected: {reason}");
        // TODO: surface this in an actual error dialog once UI art exists
    }
}