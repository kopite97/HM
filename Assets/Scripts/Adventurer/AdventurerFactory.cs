using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Random = UnityEngine.Random;

public class AdventurerFactory : Singleton<AdventurerFactory>
{
    // [변경] 캐시 키가 string -> Enum으로 변경됨
    private Dictionary<int, Dictionary<StatType, int>> _statWeightCache = new Dictionary<int, Dictionary<StatType, int>>();
    private Dictionary<int, Dictionary<NatureType, int>> _natureWeightCache = new Dictionary<int, Dictionary<NatureType, int>>();
    
    private bool _isInitialized = false;

    private void Start()
    {
        if (!_isInitialized && DataManager.Instance != null && DataManager.Instance.ClassDict.Count > 0)
        {
            InitializeStatCache();
        }
    }
    
    private void InitializeStatCache()
    {
        _statWeightCache.Clear();
        _natureWeightCache.Clear();
        
        FieldInfo[] allFields = typeof(ClassData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        // 필드와 Enum 타입을 매핑하여 저장할 리스트
        var statFieldMap = new List<(FieldInfo field, StatType type)>();
        var natureFieldMap = new List<(FieldInfo field, NatureType type)>();

        foreach (var field in allFields)
        {
            // 1. 성격 필드 파싱 (W_Nature_...)
            if (field.Name.StartsWith("W_Nature_"))
            {
                string enumName = field.Name.Substring(9); // "W_Nature_" 제거
                if (Enum.TryParse(enumName, out NatureType nType))
                {
                    natureFieldMap.Add((field, nType));
                }
                else
                {
                    Debug.LogWarning($"[Factory] 성격 Enum에 없는 데이터 발견: {enumName}");
                }
            }
            // 2. 스탯 필드 파싱 (W_...)
            else if (field.Name.StartsWith("W_"))
            {
                string enumName = field.Name.Substring(2); // "W_" 제거
                if (Enum.TryParse(enumName, out StatType sType))
                {
                    statFieldMap.Add((field, sType));
                }
            }
        }

        // 실제 데이터 캐싱
        foreach (var kvp in DataManager.Instance.ClassDict)
        {
            int classId = kvp.Key;
            ClassData data = kvp.Value;
            
            // 전투 스탯 캐싱
            Dictionary<StatType, int> statWeights = new Dictionary<StatType, int>();
            foreach (var map in statFieldMap)
            {
                int value = (int)map.field.GetValue(data);     
                statWeights.Add(map.type, value);
            }
            _statWeightCache.Add(classId, statWeights);

            // 성격 캐싱
            Dictionary<NatureType, int> natureWeights = new Dictionary<NatureType, int>();
            foreach (var map in natureFieldMap)
            {
                int value = (int)map.field.GetValue(data);
                natureWeights.Add(map.type, value);
            }
            _natureWeightCache.Add(classId, natureWeights);
        }

        _isInitialized = true;
        Debug.Log($"[Factory] Enum 기반 캐싱 완료! (스탯 종류: {statFieldMap.Count}, 성격 종류: {natureFieldMap.Count})");
    }

    public Adventurer CreateRandomAdventurer(int classID)
    {
        if (!_isInitialized) InitializeStatCache();

        if (DataManager.Instance == null || !DataManager.Instance.ClassDict.ContainsKey(classID)) return null;
        if (!_statWeightCache.ContainsKey(classID) || !_natureWeightCache.ContainsKey(classID)) return null;

        ClassData data = DataManager.Instance.ClassDict[classID];

        int age = Random.Range(15, 66);
        string name = "모험가_" + Random.Range(1000, 9999);
        Adventurer newAdv = new Adventurer(name, age, data.ID);
        newAdv.JobName = data.NameKR;

        int totalPot = RollTotalPotential();
        newAdv.TotalPotential = totalPot;
        newAdv.PotentialGrade = GetGradeString(totalPot);

        // [순서: 성격 -> 스탯]
        SetNatures(newAdv, data);
        DistributeStats(newAdv, data, totalPot, age);

        return newAdv;
    }

    void DistributeStats(Adventurer adv, ClassData data, int totalPot, int age)
    {
        Dictionary<StatType, int> weights = _statWeightCache[data.ID];
        
        float totalWeight = weights.Values.Sum();
        if (totalWeight <= 0) totalWeight = 1;

        float ageEfficiency = GetAgeEfficiency(age);
        int targetCurrentTotal = Mathf.RoundToInt(totalPot * ageEfficiency);

        // 1. 기본 분배
        foreach (var kvp in weights)
        {
            StatType type = kvp.Key;
            int weight = kvp.Value;

            float variance = Random.Range(0.8f, 1.2f);
            float ratio = (float)weight / totalWeight;
            
            int statValue = Mathf.RoundToInt(ratio * targetCurrentTotal * variance);
            if (statValue < 1) statValue = 1;

            adv.Stats[type] = statValue;
        }

        // 2. 한계 돌파 및 보정
        int currentSum = adv.CurrentTotalStat;
        int limit = GetStatLimit(adv); // 한계치 가져오기

        if (currentSum > limit)
        {
            float scaleRatio = (float)limit / currentSum;

            if (limit > adv.TotalPotential)
                Debug.Log($"<b>[한계 돌파]</b> {adv.Name}: 그릇을 넘었습니다! ({currentSum} -> {limit})"); // 나중에 알림으로 써도 될듯?

            // Enum 키 리스트로 순회
            List<StatType> keys = new List<StatType>(adv.Stats.Keys);
            foreach (StatType key in keys)
            {
                int originalVal = adv.Stats[key];
                int newVal = Mathf.FloorToInt(originalVal * scaleRatio);
                if (newVal < 1) newVal = 1;
                adv.Stats[key] = newVal;
            }
        }
    }

    // 한계치 계산 -> duty, patience, ambition, honor 
    int GetStatLimit(Adventurer adv)
    {
        int limit = adv.TotalPotential;

        // Enum 키를 사용하여 안전하게 접근
        int spiritSum = 0;
        spiritSum += GetNatureValue(adv, NatureType.Duty);
        spiritSum += GetNatureValue(adv, NatureType.Patience);
        spiritSum += GetNatureValue(adv, NatureType.Ambition);
        spiritSum += GetNatureValue(adv, NatureType.Honor);

        // 한계 돌파 로직
        if (spiritSum >= 320) limit = Mathf.RoundToInt(limit * 1.15f); // 15%
        else if (spiritSum >= 260) limit = Mathf.RoundToInt(limit * 1.05f); // 5%

        return limit;
    }

    int GetNatureValue(Adventurer adv, NatureType type)
    {
        return adv.Natures.ContainsKey(type) ? adv.Natures[type] : 0;
    }

    void SetNatures(Adventurer adv, ClassData data)
    {
        Dictionary<NatureType, int> weights = _natureWeightCache[data.ID];

        foreach (var kvp in weights)
        {
            adv.Natures[kvp.Key] = CalculateNatureValue(kvp.Value);
        }
    }

    int CalculateNatureValue(int weight) => Mathf.Clamp(20 + (weight * 6) + Random.Range(-10, 11), 0, 100);
    float GetAgeEfficiency(int age) => (age < 40) ? Mathf.Lerp(0.3f, 1.0f, (age - 15f) / 25f) : Mathf.Lerp(1.0f, 0.7f, (age - 40f) / 25f);
    int RollTotalPotential() => Random.Range(25, 101) + Random.Range(25, 101);
    string GetGradeString(int pot) => (pot >= 200) ? "The One" : (pot >= 180) ? "S" : (pot >= 150) ? "A" : (pot >= 100) ? "B" : "C";
}