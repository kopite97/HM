using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// TODO 방어력 관통 수치 넣기

public class BattleUnitModel
{
    public static readonly int StatCount = System.Enum.GetNames(typeof(StatType)).Length;
    public static readonly int NatureCount = System.Enum.GetNames(typeof(NatureType)).Length;
    
    private readonly float[] _statsArray = new float[StatCount];
    private readonly float[] _natureArray = new float[NatureCount];
    
    private IBattleEntity _sourceEntity;

    public int InstanceID { get; private set; }
    public string Name { get; private set; }
    public int AffiliationID { get; private set; }

    public float MaxHP { get; private set; }
    public float CurrentHP { get;  private set; }
    public float MaxMP { get; private set; }
    public float CurrentMP { get; private set; }
    public float MaxSP { get; private set; }
    public float CurrentSP { get; private set; }
    public float MaxDP { get; private set; }
    public float CurrentDP { get; private set; }
    
    // 위치 및 상태
    public Vector2 Position { get; private set; }
    public UnitState State { get; private set; }
    public bool IsDead { get; private set; }
    
    // 전투 변수 
    public BattleUnitModel Target { get; private set; }
    public ActionCandiate NextAction { get; private set; }
    
    // 타이머
    public float RecoveryTimer  { get; private set; }
    public float ThinkTimer    { get; private set; }
    
    // 보유 스킬 목록
    private List<LearnedSkill> _skills =  new  List<LearnedSkill>();
    public IReadOnlyList<LearnedSkill> Skills => _skills;
    
    public float GetStat(StatType stat) => _statsArray[(int)stat];
    public float GetNature(NatureType nature) => _natureArray[(int)nature];
    
    public BattleUnitModel(IBattleEntity entity)
    {
        _sourceEntity = entity;
        InstanceID = _sourceEntity.InstanceID;
        Name = _sourceEntity.Name;
        AffiliationID = _sourceEntity.AffiliationID;
        MaxHP = _sourceEntity.GetMaxHp();
        MaxMP = _sourceEntity.GetMaxMP();
        MaxSP = _sourceEntity.GetMaxSP();
        MaxDP = _sourceEntity.GetMaxDP();

        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
        CurrentSP = MaxSP;
        CurrentDP = MaxDP;

        IsDead = false;
        Target = null;
        
        _skills = _sourceEntity.GetSkills();

        RecoveryTimer = _sourceEntity.GetRecoveryTimer();
        ThinkTimer = _sourceEntity.GetThinkTimer();

        foreach (var kvp in _sourceEntity.GetStats())
        {
            _statsArray[(int)kvp.Key] = kvp.Value;
        }

        foreach (var kvp in _sourceEntity.GetNatures())
        {
            _natureArray[(int)kvp.Key] = kvp.Value;
        }
    }
}

public readonly struct ActionCandiate
{
    public readonly LearnedSkill skill;
    public readonly BattleUnitModel Target;
    public readonly float score;
}

