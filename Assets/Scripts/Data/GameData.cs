using System.Collections.Generic;

// CSV랑 Row 이름을 동일하게 맞출 것

// 1. 직업 데이터 (ClassData)
using System.Collections.Generic;

[System.Serializable]
public class ClassData
{
    public int ID;
    // CSV 컬럼명인 Name_KR, Name_EN 등에 맞춰 언더바(_) 추가
    public string Name_KR;
    public string Name_EN;
    public string Desc_KR;
    public string Desc_EN;
    
    // 가중치 (Weights)
    public int W_Might, W_Endurance, W_Reflex, W_Poise, W_Mobility, W_Vitality;
    public int W_Valor, W_Composure, W_Focus, W_Judgment, W_Resolve, W_Insight, W_Awareness, W_Command;
    public int W_Arms, W_Archery, W_Sorcery, W_Faith, W_Subtlety, W_Guarding;
    
    // 성격 가중치 (CSV 컬럼명과 일치 확인됨)
    public int W_Nature_Duty, W_Nature_Discord, W_Nature_Patience, W_Nature_Ambition;
    public int W_Nature_Greed, W_Nature_Cunning, W_Nature_Arrogance, W_Nature_Stubborn;
    public int W_Nature_Honor, W_Nature_Loyalty;
}

// Monster, Skill, Dungeon 데이터는 추후 CSV 구조에 맞춰 변수명을 조정하시면 됩니다.

// 2. 몬스터 데이터 (MonsterData)
[System.Serializable]
public class MonsterData
{
    public int ID;
    public string NameKR;
    public string NameEN;
    public int Level;
    public int HP;
    public int Atk_Power;
    public int Def_Power;
    public int Speed;
    public string Type_Atk; // ENUM으로 변환 가능
    public string Type_Def;
    public string AI_Pattern;
    public List<int> Skill_IDs; // 3001, 3005...
    public int Drop_Gold_Min;
    public int Drop_Gold_Max;
    public int Drop_Exp;
}

// 3. 스킬 데이터 (SkillData)
[System.Serializable]
public class SkillData
{
    public int ID;
    public string NameKR;
    public string NameEN;
    public string Type; // ACTIVE, PASSIVE
    public string Target;
    public string Cost_Type;
    public int Cost_Value;
    public int Cooldown;
    public string[] Base_Stats; // Body_Might, Tech_Faith...
    public float[] Power_Coefs; // 1.5, 0.8...
    public string Effect_Tag;
    public float Effect_Value;
    public string DescKR;
    public string DescEN;
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