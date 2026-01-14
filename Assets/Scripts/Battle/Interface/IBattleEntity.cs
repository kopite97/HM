using System.Collections.Generic;
public interface IBattleEntity
{
    int InstanceID { get; }
    string Name { get; }
    int AffiliationID { get; }
    int Level { get; }
    PartyPosition Position { get; }
    
    float GetMaxHp();
    float GetMaxMP();
    float GetMaxSP();
    float GetMaxDP();
    float GetDefenseScore();
    float GetRecoveryTimer();
    float GetThinkTimer();
    List<LearnedSkill> GetSkills();
    Dictionary<StatType, float> GetStats();
    Dictionary<NatureType, float> GetNatures();
}