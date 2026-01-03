using UnityEngine;

public static class EffectPipeline
{
    public static EffectContext Process(BattleUnit attacker, BattleUnit defender, LearnedSkill skill)
    {
        EffectContext ctx = new EffectContext(attacker, defender, skill);
        ctx.AddLog($"=== 🌀 상태이상 계산: {skill.Data.SkillEffect} ===");

        // 면역 체크
        CheckImmunity(ctx);
        if (ctx.IsImmune)
        {
            ctx.IsSuccess = false;
            ctx.AddLog("결과 : 면역");
            return ctx;
        }
        
        // 확률 계산
        CalculateChance(ctx);
        
        // 주사위 굴리기
        RollDice(ctx);
        
        // 성공 시 => 지속시간 및 강도 계산
        if (ctx.IsSuccess)
        {
            CalculateDuration(ctx);
            CalculatePower(ctx);
        }
        ctx.AddLog($"=== 최종 결과: {(ctx.IsSuccess ? "적중 성공" : "실패")} ===");
        return ctx;
    }

    
    private static void CheckImmunity(EffectContext ctx)
    {
        
    }

    private static void CalculateChance(EffectContext ctx)
    {
        
    }

    private static void RollDice(EffectContext ctx)
    {
        
    }

    private static void CalculateDuration(EffectContext ctx)
    {
        
    }
    
    private static void CalculatePower(EffectContext ctx)
    {
       
    }

}