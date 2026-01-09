using System.Collections.Generic;
using Sirenix.OdinInspector;
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
    
    [TabGroup("Balance","Defense")] [ShowInInspector, ReadOnly]
    private DefenseWeightData _defenseWeightData;

    [TabGroup("Balance","Defense")]
    [SerializeField,FolderPath]
    private string defenseWeightDataResourcePath = "/SO/Balance";
    
    [TabGroup("Balance","Resistance")]
    private Dictionary<DamageType,List<ResistanceFactor>> _resistanceRules = new Dictionary<DamageType, List<ResistanceFactor>>();

    public override void Initialize()
    {
        LoadDefenseWeightData();
    }

    public void SetResistanceRules(Dictionary<DamageType, List<ResistanceFactor>> resistanceRules)
    {
        _resistanceRules = resistanceRules;
    }
    
    /// <summary>
    /// 단일 속성에 대한 저항력 계산
    /// </summary>
    /// <param name="defender"></param>
    /// <param name="???"></param>
    /// <returns></returns>
    public float CalculateSingleResistance(BattleUnit defender, DamageType type)
    {
        if (!_resistanceRules.TryGetValue(type, out var formulas))
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
    
    private void LoadDefenseWeightData()
    {
        _defenseWeightData = Resources.Load<DefenseWeightData>(defenseWeightDataResourcePath);
    }

    public DefenseWeightData GetDefenseWeightData()
    {
        return _defenseWeightData;
    }
}