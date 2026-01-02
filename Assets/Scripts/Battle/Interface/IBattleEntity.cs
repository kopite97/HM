using System.Collections.Generic;
public interface IBattleEntity
{
    string Name { get; }
    int Level { get; }
    PartyPosition Position { get; }
    
    float GetMaxHp();
    float GetBaseStat(StatType type);
    float GetDefenseScore();
    List<int> GetSkillIDs();
}