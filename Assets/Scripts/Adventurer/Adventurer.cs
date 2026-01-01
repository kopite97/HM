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

    // --- 캐싱 및 더티 플래그 ---
    [SerializeField] private float _currentAbility;
    private bool _isDirtyAbility = true;

    [SerializeField] private float _cachedDefenseScore;
    [SerializeField] private AdventurerDefenseType _cachedDefenseType;
    private bool _isDirtyDefense = true;

    // --- 데이터 구조 (캡슐화) ---
    private Dictionary<StatType, int> _stats = new Dictionary<StatType, int>();
    private Dictionary<NatureType, int> _natures = new Dictionary<NatureType, int>();
    public IReadOnlyDictionary<StatType, int> Stats => _stats;
    public IReadOnlyDictionary<NatureType, int> Natures => _natures;

    public PartyPosition AssignedPosition { get; private set; } = PartyPosition.None;

    // --- 프로퍼티 (Lazy Evaluation 적용) ---
    public float DefenseScore => _cachedDefenseScore;
    public AdventurerDefenseType DefenseType => _cachedDefenseType;

    // [수정] 성장의 기준을 단순 합계가 아닌 '현재 능력치'로 변경
    public bool CanGrow => _currentAbility < TotalPotential;
    public int CurrentTotalStat => _stats.Values.Sum();

    public Adventurer(string name, int age, int jobId)
    {
        Name = name;
        Age = age;
        JobID = jobId;
    }

    // --- Setter Methods ---
    public void SetJobName(string jobName) => JobName = jobName;
    public void SetPotential(int total, string grade) { TotalPotential = total; PotentialGrade = grade; }
    public void SetPosition(PartyPosition position) => AssignedPosition = position;
    
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
}