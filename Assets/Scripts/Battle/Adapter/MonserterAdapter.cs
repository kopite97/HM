using System.Collections.Generic;

public class MonsterAdapter : IBattleEntity
{
    private Monster _mon;

    public MonsterAdapter(Monster mon)
    {
        _mon = mon;
    }

    public string Name => _mon.Name;
    public int Level => _mon.Level;
    public PartyPosition Position => _mon.StartPosition;
    
    /**
     *
     * 몬스터들은 별다른 과정 없이 스탯 그대로 (일단은)
     */
    
    
    public float GetMaxHp()
    {
        return _mon.MaxHP;
    }

    public float GetBaseStat(StatType type)
    {
        return _mon.BaseStats.TryGetValue(type, out int val) ? val : 0;
    }

    public float GetNatureStat(NatureType type)
    {
        return _mon.GetNature(type);
    }

    public float GetDefenseScore()
    {
        return _mon.DefenseScore;
    }

    public List<int> GetSkillIDs()
    {
        return _mon.SkillIDs;
    }
}