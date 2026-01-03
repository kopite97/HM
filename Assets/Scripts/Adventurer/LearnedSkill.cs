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
    }

    public int CurrentCost => Data.Cost_Value + (Data.Cost_Growth * (Level - 1));
    public float CurrentCooldown => Mathf.Max(1f,Data.Cooldown - (Data.Cooldown_Reduction * (Level - 1)));
    
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

}