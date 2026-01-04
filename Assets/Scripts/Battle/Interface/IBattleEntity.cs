using System.Collections.Generic;
public interface IBattleEntity
{
    string Name { get; }
    int Level { get; }
    PartyPosition Position { get; }
    
    float GetMaxHp();
    float GetMaxMP();
    float GetMaxSP();
    float GetMaxDP();
    int GetMaxCost();
    float GetDefenseScore();
    List<LearnedSkill> GetSkills();
    Dictionary<StatType, float> GetStats();
    Dictionary<NatureType, float> GetNatures();


}