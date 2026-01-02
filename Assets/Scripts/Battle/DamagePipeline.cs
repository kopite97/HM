using UnityEngine;

public static class DamagePipeline
{
    public static DamageContext Process(BattleUnit attacker, BattleUnit defender, LearnedSkill skill,
        BattleEnvironment env)
    {
     
        // 컨텍스트 생성
        var ctx = new DamageContext(attacker, defender, skill, env);
        ctx.AddLog($"=== ⚔️ 계산 시작: {attacker.Name} -> {defender.Name} ({skill.Data.NameKR}) ===");
        
        // 파이프라인 단계별 실행
        CalculateBasePotency(ctx);

        if (ctx.IsHeal)
        {
            ApplyEnvironment(ctx);
            ApplyHeal(ctx);
        }
        else
        {
            ApplyPositionLogic(ctx);
            ApplyEnvironment(ctx);
            CalculateDefense(ctx);
            // TODO 치명타 로직 구현하기
        }
        
        ApplyVariance(ctx);
        
        // 최종 결과 확정
        ctx.FinalResult = Mathf.Max(0, Mathf.Round(ctx.BasePotency * ctx.TotalMultiplier));
        ctx.AddLog($"=== 🏁 최종 결과: {ctx.FinalResult} {(ctx.IsHeal ? "회복" : "피해")} ===");

        return ctx;
    }
    
    // 기초 위력 계산
    private static void CalculateBasePotency(DamageContext ctx)
    {
        float potency = 0f;
        float[] coefs = ctx.Skill.GetCurrentPowerCoefs();

        ctx.AddLog($"[1. 기초 위력]");
        for (int i = 0; i < coefs.Length; i++)
        {
            StatType stat = ctx.Skill.Data.Base_Stats[i];
            float val = ctx.Attacker.GetStat(stat);
            float added = val * coefs[i];
            
            potency += added;
            ctx.AddLog($" - {stat}({val}) x {coefs[i]:F2} = {added:F1}");
        }

        ctx.BasePotency = potency;
        ctx.AddLog($" > 합계: {ctx.BasePotency:F1}");
    }

    // 포지션/사거리 보정
    private static void ApplyPositionLogic(DamageContext ctx)
    {
        float multiplier = 1.0f;
        SkillRange range = ctx.Skill.Data.Range;
        PartyPosition attPos = ctx.Attacker.CurrentPosition;
        PartyPosition defPos = ctx.Defender.CurrentPosition;

        // 공격자 위치 페널티
        if (range == SkillRange.Short)
        {
            if (attPos == PartyPosition.Midguard) multiplier = 0.8f;
            else if (attPos == PartyPosition.Rearguard) multiplier = 0f;
        }
        else if (range == SkillRange.Medium)
        {
            if (attPos == PartyPosition.Rearguard) multiplier = 0.8f;
        }
        
        // 거리 페널티
        // TODO 살짝 수정 필요.. (Defender Party의 전위,후위가 모두 죽었다면 페널티 제거)
        if (range == SkillRange.Short && defPos == PartyPosition.Rearguard)
        {
            multiplier *= 0.5f; // 너무 멈
            ctx.AddLog(" - 거리 페널티 (Short -> Rear): 50% 감소");
        }


        ctx.TotalMultiplier *=multiplier;
        ctx.AddLog($"[2. 포지션] 보정값: x{multiplier:F2}");
        
    }
    
    // 환경 변수 보정
    private static void ApplyEnvironment(DamageContext ctx)
    {
        if (ctx.Env == null) return;
        float envMult = 1.0f;

        if (ctx.Env.ContainsTag(DungeonTag.Darkness) && ctx.Skill.Data.Range == SkillRange.Long)
        {
            envMult -= 0.2f;
            ctx.AddLog(" - 환경(어둠): 원거리 효율 20% 감소");
        }

        if (ctx.IsHeal && ctx.Env.ContainsTag(DungeonTag.HolyGround))
        {
            envMult += 0.3f;
            ctx.AddLog(" - 환경(성역): 회복량 30% 증가");
        }

        ctx.TotalMultiplier *= envMult;
        ctx.AddLog($"[환경] 보정값: x{envMult:F2}");
    }
    
    // 방어력 적용
    private static void CalculateDefense(DamageContext ctx)
    {
        // 방어자가 해당 속성에 대한 저항률 계산
        float resistance = ctx.Defender.GetResistance(ctx.CurrentDamageType);
        
        // 뎀감 공식 적용 (100 / (100+방어력))
        // -> 방어력이 0이면 1.0, 100이면 0.5 
        float reduction = 100f / (100f + Mathf.Max(0f, resistance));

        ctx.AppliedResistance = resistance;
        ctx.DefenseReduction = reduction;

        ctx.TotalMultiplier *= reduction;
        ctx.AddLog($"[3. 방어]");
        ctx.AddLog($" - 속성: {ctx.CurrentDamageType}, 저항합계: {resistance}");
        ctx.AddLog($" - 데미지 비율: {reduction:P0} (x{reduction:F2})");

    }
    
    // 힐 보너스 (회복 전용) => 나중에 피흡도?
    private static void ApplyHeal(DamageContext ctx)
    {
        // 시전자의 faith스탯 1당 힐량 2%증가
        float faith = ctx.Attacker.GetStat(StatType.Tech_Faith);
        float bonus = 1.0f + (faith * 0.2f); // 0.1% 씩
        if (bonus > 1.0f)
        {
            ctx.TotalMultiplier *= bonus;
            ctx.AddLog($"[치유 보너스] 신앙({faith}) 보정: x{bonus:F2}");
        }
    }
    
    // 랜덤 분산
    private static void ApplyVariance(DamageContext ctx)
    {
        float variance = Random.Range(0.95f, 1.05f);
        ctx.TotalMultiplier *= variance;
    }
}