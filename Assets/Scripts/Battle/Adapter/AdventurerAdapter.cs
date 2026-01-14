using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdventurerAdapter : IBattleEntity
{
    private Adventurer _adv;
    
    public AdventurerAdapter(Adventurer adv)
    {
        _adv = adv;
    }

    public int InstanceID => _adv.InstanceID;
    public string Name => _adv.Name;
    public int AffiliationID => _adv.AffiliationID;
    public int Level => 1; // TODO 모험가 레벨 연동
    public PartyPosition Position => _adv.AssignedPosition;
    

    public float GetMaxHp()
    {
        float might = _adv.GetStat(StatType.Body_Might);
        float endurance = _adv.GetStat(StatType.Body_Endurance);
        float vitality = _adv.GetStat(StatType.Body_Vitality);
        float poise =  _adv.GetStat(StatType.Body_Poise);

        return (might * 15f) + (endurance * 20f) + (vitality * 100f) + (poise * 20f);
    }

    public float GetMaxDP()
    {
        float faith = _adv.GetStat(StatType.Tech_Faith);

        return faith * 100f;
    }

    public int GetMaxCost()
    {
        float composure = _adv.GetStat(StatType.Mind_Composure);
        float resolve = _adv.GetStat(StatType.Mind_Resolve);
        float focus = _adv.GetStat(StatType.Mind_Focus);

        return Mathf.FloorToInt((composure * 10f) + (resolve * 10f) + (focus * 10f));
    }

    public float GetMaxMP()
    {
        float composure = _adv.GetStat(StatType.Mind_Composure);
        float resolve = _adv.GetStat(StatType.Mind_Resolve);
        float focus = _adv.GetStat(StatType.Mind_Focus);
        float sorcery = _adv.GetStat(StatType.Tech_Sorcery);
        

        return (composure * 10f) + (resolve * 10f) + (focus * 10f) + (sorcery * 40f);
    }

    public float GetMaxSP()
    {
        float might = _adv.GetStat(StatType.Body_Might);
        float endurance = _adv.GetStat(StatType.Body_Endurance);
        float poise = _adv.GetStat(StatType.Body_Poise);
        float vitality = _adv.GetStat(StatType.Body_Vitality);
        
        return (might * 10f) + (endurance * 10f) + (poise * 10f) + (vitality * 20f);
    }

    public float GetDefenseScore()
    {
        return _adv.GetDefenseScore(ResistanceManager.Instance.GetDefenseWeightData());
    }

    public float GetRecoveryTimer()
    {
        throw new System.NotImplementedException();
    }

    public float GetThinkTimer()
    {
        throw new System.NotImplementedException();
    }

    public List<LearnedSkill> GetSkills()
    {
        List<LearnedSkill> skills = new List<LearnedSkill>();

        foreach (var skill in _adv.Skills)
        {
            skills.Add(skill);
        }

        return skills;
    }
    
    public Dictionary<StatType, float> GetStats()
    {
        Dictionary<StatType, float> stats = new Dictionary<StatType, float>();

        foreach (var v in _adv.Stats)
        {
            StatType statType = v.Key;
            float value = v.Value;
            stats.Add(statType, value);
        }

        return stats;
    }
    
    public Dictionary<NatureType, float> GetNatures()
    {
        Dictionary<NatureType, float> natures = new Dictionary<NatureType, float>();

        foreach (var v in _adv.Natures)
        {
            NatureType natureType = v.Key;
            float value = v.Value;
            natures.Add(natureType, value);
        }

        return natures;
    }
}