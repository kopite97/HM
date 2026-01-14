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
        _resistanceRules = DataManager.Instance.GetResistanceRules();
    }
    
    /// <summary>
    /// 단일 속성에 대한 저항력 계산
    /// </summary>
    /// <param name="defender"></param>
    /// <param name="???"></param>
    /// <returns></returns>
    public float CalculateSingleResistance()
    {

        return 0;
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