using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class DamagePipeline
{
    // 밸런스 상수 TODO -> 나중에 별도 설정 파일로 빼기
    private const float DEFENSE_CONSTANT = 100f; // 점감 공식 상수 (높을수록 방어 효율 낮아짐)
    private const float BASE_HIT_CHANCE = 0.95f; // 기본 명중률 95%
    private const float MIN_HIT_CHANCE = 0.20f;  // 최소 명중률 20%
    private const float BASE_CRIT_CHANCE = 0.05f;// 기본 치명타율 5%
    private const float CRIT_MULTIPLIER = 1.5f;  // 치명타 배율
    
    public static DamageContext Process(DamageContext ctx)
    {
        ctx.AddLog($"⚔️ [계산 시작] {ctx.Attacker.Name} ➜ {ctx.Defender.Name} ({ctx.Skill._sourceSkill.Name})");
        // 파이프라인 단계별 실행
       
        // 빗나감 체크
        CheckHit(ctx);
        if (!ctx.IsHit)
        {
            ctx.FinalResult = 0;
            ctx.AddLog("결과 : 빗나감 (Miss)");
            return ctx;
        }
        
        // 스탯 기반 데미지 합산
        CalculateBasePotency(ctx);
        

        ApplyEnvironment(ctx);
        if (ctx.IsHeal)
        {
            ApplyHeal(ctx);
        }
        else
        {
            CheckCritical(ctx);
            ApplyDefense(ctx);
            ApplyResistance(ctx);
        }

        ApplyVariance(ctx);
        return ctx;
    }

    private static void CheckHit(DamageContext ctx)
    {
        // 필중 or  힐은 무조건 맞춤
        if (ctx.IsHeal || ctx.HasProperty(SkillModifier.GuaranteedHit))
        {
            ctx.IsHit = true;
            return;
        }
        
        // 명중 체크 : 기본 + (집중력 - 기동성) * 보정치
        float acc = ctx.Attacker.GetStat(StatType.Mind_Focus);
        float eva = ctx.Defender.GetStat(StatType.Body_Mobility);

        float hitChance = BASE_HIT_CHANCE + (acc - eva) * 0.05f;
        hitChance = Mathf.Clamp(hitChance, MIN_HIT_CHANCE, 1.0f);

        if (Random.value > hitChance)
        {
            ctx.IsHit = false;
            ctx.AddLog($"🎲 명중 실패 (확률: {hitChance:P0})");
        }
        else
        {
            ctx.IsHit = true;
        }
    }
            
    // 기초 위력 계산
    private static void CalculateBasePotency(DamageContext ctx)
    {
        float potency = 0f;
        float[] coefs = ctx.Skill.GetCurrentPowerCoefs();

        ctx.AddLog($"[1. 기초 위력]");
        for (int i = 0; i < coefs.Length; i++)
        {
            StatType stat = ctx.Skill._sourceSkill.BaseStats[i];
            float val = ctx.Attacker.GetStat(stat);
            float added = val * coefs[i];
            
            potency += added;
            ctx.AddLog($" - {stat}({val}) x {coefs[i]:F2} = {added:F1}");
        }

        ctx.BasePotency = potency;
        ctx.AddLog($" > 합계: {ctx.BasePotency:F1}");
    }
    
    
    // 환경 변수 보정
    private static void ApplyEnvironment(DamageContext ctx)
    {
        if (ctx.Env == null) return;
        float envMult = 1.0f;

        if (ctx.IsHeal && ctx.Env.ContainsTag(DungeonTag.HolyGround))
        {
            envMult += 0.3f;
            ctx.AddLog(" - 환경(성역): 회복량 30% 증가");
        }

        ctx.TotalMultiplier *= envMult;
        ctx.AddLog($"[환경] 보정값: x{envMult:F2}");
    }
    
    private static void CheckCritical(DamageContext ctx)
    {
        if (ctx.HasProperty(SkillModifier.GuaranteedCrit))
        {
            ctx.IsCritical = true;
        }
        else
        {
            // 치명타 여부 : 기교 vs 통찰
            float subtlety = ctx.Attacker.GetStat(StatType.Tech_Subtlety);
            float insight = ctx.Defender.GetStat(StatType.Mind_Insight);

            float chance = BASE_CRIT_CHANCE + (subtlety - insight) * 0.05f;
            ctx.IsCritical = Random.value <= chance;
        }

        if (ctx.IsCritical)
        {
            ctx.TotalMultiplier *= CRIT_MULTIPLIER;
            ctx.AddLog($"💥 치명타! (x{CRIT_MULTIPLIER})");
        }
    }

    private static void ApplyDefense(DamageContext ctx)
    {
        if (ctx.IgnoreDefense)
        {
            ctx.AddLog("⚡ 방어 무시 (Penetration)");
            return;
        }

        float defense = ctx.Defender.GetTotalDefense();

        float reduction = DEFENSE_CONSTANT / (DEFENSE_CONSTANT + defense);

        ctx.DefenseReduction = 1.0f - reduction;
        ctx.TotalMultiplier *= reduction;
        
        ctx.AddLog($"🛡️ 방어력({defense}): {ctx.DefenseReduction:P1} 경감됨");
    }

    private static void ApplyResistance(DamageContext ctx)
    {
        DamageType damageType = ctx.Skill._sourceSkill.DamageType;

        // 트루 데미지 / 데미지 종류가 아니면 저항 불가
        if (damageType.HasFlag(DamageType.True) || damageType.HasFlag(DamageType.None))
        {
            return; 
        }

        float totalResistance = 0f;
        int typeCount = 0;

        foreach (DamageType checkType in Enum.GetValues(typeof(DamageType)))
        {
            if (checkType == DamageType.True || checkType == DamageType.None) continue;
            if (((int)checkType & ((int)checkType - 1)) != 0) continue;
            if (damageType.HasFlag(checkType))
            {
                totalResistance += ResistanceManager.Instance.CalculateSingleResistance(ctx.Defender, checkType);
                typeCount++;
            }
        }

        if (typeCount != 0)
        {
            // 복합 속성일 경우 평균 저항력 적용
            float finalResistance = totalResistance / typeCount;
            
            // 최대 저항 75% 제한
            finalResistance = Mathf.Clamp(finalResistance, 0f, 75f);
            
            float resistMult = 1.0f - (finalResistance * 0.01f);
            ctx.TotalMultiplier *= resistMult;
            ctx.AppliedResistance = finalResistance;
            
            ctx.AddLog($"💧 속성 저항({finalResistance:F1}%): 데미지 감소");
        }
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
        ctx.FinalResult = ctx.BasePotency * ctx.TotalMultiplier;

        if (!ctx.IsHeal && ctx.IsHit && ctx.FinalResult < 1)
        {
            ctx.FinalResult = 1;
        }
        ctx.AddLog($"🏁 최종 결과: {ctx.FinalResult}");
    }
}