using UnityEngine;

public static class BattleCalculator
{
    // 데미지 계산 파이프라인
    public static float CalculateDamage(BattleUnit attacker, BattleUnit defender, LearnedSkill skill, BattleEnvironment env)
    {
        // 0. 유효성 검사 (0데미지 처리)
        if (attacker == null || defender == null || skill == null) return 0f;

        // ----------------------------------------------------
        // 1. 기초 위력 (Base Potency)
        // ----------------------------------------------------
        float baseDamage = 0f;
        float[] coefs = skill.GetCurrentPowerCoefs(); // 레벨 성장 반영된 계수 

        for (int i = 0; i < coefs.Length; i++)
        {
            // 스킬이 참조하는 스탯(예: 힘, 지능)을 가져옴
            StatType statType = skill.Data.Base_Stats[i];
            
            // BattleUnit의 GetStat은 버프/디버프가 이미 적용된 값을 반환한다고 가정
            float statValue = attacker.GetStat(statType);
            
            baseDamage += statValue * coefs[i];
        }
        
        // ----------------------------------------------------
        // 2. 사거리 및 포지션 보정 (Position Multiplier)
        // ----------------------------------------------------
        // 공격자의 위치 페널티 + 방어자와의 거리 페널티 통합 계산
        float positionMult = GetPositionMultiplier(skill.Data.Range, attacker.CurrentPosition, defender.CurrentPosition);
        
        // 만약 포지션상 공격 불가능(0.0)이라면 즉시 0 반환
        if (positionMult <= 0f) return 0f;

        // ----------------------------------------------------
        // 3. 환경 변수 보정 (Environment Modifier)
        // ----------------------------------------------------
        float envMult = GetEnvironmentMultiplier(skill, env);
        
        // ----------------------------------------------------
        // 4. 방어력 감쇄 (Defense Reduction)
        // ----------------------------------------------------
        // 공식 : 데미지 * (100 / (100 + 방어점수))
        float defScore = defender.GetTotalDefense();
        
        // 방어력이 -100 이하로 떨어져서 분모가 0이 되는 것을 방지
        float defenseMult = 100f / (100f + Mathf.Max(0f, defScore));


        // ----------------------------------------------------
        // 5. 최종 계산 (Final Calculation)
        // ----------------------------------------------------
        float finalDamage = baseDamage * positionMult * envMult * defenseMult;

        // 크리티컬 로직이 있다면 여기서 적용
        // finalDamage = ApplyCritical(finalDamage, attacker);

        // 최소 데미지 1 보장 및 반올림
        return Mathf.Max(1f, Mathf.Round(finalDamage));
    }

    /// <summary>
    /// 포지션에 따른 효율을 계산합니다. (공격자 위치 효율 * 타겟 거리 효율)
    /// </summary>
    private static float GetPositionMultiplier(SkillRange range, PartyPosition attPos, PartyPosition defPos)
    {
        float efficiency = 1.0f;

        // A. 공격자의 위치에 따른 효율 (내 자리가 공격하기 좋은 자리인가?)
        switch (range)
        {
            case SkillRange.Short:
                // 전열: 100%, 중열: 80%, 후열: 0% (공격 불가)
                if (attPos == PartyPosition.Midguard) efficiency = 0.8f;
                else if (attPos == PartyPosition.Rearguard) return 0f; 
                break;

            case SkillRange.Medium:
                // 전열/중열: 100%, 후열: 80%
                if (attPos == PartyPosition.Rearguard) efficiency = 0.8f;
                break;

            case SkillRange.Long:
                // 모든 위치 100% (패널티 없음)
                break;
        }

        // B. 방어자 위치(거리)에 따른 추가 페널티 (너무 먼 적을 때리는가?)
        // 예: Short 스킬로 후열(Rear)을 때리려고 하면 데미지 반감
        if (range == SkillRange.Short && defPos == PartyPosition.Rearguard)
        {
            efficiency *= 0.5f; // 너무 멀어서 위력 감소
        }

        return efficiency;
    }

    /// <summary>
    /// 던전 태그나 날씨 등 환경 요소를 처리합니다.
    /// </summary>
    private static float GetEnvironmentMultiplier(LearnedSkill skill, BattleEnvironment env)
    {
        float multiplier = 1.0f;
        if (env == null) return multiplier;

        // 예시 1: '어둠(Darkness)' 태그가 있으면 원거리 명중/데미지 감소
        if (env.ContainsTag(DungeonTag.Darkness) && skill.Data.Range == SkillRange.Long)
        {
            multiplier -= 0.2f; // 20% 감소
        }

        // 예시 2: '폭우(HeavyRain)' 날씨 강도가 높으면 화염 스킬 약화
        // (SkillData에 속성(Element) 필드가 있다고 가정)
        /*
        if (env.WeatherIntensity > 0.5f && skill.Data.Element == ElementType.Fire)
        {
            multiplier -= (0.1f * env.WeatherIntensity);
        }
        */

        return Mathf.Max(0.1f, multiplier); // 최소 10% 효율 보장
    }
}