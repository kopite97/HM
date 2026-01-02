// 전투 스탯 (ClassData의 W_Might -> Might)
public enum StatType
{
    // [Body]
    Might, Endurance, Reflex, Poise, Mobility, Vitality,
    // [Mind]
    Valor, Composure, Focus, Judgment, Resolve, Insight, Awareness, Command,
    // [Tech]
    Arms, Archery, Sorcery, Faith, Subtlety, Guarding
}

// 성격 (ClassData의 W_Nature_Duty -> Duty)
public enum NatureType
{
    Duty, Discord, Patience, Ambition, Greed, 
    Cunning, Arrogance, Stubborn, Honor, Loyalty
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

public enum SkillCostType
{
    NONE,
    STAMINA,
    MANA
}

public enum EffectTag
{
    NONE,
    STUN,
    AGGRO_RESET,
    AGGRO_UP,
    KNOCKBACK,
    IGNORE_DEF,
    MORALE_DOWN,
    STAU_AOE,
    HEAL,
    DMG_ABSORB
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