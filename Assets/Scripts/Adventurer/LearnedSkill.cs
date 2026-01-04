using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class LearnedSkill
{
    public SkillData Data;
    public int Level;

    public LearnedSkill(SkillData data, int initialLevel = 1)
    {
        Data = data;
        Level = initialLevel;
        CurrentCooldown = 0;
    }

    public LearnedSkill(LearnedSkill learnedSkill)
    {
        Data = learnedSkill.Data;
        Level = learnedSkill.Level;
        CurrentCooldown = 0;
    }

    public int CurrentCost => Data.Cost_Value + (Data.Cost_Growth * (Level - 1));
    public int CurrentCooldown { get; private set; } = 0;

    public int MaxCoolDown
    {
        get
        {
            float reduction = Data.Cooldown_Reduction * (Level - 1);
            int finalCd = Mathf.FloorToInt(Data.Cooldown - reduction);
            return Mathf.Max(0,finalCd);
        }
    }

    public float[] GetCurrentPowerCoefs()
    {
        float[] currentCoefs = new float[Data.Power_Coefs.Length];

        for (int i = 0; i < Data.Power_Coefs.Length; i++)
        {
            float growth = (Data.Power_Growth != null && i<Data.Power_Growth.Length) ? Data.Power_Growth[i] : 0f;
            currentCoefs[i] = Data.Power_Coefs[i] + (growth * (Level - 1));
                
        }

        return currentCoefs;
    }

    public void LevelUp()
    {
        Level++;
        // TODO : 플레이어의 모험가는 이펙트나 로그 출력
    }
    
    public bool IsReady(BattleUnit actor)
    {
        if (CurrentCooldown > 0) return false;

        if (!actor.HasEnoughCost(Data)) return false;
        
        // 상태이상 체크는 BattleUnit에서
        return true;
    }

    public void Use()
    {
        CurrentCooldown = MaxCoolDown;
    }
    
    public void DecreaseCooldown(int amount = 1)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= amount;
            if (CurrentCooldown < 0) CurrentCooldown = 0;
        }
    }
}