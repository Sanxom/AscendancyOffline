using System;

[Serializable]
public class CharacterSaveData
{
    public bool isEmpty = true;
    public string characterName;
    public int slotIndex;

    public string raceId;
    public int currentTierIndex;
    public int level;
    public long currentXP;

    // direct-training bonuses, persisted so RecalculateStats reconstructs identically on load
    public float trainedStrengthBonus;
    public float trainedKiBonus;
    public float trainedKiDefenseBonus;
    public float trainedDefenseBonus;

    public string[] unlockedWorldTriggerIds;
    public float posX, posY;
    public string lastSceneOrZoneId;
    public DateTime lastPlayedUtc;

    public string[] knownSkillIds;
}