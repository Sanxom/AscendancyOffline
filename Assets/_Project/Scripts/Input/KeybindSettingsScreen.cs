using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimal rebinding UI: shows the player's known skills (from CharacterSkillState) and lets
/// them assign each to a slot via a dropdown. A real implementation would likely support
/// drag-and-drop onto a hotbar UI instead — this is the functional baseline, not final UX.
/// </summary>
public class KeybindSettingsScreen : MonoBehaviour
{
    [SerializeField] private CharacterSkillState skillState; // reference to the local player's skill state
    [SerializeField] private Transform slotRowContainer;
    [SerializeField] private GameObject slotRowPrefab; // has a TMP_Text (slot label) + TMP_Dropdown (skill picker)

    private static readonly string[] SkillSlotIds =
    {
        "Slot1", "Slot2", "Slot3", "Slot4", "Slot5",
        "Slot6", "Slot7", "Slot8", "Slot9", "Slot10"
    };

    public void Open()
    {
        gameObject.SetActive(true);
        BuildRows();
    }

    private void BuildRows()
    {
        foreach (Transform child in slotRowContainer) Destroy(child.gameObject);

        var knownSkillIds = skillState.GetKnownSkillIds();

        foreach (var slotId in SkillSlotIds)
        {
            var rowGO = Instantiate(slotRowPrefab, slotRowContainer);
            var label = rowGO.transform.Find("SlotLabel").GetComponent<TextMeshProUGUI>();
            var dropdown = rowGO.transform.Find("SkillDropdown").GetComponent<TMP_Dropdown>();

            label.text = slotId;

            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>(knownSkillIds) { "(Unbound)" });

            string currentSkillId = KeybindManager.Instance.GetSkillIdForSlot(slotId);
            int currentIndex = currentSkillId != null
                ? System.Array.IndexOf(knownSkillIds, currentSkillId)
                : knownSkillIds.Length; // "(Unbound)" is the last option

            dropdown.value = currentIndex >= 0 ? currentIndex : knownSkillIds.Length;

            dropdown.onValueChanged.AddListener(selectedIndex =>
            {
                if (selectedIndex == knownSkillIds.Length)
                    KeybindManager.Instance.ClearBinding(slotId);
                else
                    KeybindManager.Instance.SetBinding(slotId, knownSkillIds[selectedIndex]);
            });
        }
    }
}