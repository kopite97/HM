using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Monster
{
    public int InstanceID { get; private set; }
    public int MonsterID { get; private set; }
    public string Name { get; private set; }
    public int Level { get; private set; }
    public float MaxHP { get; private set; }
    public float DefenseScore { get;private set; }
    public int AffiliationID { get; private set; }
    private string Desc;
    
    public PartyPosition StartPosition { get; private set; } // 배치된 위치

    private List<LearnedSkill> _skills = new List<LearnedSkill>();
    public IReadOnlyList<LearnedSkill> Skills => _skills;
    

    private Dictionary<StatType, float> _stats = new Dictionary<StatType, float>();
    private Dictionary<NatureType, float> _natures = new Dictionary<NatureType, float>();
    public IReadOnlyDictionary<StatType, float> Stats => _stats;
    public IReadOnlyDictionary<NatureType, float> Natures => _natures;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="data">원본 몬스터 데이터</param>
    /// <param name="position">이 몬스터가 배치될 위치</param>
    public Monster(MonsterData data)
    {
        MonsterID = data.ID;
        Name = data.NameKR;
        Level = data.Level;
        MaxHP = data.MaxHP;
  
        DefenseScore = data.DefenseScore;
        Desc = data.Desc;

        var skillDict = SkillManager.Instance.GetSkillDictionary();

        foreach (var skillId in data.Skill_List)
        {
            SkillSO skillData = skillDict[skillId];
            LearnedSkill skill = new LearnedSkill(skillData);
            _skills.Add(skill);
        }

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            if (type == StatType.None) continue;
            
            string fieldName = type.ToString();
            FieldInfo field = typeof(MonsterData).GetField(fieldName);

            if (field != null)
            {
                float value = (float)field.GetValue(data);
                _stats.Add(type, value);
            }
            else
            {
                Debug.LogWarning($"[Monster Init] MonsterData에 '{fieldName}' 필드가 없습니다.");
            }
        }
        
        foreach(NatureType type in Enum.GetValues(typeof(NatureType)))
        {
            if(type== NatureType.None) continue;
            
            string fieldName = type.ToString();
            FieldInfo field = typeof(MonsterData).GetField(fieldName);

            if (field != null)
            {
                float value = (float)field.GetValue(data);
                _natures.Add(type, value);
            }
            else
            {
                Debug.LogWarning($"[Monster Init] MonsterData에 '{fieldName}' 필드가 없습니다.");
            }
            
        }
    }

    /// <summary>
    /// 특정 스탯 값을 가져옵니다.
    /// </summary>
    public float GetStat(StatType type)
    {
        return Stats.TryGetValue(type,out float val) ?  val : 0;
    }

    /// <summary>
    /// 성격 값을 가져옵니다. (AI 로직용)
    /// </summary>
    public float GetNature(NatureType type)
    {
        return Natures.TryGetValue(type, out float val) ? val : 0;
    }
}