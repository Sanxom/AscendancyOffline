using UnityEngine;
using TMPro;

public class RacePickerScreen : MonoBehaviour
{
    [SerializeField] private CharacterSlotManager slotManager;
    [SerializeField] private RaceDefinitionSO[] availableRaces;
    [SerializeField] private Transform raceCardContainer;
    [SerializeField] private GameObject raceCardPrefab;
    [SerializeField] private TMP_InputField characterNameInput;

    private int targetSlotIndex;
    private RaceDefinitionSO selectedRace;

    public void Open(int slotIndex)
    {
        targetSlotIndex = slotIndex;
        selectedRace = null;
        gameObject.SetActive(true);
        BuildRaceCards();
    }

    private void BuildRaceCards()
    {
        foreach (Transform child in raceCardContainer) Destroy(child.gameObject);

        foreach (var race in availableRaces)
        {
            var cardGO = Instantiate(raceCardPrefab, raceCardContainer);
            var view = cardGO.GetComponent<RaceCardView>();
            view.Setup(race, () => selectedRace = race);
        }
    }

    public void OnConfirmClicked()
    {
        if (selectedRace == null) return;
        if (string.IsNullOrWhiteSpace(characterNameInput.text)) return;

        slotManager.CreateCharacter(targetSlotIndex, characterNameInput.text, selectedRace.raceId);
    }
}