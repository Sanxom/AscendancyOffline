using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CharacterSkillState : MonoBehaviour
{
    [SerializeField] private SkillDefinitionSO[] allSkillsInGame;
    [SerializeField] private SkillDefinitionSO basicAttackSkill;

    // Server-only source of truth (fast lookups, no serialization overhead for the "do I know X" check).
    private readonly HashSet<string> knownSkillIds = new();

    // settings UI can read "what do I currently know".

    public event Action<SkillDefinitionSO> OnSkillLearned;

    private void Awake()
    {
        knownSkillIds.Add(basicAttackSkill.skillId);
    }

    public void CheckLevelUnlocks(int newLevel)
    {
        foreach (var skill in allSkillsInGame)
        {
            if (skill.unlockType != SkillUnlockType.LevelThreshold) continue;
            if (knownSkillIds.Contains(skill.skillId)) continue;
            if (newLevel < skill.requiredLevel) continue;

            LearnSkill(skill);
        }
    }

    public bool GrantSkill(string skillId)
    {
        var skill = Array.Find(allSkillsInGame, s => s.skillId == skillId);
        if (skill == null) return false;
        if (knownSkillIds.Contains(skillId)) return false;

        LearnSkill(skill);
        return true;
    }

    private void LearnSkill(SkillDefinitionSO skill)
    {
        knownSkillIds.Add(skill.skillId);
        OnSkillLearned?.Invoke(skill);
    }

    public bool KnowsSkill(string skillId) => knownSkillIds.Contains(skillId);

    /// <summary>Server-only. For save/load.</summary>
    public string[] GetKnownSkillIds()
    {
        var arr = new string[knownSkillIds.Count];
        knownSkillIds.CopyTo(arr);
        return arr;
    }

    public void LoadKnownSkills(string[] savedSkillIds)
    {
        knownSkillIds.Clear();

        if (basicAttackSkill != null)
        {
            knownSkillIds.Add(basicAttackSkill.skillId);
        }

        if (savedSkillIds != null)
        {
            foreach (var id in savedSkillIds)
            {
                knownSkillIds.Add(id);
            }
        }
    }
}