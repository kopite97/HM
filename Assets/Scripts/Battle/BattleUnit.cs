using UnityEngine;
using System.Collections.Generic;

public class BattleUnit
{
    private IBattleEntity _sourceEntity;
    
    // 전투 중 상태값
    public float CurrentHP { get; private set; }
    public float MaxHP { get; private set; }
    public PartyPosition CurrentPosition { get; private set; }

    // 버프/디버프로 변동된 스탯 관리용 
    private Dictionary<StatType,float> _statModifiers = new Dictionary<StatType, float>();

    public BattleUnit(IBattleEntity entity)
    {
        _sourceEntity = entity;

        MaxHP = _sourceEntity.GetMaxHp();
        CurrentHP = MaxHP;
        CurrentPosition = _sourceEntity.Position;
    }
    
    public string Name => _sourceEntity.Name;

    public float GetStat(StatType type)
    {
        float baseVal = _sourceEntity.GetBaseStat(type);
        float modVal = _statModifiers.TryGetValue(type, out float val) ? val : 0;
        
        return Mathf.Max(0,baseVal +  modVal);
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

}