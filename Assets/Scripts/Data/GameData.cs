using System.Collections.Generic;

// CSV랑 Row 이름을 동일하게 맞출 것

// 1. 직업 데이터 (ClassData)
using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable] 
public class ClassData
{
    public int ID;
    public string Name_KR;
    public string Name_EN;
    public string Desc_KR;
    public string Desc_EN;
    
    // 가중치 (Weights)
    public float W_Body_Might, W_Body_Endurance, W_Body_Reflex, W_Body_Poise, W_Body_Mobility, W_Body_Vitality;
    public float W_Mind_Valor, W_Mind_Composure, W_Mind_Focus, W_Mind_Judgment, W_Mind_Resolve, W_Mind_Insight, W_Mind_Awareness, W_Mind_Command;
    public float W_Tech_Arms, W_Tech_Archery, W_Tech_Sorcery, W_Tech_Faith, W_Tech_Subtlety, W_Tech_Guarding;
    
    // 성격 가중치 
    public float W_Nature_Duty, W_Nature_Discord, W_Nature_Patience, W_Nature_Ambition;
    public float W_Nature_Greed, W_Nature_Cunning, W_Nature_Arrogance, W_Nature_Stubborn;
    public float W_Nature_Honor, W_Nature_Loyalty;
}


// 2. 몬스터 데이터 (MonsterData)
[System.Serializable]
public class MonsterData
{
    // --- 기본 정보 (리플렉션으로 자동 매핑) ---
    public int ID;
    public string NameKR;
    public int Level;
    public float MaxHP;
    public float DefenseScore;

    public int[] Skill_List;
    public string Desc;
    
    // 스탯
    public float Body_Might;
    public float Body_Endurance;
    public float Body_Reflex;
    public float Body_Poise;
    public float Body_Mobility;
    public float Body_Vitality;
    
    public float Mind_Valor;
    public float Mind_Composure;
    public float Mind_Focus;
    public float Mind_Judgment;
    public float Mind_Resolve;
    public float Mind_Insight;
    public float Mind_Awareness;
    public float Mind_Command;
    
    public float Tech_Arms;
    public float Tech_Archery;
    public float Tech_Sorcery;
    public float Tech_Faith;
    public float Tech_Subtlety;
    public float Tech_Guarding;
    
    // 성격
    public float Nature_Duty;
    public float Nature_Discord;
    public float Nature_Patience;
    public float Nature_Ambition;
    public float Nature_Greed;
    public float Nature_Cunning;
    public float Nature_Arrogance;
    public float Nature_Stubborn;
    public float Nature_Honor;
    public float Nature_Loyalty;
}

// 3. 스킬 데이터 (SkillData)
[System.Serializable]
public class SkillData
{
    public int ID;
    public string NameKR;
    public SkillType Type;
    public SkillTarget Target;
    public SkillResourceType Resource_Type;
    public int Resource;
    public float Range;
    public DamageType Damage_Type;
    
    public SkillModifier[] Skill_Modifiers;
    public SkillAction[] Skill_Actions;
    public StatusEffect[] Status_Effects;
    public Trait[] Traits;
        
    // 레벨 1 기준 기본값 
    public int Cooldown;
    public StatType[] Base_Stats; 
    public float[] Power_Coefs;   // [예: 1.5, 0.8]

    // 레벨당 성장 수치 (성장 설계도) 
    public float Cooldown_Reduction; // 레벨당 감소할 쿨타임 (-0.5 등)
    public float[] Power_Growth;     // 계수별 성장치 [예: 0.1, 0.05]
    
    public string DescKR;
    
}

// 4. 던전 데이터 (DungeonData)
[System.Serializable]
public class DungeonData
{
    public int ID;
    public string NameKR;
    public string NameEN;
    public int Rank;
    public int Min_Day;
    public int Max_Day;
    public string[] Env_Tags;
    public List<int> Monster_IDs;
    public int Boss_ID;
    public int Reward_Gold_Min;
    public int Reward_Gold_Max;
    public string DescKR;
    public string DescEN;
}

// TODO 밑에 두개는 지우거나 리팩토링 해야됨
[System.Serializable]
public class EffectConfigData
{
    public int ID;
    public SkillAction action;
    public string Name;
    public StatType[] Hit_Stats;
    public float[] Hit_Coefs;
    public StatType[] Resist_Stats;
    public float[] Resist_Coefs;
    public float BestChance;
    public int BaseTurn;
}

[System.Serializable]
public class PropertyConfigData
{
    public int ID;
    public SkillModifier modifier;
    public string Name;
    public StatType[] Hit_Stats;
    public float[] Hit_Coefs;
    public StatType[] Resist_Stats;
    public float[] Resist_Coefs;
    public string Param_Desc;
    public string Desc;
}
