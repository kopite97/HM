using System.Text;

public class DamageContext
{
    // 초기 입력 변수
    public BattleUnitModel Attacker { get; private set; }
    public BattleUnitModel Defender { get; private set; }
    public LearnedSkill Skill { get; private set; }
    
    // 변동 값
    public DamageType DamageType { get; set; }
    public bool IsHeal { get; set; }
    public bool IsCritical { get; set; }
    public bool IsHit { get;  set; }
    
    public float BasePower  { get;  set; } // 기본 위력
    public float TotalMultiplier  { get;  set; } // 데미지 강화 계수 (치명타 피해량, 스킬 계수 , 환경 변수 등..)

    public float AppliedResistance { get;  set; } // 해당 데미지 타입에 대한 저항력 
    public float DefenseReduction { get;  set; } // 방어력으로 인한 감소율 -
    
    // 최종 결과
    public float FinalResult;
    
    // 유틸리티
    private StringBuilder _processLog;
    private bool _enableLogging;

    public DamageContext()
    {
        _processLog = new StringBuilder(1024);
    }

    // ObjectPooling용 
    public void InitializeContext(BattleUnitModel attacker, BattleUnitModel defender, LearnedSkill skill,bool enableLog = false)
    {
        Attacker = attacker;
        Defender = defender;
        Skill = skill;

        DamageType = skill.SourceSkill.DamageType;
        IsHeal = skill.IsHeal;
        IsCritical = false;
        IsHit = false;

        BasePower = 0f;
        TotalMultiplier = 1.0f;
        AppliedResistance = 0f;
        DefenseReduction = 0f;
        FinalResult = 0f;

        _enableLogging = enableLog;
        _processLog.Clear();
    }

    public void AppendLog(string message)
    {
        if (_enableLogging)
        {
            _processLog.AppendLine(message);
        }
    }

}