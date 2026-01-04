using System.Collections.Generic;
using UnityEngine;

public class MonsterAdapter : IBattleEntity
{
    private Monster _mon;

    public MonsterAdapter(Monster mon)
    {
        _mon = mon;
        
    }

    public string Name => _mon.Name;
    public int Level => _mon.Level;
    public PartyPosition Position => _mon.StartPosition;
    
    /**
     *
     * 몬스터들은 별다른 과정 없이 스탯 그대로 (일단은)
     */
    
    
    public float GetMaxHp()
    {
        return _mon.MaxHP;
    }

    public float GetMaxMP()
    {
        float composure = _mon.GetStat(StatType.Mind_Composure);
        float resolve = _mon.GetStat(StatType.Mind_Resolve);
        float focus = _mon.GetStat(StatType.Mind_Focus);
        float sorcery = _mon.GetStat(StatType.Tech_Sorcery);
        

        return (composure * 10f) + (resolve * 10f) + (focus * 10f) + (sorcery * 40f);
    }

    public float GetMaxSP()
    {
        float might = _mon.GetStat(StatType.Body_Might);
        float endurance = _mon.GetStat(StatType.Body_Endurance);
        float poise = _mon.GetStat(StatType.Body_Poise);
        float vitality = _mon.GetStat(StatType.Body_Vitality);
        
        return (might * 10f) + (endurance * 10f) + (poise * 10f) + (vitality * 20f);
    }

    public float GetMaxDP()
    {
        float faith = _mon.GetStat(StatType.Tech_Faith);

        return faith * 100f;
    }

    public int GetMaxCost()
    {
        float composure = _mon.GetStat(StatType.Mind_Composure);
        float resolve = _mon.GetStat(StatType.Mind_Resolve);
        float focus = _mon.GetStat(StatType.Mind_Focus);

        return Mathf.FloorToInt((composure * 10f) + (resolve * 10f) + (focus * 10f));
    }

    public float GetBaseStat(StatType type)
    {
        return _mon.Stats.TryGetValue(type, out float val) ? val : 0;
    }

    public float GetNatureStat(NatureType type)
    {
        return _mon.GetNature(type);
    }

    public float GetDefenseScore()
    {
        return _mon.DefenseScore;
    }

    public List<LearnedSkill> GetSkills()
    {
        List<LearnedSkill> skills = new List<LearnedSkill>();

        foreach (var skill in _mon.Skills)
        {
            LearnedSkill newSKill = new LearnedSkill(skill);
            skills.Add(newSKill);
        }
        return skills;
    }

    public Dictionary<StatType, float> GetStats()
    {
        Dictionary<StatType, float> stats = new Dictionary<StatType, float>();

        foreach (var v in _mon.Stats)
        {
            StatType type = v.Key;
            float value =  v.Value;
            stats.Add(type, value);
        }

        return stats;
    }

    public Dictionary<NatureType, float> GetNatures()
    {
        Dictionary<NatureType, float> natures = new Dictionary<NatureType, float>();

        foreach (var v in _mon.Natures)
        {
            NatureType type = v.Key;
            float value = v.Value;
            natures.Add(type, value);
        }

        return natures;
    }
}