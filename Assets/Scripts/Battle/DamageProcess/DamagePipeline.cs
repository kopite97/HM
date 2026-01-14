public class DamagePipeline
{

    public void Process(DamageContext ctx)
    {
        // 명중 판정
        DamageCalculator.CheckIsHit(ctx);
        
        // 기본 위력 계산
        DamageCalculator.CalculateBaseDamage(ctx);
        
        // Modifier적용 
        SkillModifierSystem.Process(ctx);
        
        // 수치 계산 [Modifer 적용]
        DamageCalculator.CheckCritical(ctx);
        DamageCalculator.ApplyDefense(ctx);
        DamageCalculator.ApplyResistance(ctx);
        
        // 즉발 행동 수행
        SkillActionSystem.Process(ctx);
        
        // 지속 효과 판정
        StatusEffectSystem.Process(ctx);
        
        // 최종 마무리 
    }
}