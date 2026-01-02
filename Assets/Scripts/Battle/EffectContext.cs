using UnityEngine;
using System.Text;

public class EffectContext
{
    // 입력 변수 
    public BattleUnit Attacker;
    public BattleUnit Defender;
    public LearnedSkill Skill;
    public EffectTag EffectType; // 적용하려는 효과 타입
    public float BaseChance; // 기본 성공 확률 (0.0 ~ 1.0)
    
    // 계산 변수 
    public float HitBonus; // 적중 보정치
    public float ResistanceVal; // 저항 수치
    public float FinalChance; // 최종 확률
    public bool IsImmune; // 면역 여부 

    public int Duration; // 지속 시간 (턴 수)
    public float Power; // 강도
    
    // 결과 
    public bool IsSuccess;
    
    // Log
    public StringBuilder ProcessLog = new StringBuilder();

    public EffectContext(BattleUnit attacker, BattleUnit defender, LearnedSkill skill)
    {
        Attacker = attacker;
        Defender = defender;
        Skill = skill;
        EffectType = skill.Data.Effect_Tag;

        BaseChance = skill.CurrentEffectValue / 100f;
        // 일단 지속시간은 1턴
        // TODO : coef value를 통한 턴 수 증가 로직 추가 
        Duration = 1;
        Power = 0f;
    }

    public void AddLog(string massage)
    {
        ProcessLog.AppendLine(massage);
    }

}