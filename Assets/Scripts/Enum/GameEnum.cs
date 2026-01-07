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
/// 데미지/치유량 계산 공식에 영향을 주는 보정값
/// </summary>
public enum SkillModifier
{
    None = 0,

    // [방어/회피 관련]
    IgnoreDefense,      // 방어력 무시 (True Damage)
    GuaranteedHit,      // 필중 (회피 불가)
    CannotBeBlocked,    // 방어(Block) 불가

    // [치명타 관련]
    GuaranteedCrit,     // 확정 치명타
    CritDamageUp,       // 치명타 피해량 증가 보정

    // [데미지 스케일링]
    ExecuteDamage,      // 잃은 체력 비례 데미지 증가 (처형)
    LifeSteal,          // 데미지의 N% 만큼 시전자 회복 (흡혈)
    RecoilDamage,       // 데미지의 N% 만큼 시전자 피해 (반동)
    
    // [특수 연산]
    PenetrateShield,    // 보호막 무시하고 직접 체력 타격
    
}

/// <summary>
/// 즉발적으로 수행하는 행동 (이동, 해제, 특수조작)
/// </summary>
public enum SkillAction
{
    None = 0,

    // [회복/자원]
    Heal,               // 체력 회복 (데미지 파이프라인 대신 회복 로직 탐)
    Revive,             // 부활
    DrainMana,          // 대상 마나 소멸
    RecoveryMana,       // 마나 재생
    

    // [위치 제어 - Physics]
    Knockback,          // 밀치기 (후열로)
    Pull,               // 당기기 (전열로)
    SwapPosition,       // 위치 서로 바꿈
    Teleport,           // 특정 위치로 이동

    // [상태 제어 - Meta]
    CleanseDebuff,      // 아군의 해로운 효과 제거 (정화)
    DispelBuff,         // 적의 이로운 효과 제거 (해제)
    ResetCooldown,      // 쿨타임 초기화
    
    // [어그로 제어]
    AggroReset,         // 어그로 초기화
    AggroProvoke,       // 대량의 어그로 즉시 획득
}

/// <summary>
/// 턴 단위로 지속되는 버프/디버프
/// </summary>
public enum StatusEffect
{
    None = 0,

    // [군중 제어 - CC]
    Stun,               // 기절 (행동 불가)
    Sleep,              // 수면 (피격 시 해제)
    Freeze,             // 빙결 (행동 불가 + 물리 내성 증가 + 화염 취약)
    Confusion,          // 혼란 (아군 공격 가능)
    Fear,               // 공포 (스킬 사용 불가)
    Taunt,              // 도발 (특정 대상만 공격 강제)
    Silence,            // 침묵 (마법 사용 불가)

    // [지속 피해 - DoT]
    Poison,             // 독 (체력 비례 피해, 치유 감소)
    Bleed,              // 출혈 (행동 시 피해, 중첩 가능)
    Burn,               // 화상 (지속 피해 + 방어력 감소)

    // [스탯 변화 - Stat Mod]
    BuffAttack,         // 공격력 증가
    BuffDefense,        // 방어력 증가
    DebuffAttack,       // 공격력 감소 (Weakness)
    DebuffDefense,      // 방어력 감소 (Armor Break)
    SpeedUp,            // 가속 (Haste)
    SpeedDown,          // 감속 (Slow)

    // [특수 상태]
    Shield,             // 보호막 (체력 대신 소모)
    Invincible,         // 무적 (데미지 0)
    Immortal,           // 불사 (체력이 1 아래로 안 내려감)
    RegenerationHP,     // 재생 (매 턴 회복)
    RegenerationMP,     // 마나 재생
    Reflect,            // 반사 (받은 피해의 일부를 돌려줌)
    
    // [인챈트 : 공격 시 발동]
    EnchantPoison,      // 공격 시 적에게 중독 부여
    EnchantFire,        // 공격 시 적에게 화상 부여 + 추가 화염 데미지
    StanceCounter,      // 피격 시 반격하는 태세
}

/// <summary>
/// 유닛, 스킬, 상태가 가진 고유 특성 (상호작용 조건)
/// </summary>
public enum Trait
{
    None = 0,

    // [1. 원소 속성 - Elements]
    Physical,       // 물리
    Magical,        // 마법
    Fire,           // 화염
    Ice,            // 얼음
    Electric,       // 전기
    Poison,         // 독 (속성으로서의 독)
    Holy,           // 신성
    Dark,           // 암흑
    
    // [2. 공격 타입 - Attack Type]
    Melee,          // 근접
    Ranged,         // 원거리
    AreaOfEffect,   // 광역(AoE)
    Projectile,     // 투사체 (막을 수 있음)

    // [3. 유닛 종족/특성 - Species]
    Humanoid,       // 인간형
    Undead,         // 언데드 (힐->데미지)
    Beast,          // 야수
    Machine,        // 기계 (상태이상 면역 등)
    Demon,          // 악마
    
    // [4. 상태 조건 - Condition]
    // (StatusEffect가 적용될 때 이 Trait도 같이 켜짐)
    Flying,         // 비행 (근접 회피)
    Heavy,          // 무거움 (넉백 면역)
    Wet,            // 젖음 (전기 연계)
    Oiled,          // 기름 (화염 연계)
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
