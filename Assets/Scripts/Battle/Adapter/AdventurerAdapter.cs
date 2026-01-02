using System.Collections.Generic;
using System.Linq;

public class AdventurerAdapter : IBattleEntity
{
    private Adventurer _adv;
    
    public AdventurerAdapter(Adventurer adv)
    {
        _adv = adv;
    }
    
    public string Name => _adv.Name;
    public int Level => 1; // TODO 모험가 레벨 연동
    public PartyPosition Position => _adv.AssignedPosition;
    

    public float GetMaxHp()
    {
        float might = _adv.GetStat(StatType.Might);
        float endurance = _adv.GetStat(StatType.Endurance);
        float vitality = _adv.GetStat(StatType.Vitality);
        float poise =  _adv.GetStat(StatType.Poise);

        return (might * 15f) + (endurance * 20f) + (vitality * 100f) + (poise * 20f);
    }

    public float GetBaseStat(StatType type)
    {
        return  _adv.GetStat(type);
    }

    public float GetDefenseScore()
    {
        return _adv.GetDefenseScore(DataManager.Instance.DefenseWeight);
    }

    public List<int> GetSkillIDs()
    {
        return _adv.Skills.Select(s => s.Data.ID).ToList();
    }
}