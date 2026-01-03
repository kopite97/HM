using UnityEngine;
using System.Text;

/// <summary>
/// 데미지 계산의 모든 Context를 저장하는 컨테이너
/// </summary>
public class DamageContext
{
    
    // 초기 입력 변수 
    public BattleUnit Attacker { get; private set; }
    public BattleUnit Defender { get; private set; }
    public LearnedSkill Skill { get; private set; }
    public BattleEnvironment Env { get; private set; }
    
    // 계산 중 변동되는 값 
    public DamageType CurrentDamageType;
    public bool IsHeal;
    public bool IsCritical;

    public float BasePotency; // 기초 위력 (스탯 합산)
    public float TotalMultiplier; // 최종 승수 (1.0 = 100%)

    public float AppliedResistance; // 적용된 저항력 수치
    public float DefenseReduction; // 방어력에 의한 감소율 (0.0~1.0)
    
    // 최종 결과 
    public float FinalResult;
    
    // 계산 과정 기록
    public StringBuilder ProcessLog = new StringBuilder();

    public DamageContext(BattleUnit att, BattleUnit def, LearnedSkill skill, BattleEnvironment env)
    {
        Attacker = att;
        Defender = def;
        Skill = skill;
        Env = env;

        CurrentDamageType = skill.Data.Damage_Type;
        //IsHeal = (skill.Data.SkillEffect == SkillEffect.HEAL);
        TotalMultiplier = 1.0f;
    }
    
    /// <summary>
    /// 로그 추가
    /// </summary>
    /// <param name="message"></param>
    public void AddLog(string message)
    {
        ProcessLog.AppendLine(message);
    }
        
}