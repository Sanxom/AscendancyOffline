using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI raceAndLevelLabel;
    [SerializeField] private Button button;

    public void Setup(CharacterSaveData saveData, Action<int> onClicked)
    {
        if (saveData.isEmpty)
        {
            nameLabel.text = "Empty Slot";
            raceAndLevelLabel.text = "";
        }
        else
        {
            nameLabel.text = saveData.characterName;
            raceAndLevelLabel.text = $"{saveData.raceId} — Lv. {saveData.level}";
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked(saveData.slotIndex));
    }
}