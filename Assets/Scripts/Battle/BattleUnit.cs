using System;
using UnityEngine;
using System.Collections.Generic;

public class BattleUnit
{
    private IBattleEntity _sourceEntity;
    
    // 턴제 변수
    public float ActionGauge { get; private set; } = 0f; // 0 ~ 100
    public float Speed { get; private set; }

    // 전투 중 상태값
    public float CurrentHP { get; private set; }
    public float CurrentMP { get; private set; }
    public float CurrentDP { get; private set; }
    public float CurrentSP { get; private set; }
    
    public float MaxHP { get; private set; }
    public float MaxMP { get; private set; }
    public float MaxDP { get; private set; }
    public float MaxSP { get; private set; }
    
    public int CurrentCost { get; private set; }
    public int MaxCost { get; private set; }

    public PartyPosition CurrentPosition { get; private set; }
    
    public bool IsDead { get; private set; } = false;
    public int Level { get; private set; } = 1;
    public bool IsActing { get; private set; } = false;

    public string Name { get; private set; }

    private Dictionary<StatType,float> _baseStats = new Dictionary<StatType, float>();
    private Dictionary<NatureType,float> _natures = new Dictionary<NatureType, float>();
    private List<LearnedSkill> _skills = new List<LearnedSkill>();
    // 버프/디버프로 변동된 스탯 관리용 
    private Dictionary<StatType,float> _statModifiers = new Dictionary<StatType, float>();

    public BattleUnit(IBattleEntity entity)
    {
        _sourceEntity = entity;
    
        // 기본 정보 초기화
        CurrentPosition = _sourceEntity.Position;
        Level = _sourceEntity.Level;
        Name = _sourceEntity.Name;
        _baseStats = _sourceEntity.GetStats();
        _natures = _sourceEntity.GetNatures();
        _skills = _sourceEntity.GetSkills();

        // [성장 공식 적용]
        // HP: 레벨당 10% 증가
        MaxHP = CalculateGrowth(_sourceEntity.GetMaxHp(), 0.10f); 
    
        // MP/SP: 레벨당 5% 증가 
        MaxMP = CalculateGrowth(_sourceEntity.GetMaxMP(), 0.05f); 
        MaxSP = CalculateGrowth(_sourceEntity.GetMaxSP(), 0.05f); 
    
  
        MaxDP = CalculateGrowth(_sourceEntity.GetMaxDP(), 0.0f); 
        MaxCost = Mathf.FloorToInt(CalculateGrowth(_sourceEntity.GetMaxCost(), 0.0f)); 

        // 현재 수치 채우기
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
        CurrentDP = MaxDP;
        CurrentSP = MaxSP;
        CurrentCost = MaxCost;

    }

    public void Tick(float deltaTime)
    {
        if (IsDead) return;

        float chargeAmount = Speed * deltaTime * 0.5f;
        
        ActionGauge += chargeAmount;
    }
    
    public void OnTurnStart(float cost)
    {
        IsActing = true;
        ActionGauge -= cost;
    }
    
    public void OnTurnEnd()
    {
        IsActing = false;
    }

    public void ModifiedActionGuage(float amount)
    {
        ActionGauge += amount;
    }

    public float GetStat(StatType type)
    {
        float baseVal = _baseStats.TryGetValue(type, out float bV) ? bV : 0;
        float modVal = _statModifiers.TryGetValue(type, out float mV) ? mV : 0;
        
        return Mathf.Max(0,baseVal +  modVal);
    }

    public float GetNatureStat(NatureType type)
    {
        // 성격은 전투 중에는 변하지 않음
        return _natures.TryGetValue(type, out float nV) ? nV : 0;
    }

    public List<LearnedSkill> GetSkills()
    {
        return _skills;
    }

    public float GetTotalDefense()
    {
        return _sourceEntity.GetDefenseScore();
    }

    public PartyPosition UpdatePartyPosition(PartyPosition newPosition)
    {
        CurrentPosition = newPosition;
        return CurrentPosition;
    }
    
    public void TakeDamage(float damage)
    {
        CurrentHP -=  damage;
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            IsDead = true;
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        CurrentHP += amount;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;
    }

    /// <summary>
    ///  공격 속성에 따른 저항력 계산
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetResistance(DamageType type)
    {
        // 고정 피해나 힐은 저항 불가
        if (type.HasFlag(DamageType.True) || type == DamageType.None) return 0f;

        float totalResistance = 0f;
        int typeCount = 0;

        foreach (DamageType checkType in Enum.GetValues(typeof(DamageType)))
        {
            // None이나 복합 플래그는 건너뛰기
            if (checkType == DamageType.None) continue;
            
            // 해당 속성이 공격에 포함되어 있는지 확인
            if (IsSingleBit(checkType) && type.HasFlag(checkType))
            {
                totalResistance += ResistanceManager.Instance.CalculateSingleResistance(this, checkType);
                typeCount++;
            }
        }
        
        return typeCount > 0 ? totalResistance / typeCount : 0f;
    }

    private bool IsSingleBit(DamageType type)
    {
        int val = (int)type;
        return val != 0 && (val & (val - 1)) == 0;
    }

    public bool CanAct()
    {
        return true;
    }

    public bool HasEnoughCost(SkillData data)
    {
        SkillResourceType resourceType = data.Resource_Type;
        int skillResource = data.Resource;
        int cost = data.Cost_Value;
        
        if (CurrentCost < cost) return false;

        if (resourceType == SkillResourceType.HP)
        {
            if(skillResource > CurrentHP)  return false;
        }
        else if (resourceType == SkillResourceType.SP)
        {
            if (skillResource > CurrentSP) return false;
        }
        else if (resourceType == SkillResourceType.MP)
        {
            if (skillResource > CurrentMP) return false;
        }
        else if (resourceType == SkillResourceType.DP)
        {
            if (skillResource > CurrentDP) return false;
        }
        
            return true;
    }
    
    private float CalculateGrowth(float baseValue, float growthRate)
    {
        if (Level <= 1) return baseValue;

        // 공식: 기본값 * (1 + (레벨-1) * 계수)
        float multiplier = 1.0f + ((Level - 1) * growthRate);
    
        // 소수점 버림 or 반올림 (깔끔한 UI를 위해 int형 변환 추천)
        return Mathf.Floor(baseValue * multiplier);
    }
}