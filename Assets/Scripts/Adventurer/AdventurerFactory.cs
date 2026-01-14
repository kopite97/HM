using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Random = UnityEngine.Random;

public class AdventurerFactory : Singleton<AdventurerFactory>
{
    // [변경] 캐시 키가 string -> Enum으로 변경됨
    private Dictionary<int, Dictionary<StatType, float>> _statWeightCache = new Dictionary<int, Dictionary<StatType, float>>();
    private Dictionary<int, Dictionary<NatureType, float>> _natureWeightCache = new Dictionary<int, Dictionary<NatureType, float>>();
    
    private bool _isInitialized = false;
    
    // 스탯 하나당 최대치 
    private const float INPUT_MAX = 10f; // 가중치 최대값
    private const float MAX_STAT_VALUE = 100f;
    private const float MAX_NATURE_VALUE = 100f;
    private const float MIN_POTENTIAL_RATIO = 0.4f;
    private const float MAX_POTENTIAL_RATIO = 0.8f;
    private const float LIMIT_BREAK_HIGH_RATIO = 0.93f;
    private const float LIMIT_BREAK_MID_RATIO = 0.85f;
    private const float LIMIT_BREAK_HIGH_MULTIPLIER = 1.15f;
    private const float LIMIT_BREAK_MID_MULTIPLIER = 1.05f;

    private int _statCount = 0;
    private float _theoreticalMaxTotal = 0;
    private int _adventurerCount = 0;

    
    public override void Initialize()
    {
        InitializeStatCache();
    }
    
    private void InitializeStatCache()
    {
        _statWeightCache.Clear();
        _natureWeightCache.Clear();
        
        FieldInfo[] allFields = typeof(ClassData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        Debug.Log($"[Factory] ClassData에서 발견된 전체 필드 수: {allFields.Length}");
        // 필드와 Enum 타입을 매핑하여 저장할 리스트
        var statFieldMap = new List<(FieldInfo field, StatType type)>();
        var natureFieldMap = new List<(FieldInfo field, NatureType type)>();

        foreach (var field in allFields)
        {
            // 1. 성격 필드 파싱 (W_Nature_...)
            if (field.Name.StartsWith("W_Nature_"))
            {
                string enumName = field.Name.Substring(2); // "W_" 제거
                if (Enum.TryParse(enumName,true, out NatureType nType))
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
                if (Enum.TryParse(enumName,true, out StatType sType))
                {
                    statFieldMap.Add((field, sType));
                }
            }
        }

        // 실제 데이터 캐싱
        foreach (var kvp in AdventurerManager.Instance.GetClassDataDict())
        {
            int classId = kvp.Key;
            ClassData data = kvp.Value;
            
            // 전투 스탯 캐싱
            Dictionary<StatType, float> statWeights = new Dictionary<StatType, float>();
            foreach (var map in statFieldMap)
            {
                float value = (float)map.field.GetValue(data);     
                statWeights.Add(map.type, value);
            }
            _statWeightCache.Add(classId, statWeights);

            // 성격 캐싱
            Dictionary<NatureType, float> natureWeights = new Dictionary<NatureType, float>();
            foreach (var map in natureFieldMap)
            {
                float value = (float)map.field.GetValue(data);
                natureWeights.Add(map.type, value);
            }
            _natureWeightCache.Add(classId, natureWeights);
        }

        _statCount = statFieldMap.Count;
        _theoreticalMaxTotal = _statCount * MAX_STAT_VALUE;

        _isInitialized = true;
        
        Debug.Log($"[Factory] Enum 기반 캐싱 완료! (스탯 종류: {statFieldMap.Count}, 성격 종류: {natureFieldMap.Count})");
    }

    public Adventurer CreateRandomAdventurer(int classID)
    {
        if (!_isInitialized) InitializeStatCache();

        if (AdventurerManager.Instance.ContainsKey(classID)) return null;
        if (!_statWeightCache.ContainsKey(classID) || !_natureWeightCache.ContainsKey(classID)) return null;

        ClassData data = AdventurerManager.Instance.GetClassData(classID);
        

        int age = Random.Range(15, 66);
        string name = "모험가_" + Random.Range(1000, 9999);
        Adventurer newAdv = new Adventurer(_adventurerCount++,name, age, data.ID);
        newAdv.SetJobName(data.Name_KR);
        
        int totalPot = RollTotalPotential();
        newAdv.SetPotential(totalPot, GetGradeString(totalPot));
        
        SetNatures(newAdv, data);
        DistributeStats(newAdv, data, totalPot, age);
        
        // [테스트용 스킬 부여 로직]
        // 나중에는 ClassData.MainSkillID를 참조하게 바꿀 것
        int skillIdToLearn = 0;
    
        if (classID == 1001) skillIdToLearn = 1001; // 전사 -> 강격
        else if (classID == 1002) skillIdToLearn = 3001; // 마법사 -> 화염구
        else skillIdToLearn = 2003; // 그 외 -> 단검 투척(예시)

       
        if (SkillManager.Instance.TryGetSkill(skillIdToLearn,out SkillSO skill))
        {
            // 스킬 습득 (기본 1레벨)
            newAdv.LearnSkill(skill, 1);
            Debug.Log($"[Factory] {newAdv.Name}에게 스킬 부여 완료: {skill.Name}");
        }
        else
        {
            Debug.LogWarning($"[Factory] 스킬 ID {skillIdToLearn}를 찾을 수 없습니다.");
        }

        // 스킬 습득 후 포지션 분석
        //newAdv.AnalyzePreferredPosition(DataManager.Instance.DefenseWeight);
        return newAdv;
    }
    
    private void DistributeStats(Adventurer adv, ClassData data, int totalPot, int age)
        {
        if (!_statWeightCache.TryGetValue(data.ID, out var jobWeights)) return;
    
        // 목표치 및 한계치 설정
        float ageEfficiency = GetAgeEfficiency(age);
        int targetAbility = Mathf.FloorToInt(totalPot * ageEfficiency);
        int sumLimit = GetStatLimit(adv);               
        bool isLimitBroken = sumLimit > totalPot;       
        
        int currentTotalScore = 0; 
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            adv.SetStat(type, 1);
            currentTotalScore += 1;
        }
        
        Dictionary<StatType, float> lotteryWeights = jobWeights.ToDictionary(
            k => k.Key, 
            v => Mathf.Max((float)v.Value, 0.1f) 
        );
        
        // 벌크 배분 
        int pointsNeeded = targetAbility - currentTotalScore;
        // 배분할 포인트가 충분히 많을 때만 벌크 배분 수행 
        if (pointsNeeded > 50)
        {
            int bulkPoints = Mathf.FloorToInt(pointsNeeded * 0.8f); // 80% 선배분
            float totalWeightSum = lotteryWeights.Values.Sum();
            
            List<StatType> keys = new List<StatType>(lotteryWeights.Keys);
            
            foreach (var type in keys)
            {
                float ratio = lotteryWeights[type] / totalWeightSum;
                
                float noise = Random.Range(0.9f, 1.1f); 
                int addAmount = Mathf.FloorToInt(bulkPoints * ratio * noise);
    
                if (addAmount > 0)
                {
                    float currentVal = adv.GetStat(type);
                    float safeAdd = Mathf.Min(addAmount, MAX_STAT_VALUE - currentVal);
                    
                    if (safeAdd > 0)
                    {
                        adv.SetStat(type, currentVal + safeAdd);
                        currentTotalScore += (int)safeAdd;
                    }
                }
            }
        }
        
        // 잔여 포인트 랜덤 배분 
        while (currentTotalScore < targetAbility && currentTotalScore < sumLimit)
        {
            StatType selected = GetWeightedRandomStat(lotteryWeights);
            float currentVal = adv.GetStat(selected);
    
            if (currentVal < MAX_STAT_VALUE)
            {
                adv.SetStat(selected, currentVal + 1);
                currentTotalScore++; // 변수 업데이트
            }
            else
            {
                lotteryWeights.Remove(selected);
                if (lotteryWeights.Count == 0) break;
            }
        }
    
        // 추가 성장
        if (isLimitBroken && currentTotalScore < sumLimit)
        {
            while (currentTotalScore < sumLimit)
            {
                StatType selected = GetWeightedRandomStat(lotteryWeights);
                float currentVal = adv.GetStat(selected);
    
                if (currentVal < MAX_STAT_VALUE)
                {
                    adv.SetStat(selected, currentVal + 1);
                    currentTotalScore++;
                }
                else
                {
                    lotteryWeights.Remove(selected);
                    if (lotteryWeights.Count == 0) break;
                }
            }
        }
    
        // 7. 결과 보고
        string limitColor = isLimitBroken ? "cyan" : "white";
        // 로그 찍을 때만 최후 검증용으로 Sum() 사용
        Debug.Log($"<color={limitColor}><b>[{adv.Name}]</b></color> 생성 완료. " +
                  $"스탯총합: {currentTotalScore} (실제: {adv.Stats.Values.Sum()})");
    }

    // 가중치 랜덤 선택 로직
    private StatType GetWeightedRandomStat(Dictionary<StatType, float> weights)
    {
        float totalSum = weights.Values.Sum();
        float rand = Random.Range(0f, totalSum);
        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (rand < cumulative) return kvp.Key;
        }
        return weights.Keys.First();
    }
    
    float CalculateNatureValue(float weight)
    {
        float scaleFactor = MAX_NATURE_VALUE / INPUT_MAX; 
        float baseVal = weight * scaleFactor; 
        float variance = Random.Range(-2.0f * scaleFactor, 2.0f * scaleFactor); 

        return Mathf.Clamp(baseVal + variance, 1, MAX_NATURE_VALUE);
    }

    // 한계치 계산 -> duty, patience, ambition, honor 
    int GetStatLimit(Adventurer adv)
    {
        int limit = adv.TotalPotential;

        float spiritSum = 0f;
        spiritSum += GetNatureValue(adv, NatureType.Nature_Duty);
        spiritSum += GetNatureValue(adv, NatureType.Nature_Patience);
        spiritSum += GetNatureValue(adv, NatureType.Nature_Ambition);
        spiritSum += GetNatureValue(adv, NatureType.Nature_Honor);

        // 성격 4개 만점 기준 비율 계산
        float maxSpirit = MAX_NATURE_VALUE * 4f;
        
        if (spiritSum >= maxSpirit * LIMIT_BREAK_HIGH_RATIO) 
            limit = Mathf.RoundToInt(limit * LIMIT_BREAK_HIGH_MULTIPLIER);
        else if (spiritSum >= maxSpirit * LIMIT_BREAK_MID_RATIO) 
            limit = Mathf.RoundToInt(limit * LIMIT_BREAK_MID_MULTIPLIER);

        return limit;
    }

    float GetNatureValue(Adventurer adv, NatureType type)
    {
        return adv.Natures.ContainsKey(type) ? adv.Natures[type] : 0;
    }

    void SetNatures(Adventurer adv, ClassData data)
    {
        Dictionary<NatureType, float> weights = _natureWeightCache[data.ID];

        foreach (var kvp in weights)
        {
            adv.SetNature(kvp.Key, CalculateNatureValue(kvp.Value));
        }
    }
    
    float GetAgeEfficiency(int age) => (age < 40) ? Mathf.Lerp(0.3f, 1.0f, (age - 15f) / 25f) : Mathf.Lerp(1.0f, 0.7f, (age - 40f) / 25f);

    int RollTotalPotential()
    {
        float minPot = _theoreticalMaxTotal * MIN_POTENTIAL_RATIO;
        float maxPot = _theoreticalMaxTotal * MAX_POTENTIAL_RATIO;

        float halfMin = minPot * 0.5f;
        float halfMax = maxPot * 0.5f;
        int val1 = Random.Range(Mathf.RoundToInt(halfMin), Mathf.RoundToInt(halfMax) + 1);
        int val2 = Random.Range(Mathf.RoundToInt(halfMin), Mathf.RoundToInt(halfMax) + 1);
        
        return val1 + val2;
    }

    string GetGradeString(int pot)
    {
        float ratio = (float)pot / _theoreticalMaxTotal;

        if (ratio >= 0.75f) return "The One"; // 상위 0.1% (만점의 75% 이상)
        if (ratio >= 0.675f) return "S";      // 상위 10%
        if (ratio >= 0.60f) return "A";       // 상위 30%
        if (ratio >= 0.50f) return "B";       // 평균
        return "C";
    }
}