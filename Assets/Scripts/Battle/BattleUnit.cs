using System;
using UnityEngine;
using System.Collections.Generic;

public class BattleUnit
{
    private IBattleEntity _sourceEntity;
    
    // 전투 중 상태값
    public float CurrentHP { get; private set; }
    public float MaxHP { get; private set; }
    public PartyPosition CurrentPosition { get; private set; }
    public bool IsDead { get; private set; } = false;
    public int Level { get; private set; } = 1;

    // 버프/디버프로 변동된 스탯 관리용 
    private Dictionary<StatType,float> _statModifiers = new Dictionary<StatType, float>();

    public BattleUnit(IBattleEntity entity)
    {
        _sourceEntity = entity;

        MaxHP = _sourceEntity.GetMaxHp();
        CurrentHP = MaxHP;
        CurrentPosition = _sourceEntity.Position;
        Level = _sourceEntity.Level;
    }
    
    public string Name => _sourceEntity.Name;

    public float GetStat(StatType type)
    {
        float baseVal = _sourceEntity.GetBaseStat(type);
        float modVal = _statModifiers.TryGetValue(type, out float val) ? val : 0;
        
        return Mathf.Max(0,baseVal +  modVal);
    }

    public float GetNatureStat(NatureType type)
    {
        // 성격은 전투 중에는 변하지 않음
        return _sourceEntity.GetNatureStat(type);
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

    public List<int> GetSkillIDs()
    {
        return _sourceEntity.GetSkillIDs();
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

}