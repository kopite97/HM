using System;
using System.Collections.Generic;

public static class SkillModifierSystem
{
    private static readonly Dictionary<SkillModifier,Action<DamageContext,float>> _handlers 
        = new Dictionary<SkillModifier,Action<DamageContext,float>>();

    static SkillModifierSystem()
    {
        // 수치 관련 로직 등록
        
    }

    public static void Process(DamageContext ctx)
    {
        if (!ctx.Skill.HasModifiers) return;

        LearnedSkill skill = ctx.Skill;
        for (int i = 0; i < skill.Modifiers.Length; i++)
        {
            if (_handlers.TryGetValue(skill.Modifiers[i], out var handler))
            {
                handler(ctx, skill.ModifiersValue[i]);
            }
        }
    }
}