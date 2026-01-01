using UnityEngine;
using System.Collections.Generic;
using System.Data;
using System.Linq;

[System.Serializable]
public class Adventurer
{
    // 기본 정보
    public string Name;
    public int Age;
    public int JobID;
    public string JobName;

    // 핵심 데이터
    public int TotalPotential;      // 잠재력 총합 (이 그릇의 크기)
    public string PotentialGrade;   // 등급 (S, A, B...)

    public Dictionary<StatType, int> Stats = new Dictionary<StatType, int>();
    public Dictionary<NatureType, int> Natures = new Dictionary<NatureType, int>();

    public float GetDefenseScore()
    {
        return 0f;
    }
    
    public int CurrentTotalStat => Stats.Values.Sum();
    
    public bool CanGrow => CurrentTotalStat < TotalPotential;

    public Adventurer(string name, int age, int jobId)
    {
        Name = name;
        Age = age;
        JobID = jobId;
    }

    public int GetStat(StatType type)
    {
        return Stats.ContainsKey(type) ? Stats[type] : 0;
    }
}