using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Adventurer
{
    // --- 기본 정보 ---
    public string Name { get; private set; }
    public int Age { get; private set; }
    public int JobID { get; private set; }
    public string JobName { get; private set; }
    public int TotalPotential { get; private set; }
    public string PotentialGrade { get; private set; }

    [SerializeField] 
    private List<LearnedSkill> _skills =  new List<LearnedSkill>();
    public IReadOnlyList<LearnedSkill> Skills => _skills;
    
    // --- 캐싱 및 더티 플래그 ---
    [SerializeField] private float _currentAbility;
    private bool _isDirtyAbility = true;

    [SerializeField] private float _cachedDefenseScore;
    [SerializeField] private AdventurerDefenseType _cachedDefenseType;
    private bool _isDirtyDefense = true;
    
    private Dictionary<StatType, int> _stats = new Dictionary<StatType, int>();
    private Dictionary<NatureType, int> _natures = new Dictionary<NatureType, int>();
    public IReadOnlyDictionary<StatType, int> Stats => _stats;
    public IReadOnlyDictionary<NatureType, int> Natures => _natures;

    public PartyPosition PreferredPosition { get; private set; } = PartyPosition.None;
    public PartyPosition AssignedPosition { get; private set; } = PartyPosition.None;

    // --- 프로퍼티 (Lazy Evaluation 적용) ---
    public float DefenseScore => _cachedDefenseScore;
    public AdventurerDefenseType DefenseType => _cachedDefenseType;

    // [수정] 성장의 기준을 단순 합계가 아닌 '현재 능력치'로 변경
    public bool CanGrow => _currentAbility < TotalPotential;
    public int CurrentTotalStat => _stats.Values.Sum();
    
    // 포지션 잠금
    public bool IsPositionLocked { get; private set; } = false;

    public Adventurer(string name, int age, int jobId)
    {
        Name = name;
        Age = age;
        JobID = jobId;
    }

    // --- Setter Methods ---
    public void SetJobName(string jobName) => JobName = jobName;
    public void SetPotential(int total, string grade) { TotalPotential = total; PotentialGrade = grade; }
    public void SetPosition(PartyPosition position)
    {
        if(!IsPositionLocked)
            AssignedPosition = position;
    }
    
    public void SetNature(NatureType type, int value) => _natures[type] = value;

    public void SetStat(StatType type, int value)
    {
        if (!_stats.ContainsKey(type)) _stats.Add(type, 0);
        
        if (_stats[type] != value)
        {
            _stats[type] = value;
            _isDirtyDefense = true;
            _isDirtyAbility = true;
        }
    }

    public void SetPreferredPosition(PartyPosition preferred)
    {
        PreferredPosition = preferred;
    }

    /**
     * 유저가 직접 포지션을 지정할 때 호출
     */
    public void SetManualPosition(PartyPosition pos)
    {
        AssignedPosition = pos;
        IsPositionLocked = true;
    }

    /**
     * 자동 배정 시스템으로 복귀할 때 호출
     */
    public void UnlockPosition()
    {
        IsPositionLocked = false;
    }

    
    // --- 로직 메서드 ---
    public int GetStat(StatType type) => _stats.TryGetValue(type, out int val) ? val : 0;

    public float GetCurrentAbility(Dictionary<int, Dictionary<StatType, int>> weightCache)
    {
        if (_isDirtyAbility) UpdateCurrentAbility(weightCache);
        return _currentAbility;
    }

    private void UpdateCurrentAbility(Dictionary<int, Dictionary<StatType, int>> weightCache)
    {
        if (weightCache == null || !weightCache.TryGetValue(this.JobID, out var jobWeights))
        {
            _currentAbility = 0; return;
        }

        float weightedSum = 0;
        float totalWeight = 0;

        foreach (var kvp in jobWeights)
        {
            weightedSum += GetStat(kvp.Key) * kvp.Value;
            totalWeight += kvp.Value;
        }

        float score = (totalWeight > 0) ? (weightedSum / totalWeight) * 10f : 0f;
        _currentAbility = Mathf.Round(score * 10f) / 10f;
        _isDirtyAbility = false;
    }

    public AdventurerDefenseType GetDefenseStyle(DefenseWeightData weightData)
    {
        if (_isDirtyDefense) UpdateDefenseStats(weightData);
        return _cachedDefenseType;
    }

    public float GetDefenseScore(DefenseWeightData weightData)
    {
        if (_isDirtyDefense) UpdateDefenseStats(weightData);
        return _cachedDefenseScore;
    }

    private void UpdateDefenseStats(DefenseWeightData weightData)
    {
        if (weightData == null) { Debug.LogError("Defense weight data is null"); return; }

        float scoreA = weightData.CalculateScore(_stats, weightData.PhysicalDefenseWeights);
        float scoreB = weightData.CalculateScore(_stats, weightData.EvasionDefenseWeights);
        float scoreC = weightData.CalculateScore(_stats, weightData.MagicalDefenseWeights);
        float scoreD = weightData.CalculateScore(_stats, weightData.DivineDefenseWeights); // [수정] 오타 scroeD -> scoreD

        float maxScore = Mathf.Max(scoreA, Mathf.Max(scoreB, Mathf.Max(scoreC, scoreD)));
        _cachedDefenseScore = maxScore;
        
        if (maxScore == scoreA) _cachedDefenseType = AdventurerDefenseType.Physical;
        else if (maxScore == scoreB) _cachedDefenseType = AdventurerDefenseType.Evasion;
        else if (maxScore == scoreC) _cachedDefenseType = AdventurerDefenseType.Magical;
        else _cachedDefenseType = AdventurerDefenseType.Divine;

        _isDirtyDefense = false;
    }

    /**
    public void CalculatePreferredPosition(SkillRange mainSkillRange, DefenseWeightData defenseData)
    {
        float vanguardThreshold = PartyManager.Instance.GetVanguardThreshold();
        float defense = GetDefenseScore(defenseData);

        switch (mainSkillRange)
        {
            case SkillRange.Short:
                PreferredPosition = (defense >= vanguardThreshold) ? PartyPosition.Vanguard : PartyPosition.Midguard;
                break;
            case SkillRange.Medium:
                PreferredPosition = (defense >= vanguardThreshold) ? PartyPosition.Vanguard : PartyPosition.Midguard;
                break;
            case SkillRange.Long:
                PreferredPosition = (defense >= vanguardThreshold) ? PartyPosition.All : PartyPosition.Rearguard;
                break;
        }
    }**/

    public void LearnSkill(SkillData data, int level = 1)
    {
        if (_skills.Exists(s => s.Data.ID == data.ID)) return;
        _skills.Add(new LearnedSkill(data, level));
    }
    
    public void AnalyzePreferredPosition(DefenseWeightData defenseData)
    {
        if (_skills.Count == 0) return;

        float defScore = GetDefenseScore(defenseData);
    
        // 1. 스킬셋 사거리 점수 계산 (가중치 적용)
        float totalRangeScore = 0;
        float totalWeight = 0;

        foreach (var skill in _skills)
        {
            // 액티브 스킬이 패시브보다 포지션 결정에 더 큰 영향을 미침 (가중치 2배)
            float typeWeight = (skill.Data.Type == SkillType.ACTIVE) ? 2.0f : 1.0f;
        
            // 사거리별 기본 점수 (Short일수록 높은 점수 부여)
            float rangeBase = (skill.Data.Range == SkillRange.Short) ? 10f : 
                (skill.Data.Range == SkillRange.Medium) ? 5f : 0f;

            // 공식: (사거리 점수 * 스킬 레벨 * 타입 가중치)
            totalRangeScore += rangeBase * skill.Level * typeWeight;
            totalWeight += skill.Level * typeWeight;
        }

        float finalRangeScore = totalRangeScore / totalWeight;

        // 2. 사거리 점수 + 방어 점수 하이브리드 판정
        // finalRangeScore가 높을수록(근접 스킬 위주일수록) 앞쪽을 선호
        if (finalRangeScore >= 7.5f) // 근접 위주
        {
            PreferredPosition = (defScore >= 145f) ? PartyPosition.Vanguard : PartyPosition.Midguard;
        }
        else if (finalRangeScore >= 3.5f) // 중거리 위주
        {
            PreferredPosition = (defScore >= 165f) ? PartyPosition.Vanguard : PartyPosition.Midguard;
        }
        else // 원거리 위주
        {
            PreferredPosition = (defScore >= 180f) ? PartyPosition.All : PartyPosition.Rearguard;
        }
    }
}