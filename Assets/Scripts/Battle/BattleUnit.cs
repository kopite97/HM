using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class BattleUnit
{
    private IBattleEntity _sourceEntity;
    
    // =======================================
    // [이벤트] 외부 구도 가능하게
    // =======================================
    public event Action<BattleUnit> OnTurnStartEvent;
    public event Action<BattleUnit> OnTurnEndEvent;
    public event Action<DamageContext> OnBeforeTakeDamage;
    public event Action<DamageContext> OnTakeDamage;
    public event Action<BattleUnit> OnDeath;
    public event Action<float> OnHPChanged;
    
    // =======================================
    // 스탯 및 속성
    // =======================================
    public float Speed => GetStat(StatType.Body_Mobility);
    
    // 턴제 변수
    public float ActionGauge { get; private set; } = 0f; // 0 ~ 100
    public const float MAX_GAUGE = 100f;
    
    // 전투 중 상태값
    public float CurrentHP { get; private set; }
    public float CurrentMP { get; private set; }
    public float CurrentDP { get; private set; }
    public float CurrentSP { get; private set; }
    
    // Max 값
    public float MaxHP { get; private set; }
    public float MaxMP { get; private set; }
    public float MaxDP { get; private set; }
    public float MaxSP { get; private set; }

    // 기본 정보
    public PartyPosition CurrentPosition { get; private set; }
    public int Level { get; private set; } = 1;
    public string Name { get; private set; }
    public bool IsDead { get; private set; } = false;
    
    // 스탯 컨테이너
    private Dictionary<StatType, float> _baseStats;
    private Dictionary<NatureType, float> _natures;
    private List<LearnedSkill> _skills;
    
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

        // 현재 수치 채우기
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
        CurrentDP = MaxDP;
        CurrentSP = MaxSP;

    }
    
    // ====================================
    // 행동 로직
    // ====================================

    public void TickGauge(float deltaTime)
    {
        if (IsDead) return;

        float chargeAmount = Speed * deltaTime * 0.5f;
        
        ActionGauge += chargeAmount;
    }
    
    public void OnTurnStart(float cost)
    {
        ActionGauge -= cost;
        OnTurnStartEvent?.Invoke(this);
    }
    
    public void OnTurnEnd()
    {
      OnTurnEndEvent?.Invoke(this);
    }

    public void ModifiedActionGuage(float amount)
    {
        ActionGauge += amount;
    }

    // ==============================================
    // 피격 / 치유 / 비용 로직 
    // ==============================================

    public void ReceiveAttack(DamageContext ctx)
    {
        if (IsDead) return;
        
        // 1. 맞기 전 이벤트
        OnBeforeTakeDamage?.Invoke(ctx);

        if (!ctx.IsHit) return;

        // 2. 체력 변경
        float damage = ctx.FinalResult;
        if (damage > 0)
        {
            CurrentHP -= damage;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Die();
            }
        }
        else if (damage < 0) // 데미지가 음수면 힐 처리
        {
            Heal(-damage);
        }
        
        // 3. UI 갱신 및 맞은 후 이벤트 
        OnHPChanged?.Invoke(CurrentHP / MaxHP);
        OnTakeDamage?.Invoke(ctx);
    }

    public void Heal(float amount)
    {
        if(IsDead) return;
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        OnHPChanged?.Invoke(CurrentHP / MaxHP);
    }

    public void Die()
    {
        if(IsDead) return;
        IsDead = true;
        OnDeath?.Invoke(this);
    }
    
    // =============================================
    // 자원 체크 및 소모
    // =============================================
    
    public bool HasEnoughCost(SkillSO skill)
    {
        SkillResourceType resourceType = skill.ResourceType;
        int skillResource = skill.ResourceCost;
        return resourceType switch
        {
            SkillResourceType.NONE => true,
            SkillResourceType.HP => CurrentHP > skillResource,
            SkillResourceType.SP => CurrentSP > skillResource,
            SkillResourceType.MP => CurrentMP > skillResource,
            SkillResourceType.DP => CurrentDP > skillResource,
            _ => true
        };
    }
    
    public void ConsumeCost(SkillSO skill)
    {
        SkillResourceType resourceType = skill.ResourceType;
        int skillResource = skill.ResourceCost;

        switch (resourceType)
        {
            case  SkillResourceType.HP:
                CurrentHP = Mathf.Max(1, CurrentHP - skillResource);
                OnHPChanged?.Invoke(CurrentHP / MaxHP);
                break;
            case  SkillResourceType.SP:
                CurrentSP = Mathf.Max(1, CurrentSP - skillResource);
                break;
            case  SkillResourceType.MP:
                CurrentMP = Mathf.Max(1, CurrentMP - skillResource);
                break;
            case  SkillResourceType.DP:
                CurrentDP = Mathf.Max(1, CurrentDP - skillResource);
                break;
        }
        
    }
    
    
    // =============================================
    // 유틸리티
    // =============================================
    
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
    
    private float CalculateGrowth(float baseValue, float growthRate)
    {
        if (Level <= 1) return baseValue;

        // 공식: 기본값 * (1 + (레벨-1) * 계수)
        float multiplier = 1.0f + ((Level - 1) * growthRate);
    
        // 소수점 버림 or 반올림 (깔끔한 UI를 위해 int형 변환 추천)
        return Mathf.Floor(baseValue * multiplier);
    }
}