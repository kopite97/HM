using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct StatWeight
{
    public StatType stat;
    [Range(0f,5f)] public float multiplier; // 0~5배수
}

[CreateAssetMenu(fileName = "DefenseWeightData", menuName = "Game/Combat/DefenseWeightData")]
public class DefenseWeightData : ScriptableObject
{
    [Header("방어 점수 가중치")] 
    public List<StatWeight> PhysicalDefenseWeights; 
    public List<StatWeight> EvasionDefenseWeights;
    public List<StatWeight> MagicalDefenseWeights;
    public List<StatWeight> DivineDefenseWeights;

    public float CalculateScore(Dictionary<StatType, int> stats, List<StatWeight> weights)
    {
        float totalScroe = 0f;

        foreach (var w in weights)
        {
            if (stats.ContainsKey(w.stat))
            {
                totalScroe += stats[w.stat] * w.multiplier;
            }
        }

        return totalScroe;
    }
}