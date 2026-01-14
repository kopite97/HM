using UnityEngine;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;

[System.Serializable]
public class LearnedSkill
{ 
    public SkillSO SourceSkill { get; private set; }
    public int Level { get; private set; }
    public float MaxCooldown { get; private set; }
    public float CurrentCooldown { get; private set; }
    public bool IsHeal { get; private set; }
    public float Range { get; private set; }
   
    public DamageType DamageType { get; private set; }
    public float Damage { get; private set; }
    public SkillTarget SkillTarget { get; private set; }
    public int TargetCount { get; private set; }
    public SkillResourceType ResourceType { get; private set; }
    public float ResourceCost { get; private set; }

    public StatType[] BaseStats { get; private set; }
    public SkillModifier[] Modifiers { get; private set; }
    public float[] ModifiersValue { get; private set; }
    public SkillAction[] Actions { get; private set; }
    public float[] ActionsValue { get; private set; }
    public StatusEffect[] StatusEffects { get; private set; }
    public float[] EffectsValue { get; private set; }
    public Trait[]  Traits { get; private set; }
    
    // 캐싱 
    public float[] CachedPowerCoefs { get; private set; } // 현재 스킬의 계수 
    public float CachedDamage { get; private set; }

    public bool HasModifiers { get; private set; } = false;
    public bool HasActions { get; private set; } = false;
    public bool HasEffects { get; private set; } = false;
    public bool HasTraits { get; private set; } = false;


    public LearnedSkill(SkillSO skillSO, int initialLevel = 1)
    {
        Initialize(skillSO, initialLevel);
    }

    public LearnedSkill(LearnedSkill learnedSkill)
    {
        Initialize(learnedSkill.SourceSkill, learnedSkill.Level);
    }

    public DamageType GetDamageType()
    {
        return SourceSkill.DamageType;
    }

    public void LevelUp()
    {
        Level++;
        RefreshCachedStats();
        // TODO : 플레이어의 모험가는 이펙트나 로그 출력
    }
    
    
    public void Use()
    {
        CurrentCooldown = MaxCooldown;
    }
    
    public void DecreaseCooldown(float amount = 1)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= amount;
            if (CurrentCooldown < 0) CurrentCooldown = 0;
        }
    }

    private void Initialize(SkillSO skillSO, int level)
    {
        SourceSkill = skillSO;
        Level = level;
        CurrentCooldown = 0;
        Range = SourceSkill.Range;
        DamageType =  SourceSkill.DamageType;
        Damage = SourceSkill.Damage;
        SkillTarget = SourceSkill.Target;
        TargetCount = SourceSkill.TargetCount;
        ResourceType = SourceSkill.ResourceType;
        ResourceCost = SourceSkill.ResourceCost;
        BaseStats = SourceSkill.BaseStats;
        
        Modifiers = SourceSkill.Modifiers;
        ModifiersValue = SourceSkill.ModifiersValue;
        Actions = SourceSkill.Actions;
        ActionsValue =  SourceSkill.ActionsValue;
        StatusEffects = SourceSkill.StatusEffects;
        EffectsValue = SourceSkill.EffectsValue;
        Traits = SourceSkill.Traits;
        
        
        foreach (var action in skillSO.Actions)
        {
            if (action == SkillAction.Heal)
            {
                IsHeal = true;
                break;
            }
        }
        
        RefreshCachedStats();
        HasSkillProperties();
    }
    
    private void RefreshCachedStats()
    {
        // 최대 쿨타임 캐싱
        float reduction = SourceSkill.CooldownReductionPerLv * (Level - 1);
        int finalCooldown = Mathf.FloorToInt(SourceSkill.Cooldown - reduction);
        MaxCooldown = Mathf.Max(0, finalCooldown);
        
        // 계수 캐싱
        if (CachedPowerCoefs == null || CachedPowerCoefs.Length != SourceSkill.PowerCoefs.Length)
        {
            CachedPowerCoefs = new float[SourceSkill.PowerCoefs.Length];
        }

        for (int i = 0; i < CachedPowerCoefs.Length; i++)
        {
            float growth = (SourceSkill.PowerGrowthPerLv != null && i<SourceSkill.PowerCoefs.Length) ? SourceSkill.PowerGrowthPerLv[i] : 0;
            CachedPowerCoefs[i] = SourceSkill.PowerCoefs[i] + (growth * (Level - 1));
        }
        
        
    }


    private void HasSkillProperties()
    {
        HasModifiers = Modifiers != null && Modifiers.Length > 0 && Modifiers[0] != SkillModifier.None;
        HasActions = Actions != null && Actions.Length > 0 && Actions[0] != SkillAction.None;
        HasEffects = StatusEffects != null && StatusEffects.Length > 0 && StatusEffects[0] != StatusEffect.None;
        HasTraits = Traits != null && Traits.Length > 0 && Traits[0] != Trait.None;
    }
}