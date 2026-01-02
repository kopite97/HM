using System.Collections.Generic;
using UnityEngine;

public class Monster
{
    // 원본 데이터 참조 (Flyweight 패턴: 여러 고블린이 하나의 고블린 데이터를 공유)
    private MonsterData _data;
    
    public PartyPosition StartPosition { get; private set; } // 배치된 위치
    
    // 만약 '접두사(강력한)'가 붙거나 해서 이름이 바뀔 수 있다면 별도 변수로 분리 가능
    public string Name => _data.NameKR; 
    public int Level => _data.Level;
    
    // 몬스터는 성장이 없으므로 데이터의 값을 그대로 반환
    public float MaxHP => _data.MaxHP;
    public float DefenseScore => _data.DefenseScore;
    
    // 스킬 목록
    public List<int> SkillIDs => _data.SkillIDs;
    
    public IReadOnlyDictionary<StatType, int> BaseStats => _data.BaseStats;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="data">원본 몬스터 데이터</param>
    /// <param name="position">이 몬스터가 배치될 위치</param>
    public Monster(MonsterData data, PartyPosition position)
    {
        _data = data;
        StartPosition = position;
    }

    /// <summary>
    /// 특정 스탯 값을 가져옵니다.
    /// </summary>
    public int GetStat(StatType type)
    {
        return _data.BaseStats.TryGetValue(type, out int val) ? val : 0;
    }

    /// <summary>
    /// 성격 값을 가져옵니다. (AI 로직용)
    /// </summary>
    public int GetNature(NatureType type)
    {
        return _data.BaseNatures.TryGetValue(type, out int val) ? val : 0;
    }
}