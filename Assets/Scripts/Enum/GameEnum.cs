// 전투 스탯 (ClassData의 W_Might -> Might)

using System;

public enum StatType
{
    None,
    // [Body]
    Body_Might, Body_Endurance, Body_Reflex, Body_Poise, Body_Mobility, Body_Vitality,
    // [Mind]
    Mind_Valor, Mind_Composure, Mind_Focus, Mind_Judgment, Mind_Resolve, Mind_Insight, Mind_Awareness, Mind_Command,
    // [Tech]
    Tech_Arms, Tech_Archery, Tech_Sorcery, Tech_Faith, Tech_Subtlety, Tech_Guarding
}

// 성격 (ClassData의 W_Nature_Duty -> Duty)
public enum NatureType
{
    None,
    Nature_Duty, Nature_Discord, Nature_Patience, Nature_Ambition, Nature_Greed, 
    Nature_Cunning, Nature_Arrogance, Nature_Stubborn, Nature_Honor, Nature_Loyalty
}


public enum MonsterAttackType
{
    Normal,             // [추가] 일반 공격 (단순 물리 피해, 저레벨용)
    PhysicalBreak,      // 1. 육체 파괴 (전열 우선, 강인함/맷집 체크) 
    MentalPollution,    // 2. 정신 오염 (파티 전체, 용맹/집념 체크) 
    TacticalDisruption, // 3. 전술 교란 (진형 붕괴 유도, 통솔/기동 체크) 
    MagicBombardment,   // 4. 마법 폭격 (광역 마법 피해, 반사/신앙 체크) 
    Assassination,      // 5. 암살/저격 (후열 힐러/법사 우선, 전장감각/예지 체크) 
    Charm               // 6. 유혹/현혹 (배신/팀킬 유도, 의리/직업윤리 체크)
}

public enum MonsterDefenseType
{
    Normal,     // [추가] 일반형 (밸런스형, 특별한 약점/강점 없음)
    HeavyArmor, // 1. 중갑형 (물리 데미지 반감, 강한 공격력이나 마법 필수) 
    Ethereal,   // 2. 영체형 (물리 공격 회피, 신앙/마법 필수) 
    Evasive     // 3. 회피형 (높은 회피율, 반사/사격 수치 중요) 
}

public enum PartyPosition
{
    None, // 미배정
    Vanguard, // 전위
    Midguard, // 중위
    Rearguard, // 후위 
    All
}

public enum AdventurerDefenseType
{
    Physical,
    Evasion,
    Magical,
    Divine
}

public enum SkillRange
{
    Short,
    Medium,
    Long
}

public enum SkillType
{
    ACTIVE,
    PASSIVE
}

public enum SkillTarget
{
    ENEMY_SINGLE,
    ENEMY_AREA,
    ENEMY_ALL,
    ENEMY_RANDOM,
    
    ALLY_SINGLE,
    ALLY_AREA,
    ALLY_ALL,
    ALLY_RANDOM,
    
    SELF
}

public enum SkillResourceType
{
    NONE,
    HP,
    SP,
    MP,
    DP
}

/// <summary>
/// 즉발 효과
/// </summary>
public enum SkillProperty
{
    NONE,
    
    // [계산 보정] - DamagePipeline에서 처리
    HEAL,             // 데미지 대신 회복 적용
    IGNORE_DEF,       // 방어 공식 생략 (True Damage)
    ALWAYS_CRIT,      // 치명타 확률 100%로 고정
    ALWAYS_HIT,       // 명중률 100% (회피 불가)
    EXECUTE,          // 체력이 낮으면 데미지 2배 (처형)
    
    
    // [즉발 행동] 
    DRAIN_HP,         // 데미지의 N% 만큼 시전자 회복 (흡혈)
    RECOIL,           // 데미지의 N% 만큼 시전자 피해 (반동)
    
    // [전장 제어]
    KNOCKBACK,        // 대상을 후열로 이동
    PULL,             // 대상을 전열로 이동
    SWAP_POS,         // 위치 바꿈
    
    // [유틸리티]
    CLEANSE,          // 아군의 '해로운' EffectTag 제거
    DISPEL,           // 적의 '이로운' EffectTag 제거
    AGGRO_RESET,      // 시전자의 누적 어그로 초기화
    AGGRO_UP,
    
}

/// <summary>
/// 지속적인 효과
/// </summary>
public enum SkillEffect
{
    NONE,
    
    // [상태 제어 - CC]
    STUN,           // 기절 (행동 불가)
    SLEEP,          // 수면 (피격 시 해제)
    CONFUSION,      // 혼란 (아군 공격 가능)
    TAUNT,          // 도발 (특정 대상만 공격 강제)
    FEAR,           // 공포 (스킬 사용 불가)
    
    // [지속 피해 - DoT]
    POISON,         // 독 (체력 비례 피해)
    BLEED,          // 출혈 (행동 시 피해, 중첩됨)
    BURN,           // 화상 (피해 + 방어력 감소)
    
    // [스탯 변화 - Stat Mod]
    BUFF_ATK,       // 공격력 증가
    BUFF_DEF,       // 방어력 증가
    DEBUFF_ATK,     // 공격력 감소 (Weakness)
    DEBUFF_DEF,     // 방어력 감소 (Armor Break)
    SLOW,           // 속도 감소
    HASTE,          // 속도 증가
    
    // [특수 버프]
    SHIELD,         // 보호막 (체력 대신 소모)
    IMMORTAL,       // 불사 (체력이 1 아래로 안 내려감)
    REGEN,           // 재생 (매 턴 회복)
    
    // 인챈트
    ENCHANT_POISON,
}

public enum TagType
{
    None = 0,

    // 원소 속성
    Fire,       // 화염 (화상, 폭발)
    Ice,        // 얼음 (빙결, 둔화)
    Electric,   // 전기 (감전, 기계 추댐)
    Poison,     // 독
    Holy,       // 신성 (언데드 추댐)
    Dark,       // 암흑
    
    // 상호작용 상태
    Wet,        // 젖음 (전기 취약)
    Oiled,      // 기름 (불 취약)
    Frozen,     // 빙결 (행동 불가 + 물리 취약)
    Stunned,    // 기절 (행동 불가)
    
    // 유닛 특성
    Undead,     // 언데드 (힐->데미지, 신성 취약)
    Machine,    // 기계 (독 면역, 전기 취약)
    Flying,     // 비행 (근접 회피)
    Heavy,      // 무거움 (넉백 면역)
}

public enum DungeonTag
{
    None,
    Darkness,   // 어둠: 원거리 효율 감소
    Swamp,      // 늪지: 민첩성 감소
    HolyGround, // 성역: 신성 마법 강화
    Cursed,     // 저주: 회복 효율 감소
    Narrow      // 좁은길: 광역 공격 효율 감소
}

public enum BattleState
{
    Idle,
    Setup,
    InBattle,
    Win,
    Lose
}

[System.Flags]
public enum DamageType
{
    None        = 0,
    Physical    = 1 << 0, //1
    Magical     = 1 << 1, //2
    Mental      = 1 << 2, //4
    Cursed      = 1 << 3, //8
    Holy        = 1 << 4, //16
    True        = 1 << 5, //32
}
