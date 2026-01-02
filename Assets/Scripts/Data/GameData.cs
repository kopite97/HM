using System.Collections.Generic;

// CSV랑 Row 이름을 동일하게 맞출 것

// 1. 직업 데이터 (ClassData)
using System.Collections.Generic;

[System.Serializable] 
public class ClassData
{
    public int ID;
    public string Name_KR;
    public string Name_EN;
    public string Desc_KR;
    public string Desc_EN;
    
    // 가중치 (Weights)
    public int W_Body_Might, W_Body_Endurance, W_Body_Reflex, W_Body_Poise, W_Body_Mobility, W_Body_Vitality;
    public int W_Mind_Valor, W_Mind_Composure, W_Mind_Focus, W_Mind_Judgment, W_Mind_Resolve, W_Mind_Insight, W_Mind_Awareness, W_Mind_Command;
    public int W_Tech_Arms, W_Tech_Archery, W_Tech_Sorcery, W_Tech_Faith, W_Tech_Subtlety, W_Tech_Guarding;
    
    // 성격 가중치 
    public int W_Nature_Duty, W_Nature_Discord, W_Nature_Patience, W_Nature_Ambition;
    public int W_Nature_Greed, W_Nature_Cunning, W_Nature_Arrogance, W_Nature_Stubborn;
    public int W_Nature_Honor, W_Nature_Loyalty;
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
    public string Desc;
    
    // (CSV 파싱 전 임시 저장용)
    public string SkillIDs_Str; 
    
    // --- 스탯 & 성격 데이터 저장소 ---
    public Dictionary<StatType, int> BaseStats = new Dictionary<StatType, int>();
    public Dictionary<NatureType, int> BaseNatures = new Dictionary<NatureType, int>();
    public List<int> SkillIDs = new List<int>();
}

// 3. 스킬 데이터 (SkillData)
[System.Serializable]
public class SkillData
{
    public int ID;
    public string NameKR;
    public SkillType Type;
    public SkillTarget Target;
    public SkillCostType Cost_Type;
    public SkillRange Range;
    public EffectTag Effect_Tag;
    public DamageType Damage_Type;

    // 레벨 1 기준 기본값 
    public int Cost_Value;
    public int Cooldown;
    public StatType[] Base_Stats; 
    public float[] Power_Coefs;   // [예: 1.5, 0.8]
    public float Effect_Value;

    // 레벨당 성장 수치 (성장 설계도) 
    public int Cost_Growth;          // 레벨당 증가할 코스트 (+2 등)
    public float Cooldown_Reduction; // 레벨당 감소할 쿨타임 (-0.5 등)
    public float[] Power_Growth;     // 계수별 성장치 [예: 0.1, 0.05]
    public float Effect_Growth;      // 효과값 성장치 (+1.0 등)
    
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

public class ResistanceConfig
{
    
}