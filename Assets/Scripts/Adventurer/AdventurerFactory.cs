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
    private const float MAX_STAT_VALUE = 20;
    
    private const float LIMIT_BREAK_HIGH_THRESHOLD = 75;
    private const float LIMIT_BREAK_MID_THRESHOLD = 68;
    private const float LIMIT_BREAK_HIGH_MULTIPLIER = 1.15f;
    private const float LIMIT_BREAK_MID_MULTIPLIER = 1.05f;

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
        foreach (var kvp in DataManager.Instance.ClassDict)
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
        newAdv.SetJobName(data.Name_KR);
        
        int totalPot = RollTotalPotential();
        newAdv.SetPotential(totalPot, GetGradeString(totalPot));
        
        SetNatures(newAdv, data);
        DistributeStats(newAdv, data, totalPot, age);
        
        // [테스트용 스킬 부여 로직]
        // 나중에는 ClassData.MainSkillID를 참조하게 바꿀 것입니다.
        int skillIdToLearn = 0;
    
        if (classID == 1001) skillIdToLearn = 1001; // 전사 -> 강격
        else if (classID == 1002) skillIdToLearn = 3001; // 마법사 -> 화염구
        else skillIdToLearn = 2003; // 그 외 -> 단검 투척(예시)

        if (DataManager.Instance.SkillDict.TryGetValue(skillIdToLearn, out SkillData skillData))
        {
            // 스킬 습득 (기본 1레벨)
            newAdv.LearnSkill(skillData, 1);
            Debug.Log($"[Factory] {newAdv.Name}에게 스킬 부여 완료: {skillData.NameKR}");
        }
        else
        {
            Debug.LogWarning($"[Factory] 스킬 ID {skillIdToLearn}를 찾을 수 없습니다.");
        }

        // 스킬 습득 후 포지션 분석
        newAdv.AnalyzePreferredPosition(DataManager.Instance.DefenseWeight);
        return newAdv;
    }

    private SkillRange GetJobMainRange(int classID)
    {
        // 일단은 임시로.. 
        // 추후에 SKillData에서 가져올 예정
        if (classID == 1001) return SkillRange.Short; // 전사
        if (classID == 1002) return SkillRange.Long;  // 마법사
        return SkillRange.Medium;
    }

    void DistributeStats(Adventurer adv, ClassData data, int totalPot, int age)
    {
        // 1. 해당 직업의 가중치 데이터 가져오기
        if (!_statWeightCache.TryGetValue(data.ID, out var jobWeights)) return;

        // 2. 목표치 및 한계치 설정
        float ageEfficiency = GetAgeEfficiency(age);
        float targetAbility = totalPot * ageEfficiency; // 나이에 따른 목표 능력치 점수 (0~200)
        int sumLimit = GetStatLimit(adv);               // 성격에 따른 스탯 총량 한계 (잠재력 돌파 가능)
        bool isLimitBroken = sumLimit > totalPot;       // 한계 돌파 여부 체크

        // 3. 기초 공사: 모든 스탯을 1로 초기화 (최소치 보장)
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            adv.SetStat(type, 1);
        }

        // 4. 가중치 추첨 준비
        // 가중치가 0인 스탯도 아주 낮은 확률(0.1)로 선택될 수 있게 보정
        Dictionary<StatType, float> lotteryWeights = jobWeights.ToDictionary(
            k => k.Key, 
            v => Mathf.Max((float)v.Value, 0.1f) 
        );

        // 5. [1차 배분] 능력치 포인트 추첨 (효율적 성장)
        // 조건: 현재 능력치가 목표에 미달 AND 스탯 총합이 한계를 넘지 않음
        while (adv.GetCurrentAbility(_statWeightCache) < targetAbility && 
               adv.Stats.Values.Sum() < sumLimit)
        {
            StatType selected = GetWeightedRandomStat(lotteryWeights);
            float currentVal = adv.GetStat(selected);

            if (currentVal < MAX_STAT_VALUE) // 상한선 20 체크
            {
                adv.SetStat(selected, currentVal + 1);
            }
            else
            {
                lotteryWeights.Remove(selected);
                if (lotteryWeights.Count == 0) break;
            }
        }

        // 6. [2차 배분] 한계 돌파 추가 성장 (노력에 의한 잠재력 초과)
        // 성격이 좋아 sumLimit이 남았다면, 효율에 상관없이 한계치까지 스탯을 더 채움
        if (isLimitBroken && adv.Stats.Values.Sum() < sumLimit)
        {
            while (adv.Stats.Values.Sum() < sumLimit)
            {
                StatType selected = GetWeightedRandomStat(lotteryWeights);
                float currentVal = adv.GetStat(selected);

                if (currentVal < MAX_STAT_VALUE)
                {
                    adv.SetStat(selected, currentVal + 1);
                }
                else
                {
                    lotteryWeights.Remove(selected);
                    if (lotteryWeights.Count == 0) break;
                }
            }
        }

        // 7. 결과 보고 (한계 돌파 시 강조 표시)
        string limitColor = isLimitBroken ? "cyan" : "white";
        Debug.Log($"<color={limitColor}><b>[{adv.Name}]</b></color> 생성 완료. " +
                  $"방어점수: {adv.GetDefenseScore(DataManager.Instance.DefenseWeight)} " + 
                  $"(잠재력: {adv.TotalPotential}) / " +
                  $"능력치: {adv.GetCurrentAbility(_statWeightCache)} / " + 
                  $"스탯총합: {adv.Stats.Values.Sum()} (한계: {sumLimit})");
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
        // 기본값: weight + 랜덤(-3 ~ +5)
        float baseVal = weight + Random.Range(-3, 6);
        return Mathf.Clamp(baseVal, 1, 20);
    }

    // 한계치 계산 -> duty, patience, ambition, honor 
    int GetStatLimit(Adventurer adv)
    {
        int limit = adv.TotalPotential;

        // Enum 키를 사용하여 안전하게 접근
        float spiritSum = 0f;
        spiritSum += GetNatureValue(adv, NatureType.Nature_Duty);
        spiritSum += GetNatureValue(adv, NatureType.Nature_Patience);
        spiritSum += GetNatureValue(adv, NatureType.Nature_Ambition);
        spiritSum += GetNatureValue(adv, NatureType.Nature_Honor);

        // 한계 돌파 로직
        if (spiritSum >= LIMIT_BREAK_HIGH_THRESHOLD) limit = Mathf.RoundToInt(limit * LIMIT_BREAK_HIGH_MULTIPLIER); // 15%
        else if (spiritSum >= LIMIT_BREAK_MID_THRESHOLD) limit = Mathf.RoundToInt(limit * LIMIT_BREAK_MID_MULTIPLIER); // 5%

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
    int RollTotalPotential() => Random.Range(25, 101) + Random.Range(25, 101);
    string GetGradeString(int pot) => (pot >= 200) ? "The One" : (pot >= 180) ? "S" : (pot >= 150) ? "A" : (pot >= 100) ? "B" : "C";
}