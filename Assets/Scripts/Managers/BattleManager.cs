using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BattleManager : Singleton<BattleManager>
{
    [Header("Settings")]
    public float TurnDelay = 1.0f; // 턴 사이의 대기 시간 (애니메이션 등 고려)

    [Header("Runtime Info")]
    public BattleState CurrentState = BattleState.Idle;
    public BattleEnvironment CurrentEnv;
    
    // 전투에 참여중인 모든 유닛 (속도 순서 정렬용)
    private List<BattleUnit> _allUnits = new List<BattleUnit>();
    
    // 진영별 리스트 (타겟팅 용도)
    private List<BattleUnit> _playerUnits = new List<BattleUnit>();
    private List<BattleUnit> _enemyUnits = new List<BattleUnit>();

    /// <summary>
    /// 전투를 시작합니다. (던전 입장 시 호출)
    /// </summary>
    /// <param name="party">플레이어 파티</param>
    /// <param name="monsters">등장할 몬스터 데이터 목록</param>
    public void StartBattle(List<Adventurer> party, List<MonsterData> monsters)
    {
        if (CurrentState == BattleState.InBattle) return;

        CurrentState = BattleState.Setup;
        CurrentEnv = new BattleEnvironment(); // 나중에 던전 태그 로드 로직 추가

        // 1. 유닛 초기화 & 어댑터 연결
        InitializeUnits(party, monsters);

        // 2. 전투 루프 시작
        StartCoroutine(BattleRoutine());
    }

    private void InitializeUnits(List<Adventurer> party, List<MonsterData> monsters)
    {
        _allUnits.Clear();
        _playerUnits.Clear();
        _enemyUnits.Clear();

        // 플레이어 생성 (AdventurerAdapter 사용)
        foreach (var adv in party)
        {
            var adapter = new AdventurerAdapter(adv);
            var unit = new BattleUnit(adapter); // Faction은 내부에서 처리된다고 가정 (혹은 수동 설정)
            // * BattleUnit 생성자에 Faction 파라미터를 추가하거나 Adapter에서 가져오도록 수정 필요
            // 여기서는 편의상 리스트에 넣을 때 구분합니다.
            
            _playerUnits.Add(unit);
            _allUnits.Add(unit);
        }

        // 몬스터 생성 (MonsterAdapter 사용)
        foreach (var mData in monsters)
        {
            // 몬스터의 인스턴스(Monster) 생성 -> 어댑터 -> BattleUnit
            // 위치는 임의로 분배한다고 가정 (데이터에 있다면 그것 사용)
            var monsterInstance = new Monster(mData, PartyPosition.Vanguard); 
            var adapter = new MonsterAdapter(monsterInstance);
            var unit = new BattleUnit(adapter);

            _enemyUnits.Add(unit);
            _allUnits.Add(unit);
        }

        Debug.Log($"⚔️ 전투 개시! 아군 {_playerUnits.Count}명 vs 적군 {_enemyUnits.Count}명");
    }

    /// <summary>
    /// 메인 전투 루프 (코루틴)
    /// </summary>
    private IEnumerator BattleRoutine()
    {
        CurrentState = BattleState.InBattle;
        int turnCount = 1;

        while (CurrentState == BattleState.InBattle)
        {
            Debug.Log($"--- 🔄 Turn {turnCount} Start ---");

            // 1. 행동 순서 결정 (속도 = Reflex + Mobility 등)
            // 내림차순 정렬 (빠른 놈이 먼저)
            _allUnits = _allUnits
                .OrderByDescending(u => u.GetStat(StatType.Body_Reflex) + u.GetStat(StatType.Body_Mobility))
                .ToList();

            // 2. 각 유닛 행동 처리
            foreach (var unit in _allUnits)
            {
                if (unit.IsDead) continue; // 죽은 유닛 스킵
                if (CheckBattleEnd()) yield break; // 전투 종료 체크

                // 유닛의 턴 진행
                yield return StartCoroutine(ProcessTurn(unit));
            }

            turnCount++;
            yield return null;
        }
    }

    /// <summary>
    /// 개별 유닛의 턴 처리 로직
    /// </summary>
    private IEnumerator ProcessTurn(BattleUnit actor)
    {
        // 쿨타임 감소 처리 등 (BattleUnit 내부에 Tick 메서드 필요)
        // actor.TickCooldowns(); 

        // 1. 사용할 스킬 선택 (AI)
        LearnedSkill selectedSkill = SelectBestSkill(actor);
        
        if (selectedSkill != null)
        {
            // 2. 타겟 선택 (AI)
            BattleUnit target = SelectBestTarget(actor, selectedSkill);

            if (target != null)
            {
                // 3. 행동 수행 (계산 및 적용)
                PerformAction(actor, target, selectedSkill);
            }
            else
            {
                Debug.Log($"{actor.Name}은(는) 대상을 찾지 못했다...");
            }
        }
        else
        {
            Debug.Log($"{actor.Name}은(는) 사용할 수 있는 스킬이 없다 (휴식)");
            // 마나 회복 등의 로직
        }

        // 연출을 위한 딜레이
        yield return new WaitForSeconds(TurnDelay);
    }

    /// <summary>
    /// 가장 적절한 스킬을 선택하는 간단한 AI
    /// </summary>
    private LearnedSkill SelectBestSkill(BattleUnit actor)
    {
        // TODO: 실제로는 쿨타임, 코스트(마나) 체크 필요
        // 지금은 가지고 있는 첫 번째 공격 스킬을 무조건 사용한다고 가정
        var skillIDs = actor.GetSkillIDs(); // 인터페이스를 통해 ID 목록 가져옴
        
        foreach(var id in skillIDs)
        {
            if(DataManager.Instance.SkillDict.TryGetValue(id, out SkillData sData))
            {
                // 쿨타임 체크 로직이 BattleUnit에 있다면 여기서 확인
                // if (!actor.IsSkillReady(id)) continue;
                
                // 임시: LearnedSkill 객체를 급조해서 리턴 (실제론 BattleUnit이 들고 있어야 함)
                return new LearnedSkill(sData, actor.Level); 
            }
        }
        return null; 
    }

    /// <summary>
    /// 스킬 타입에 맞춰 최적의 타겟을 선정 (어그로 로직 포함)
    /// </summary>
    private BattleUnit SelectBestTarget(BattleUnit actor, LearnedSkill skill)
    {
        bool isPlayerSide = _playerUnits.Contains(actor);
        List<BattleUnit> enemies = isPlayerSide ? _enemyUnits : _playerUnits;
        List<BattleUnit> allies = isPlayerSide ? _playerUnits : _enemyUnits;

        // 타겟이 아군인가? (힐/버프)
        if (skill.Data.Target == SkillTarget.ALLY_SINGLE)
        {
            // 체력 비율이 가장 낮은 아군 선택
            return allies
                .Where(u => !u.IsDead)
                .OrderBy(u => u.CurrentHP / u.MaxHP)
                .FirstOrDefault();
        }
        // 타겟이 적군인가? (공격)
        else
        {
            // [기본 규칙] 전열(Vanguard) 우선 타격
            var livingEnemies = enemies.Where(u => !u.IsDead).ToList();
            
            var vanguards = livingEnemies.Where(u => u.CurrentPosition == PartyPosition.Vanguard).ToList();
            if (vanguards.Count > 0)
            {
                // 전열 중 랜덤 혹은 가장 약한 적
                return vanguards[Random.Range(0, vanguards.Count)];
            }

            // 전열이 없으면 아무나 (중열 -> 후열 순)
            return livingEnemies.FirstOrDefault();
        }
    }

    private void PerformAction(BattleUnit actor, BattleUnit target, LearnedSkill skill)
    {
 
        // ------------데미지 파이프 라인 ------------
        
        // 파이프 라인 호출
        DamageContext result = DamagePipeline.Process(actor, target, skill, CurrentEnv);

        if (result.IsHeal)
        {
            target.Heal(result.FinalResult);
        }
        else
        {
            target.TakeDamage(result.FinalResult);
        }
        
        // --------이페긑 파이프라인--------------- 
        // 힐이나 단순 공격이 아니라 효과가 있는 경우만 실행
        if (skill.Data.Effect_Tag != EffectTag.NONE && skill.Data.Effect_Tag != EffectTag.HEAL)
        {
            EffectContext effResult = EffectPipeline.Process(actor, target, skill);

            if (effResult.IsSuccess)
            {
                // TODO 상태부여
                // target.ApplyEffect 
            }
            else
            {
                // 상태이상 저항
            }
        }
    }

    private bool CheckBattleEnd()
    {
        // 아군 전멸 체크
        if (_playerUnits.All(u => u.IsDead))
        {
            EndBattle(false);
            return true;
        }

        // 적군 전멸 체크
        if (_enemyUnits.All(u => u.IsDead))
        {
            EndBattle(true);
            return true;
        }

        return false;
    }

    private void EndBattle(bool isWin)
    {
        CurrentState = isWin ? BattleState.Win : BattleState.Lose;
        Debug.Log(isWin ? "🎉 승리! 던전 클리어!" : "☠️ 패배... 파티가 전멸했습니다.");
        
        // TODO: 보상 지급 or 게임 오버 UI 호출
    }
}