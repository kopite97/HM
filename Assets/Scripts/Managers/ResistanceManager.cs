using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public struct ResistanceFactor
{
    public StatType Stat;
    public NatureType Nature;
    public float Coefficient;
    public bool IsNature;
}

public class ResistanceManager : Singleton<ResistanceManager>
{
    /// <summary>
    /// 단일 속성에 대한 저항력 계산
    /// </summary>
    /// <param name="defender"></param>
    /// <param name="???"></param>
    /// <returns></returns>
    public float CalculateSingleResistance(BattleUnit defender, DamageType type)
    {
        if (!DataManager.Instance.ResistanceRules.TryGetValue(type, out var formulas))
        {
            return 0f; // 데미지 저항 공식이 없으면 True 데미지
        }

        float totalRes = 0f;
        
        // 정의된 공식대로 스탯 합산
        foreach (var factor in formulas)
        {
            float statValue = 0f;
            if (factor.IsNature)
            {
                statValue = defender.GetNatureStat(factor.Nature);
            }
            else
            {
                statValue = defender.GetStat(factor.Stat);
            }

            totalRes += statValue * factor.Coefficient;
        }

        return totalRes;
    }
    
}