using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keybind preferences: which physical key/button triggers which
/// learned skillId. Persisted via PlayerPrefs.
/// </summary>
public class KeybindManager : MonoBehaviour
{
    public static KeybindManager Instance { get; private set; }

    // Maps a keybind slot (an arbitrary string identifying a bindable input, e.g. "Slot1".."Slot10",
    // or a raw key name) to a learned skillId. Empty/missing entry = unbound.
    private readonly Dictionary<string, string> slotToSkillId = new();

    public event Action<string, string> OnBindingChanged; // (slotId, newSkillId)

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadBindings(IEnumerable<string> availableSlotIds)
    {
        slotToSkillId.Clear();
        foreach (var slotId in availableSlotIds)
        {
            string skillId = PlayerPrefs.GetString(slotId, string.Empty);
            if (!string.IsNullOrEmpty(skillId))
            {
                slotToSkillId[slotId] = skillId;
            }
        }
    }

    /// <summary>Assigns skillId to a slot, overwriting any previous binding on that slot. Persists immediately.</summary>
    public void SetBinding(string slotId, string skillId)
    {
        slotToSkillId[slotId] = skillId;
        PlayerPrefs.SetString(slotId, skillId);
        PlayerPrefs.Save();
        OnBindingChanged?.Invoke(slotId, skillId);
    }

    public void ClearBinding(string slotId)
    {
        slotToSkillId.Remove(slotId);
        PlayerPrefs.DeleteKey(slotId);
        OnBindingChanged?.Invoke(slotId, null);
    }

    /// <summary>Returns the skillId bound to a slot, or null if unbound.</summary>
    public string GetSkillIdForSlot(string slotId) =>
        slotToSkillId.TryGetValue(slotId, out var skillId) ? skillId : null;

    /// <summary>Reverse lookup — used by settings UI to show "this skill is currently on slot X".</summary>
    public string GetSlotForSkillId(string skillId)
    {
        foreach (var kvp in slotToSkillId)
            if (kvp.Value == skillId) return kvp.Key;
        return null;
    }
}