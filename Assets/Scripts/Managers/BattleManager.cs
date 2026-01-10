using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class BattleManager : Singleton<BattleManager>
{
    
    public BattleState CurrentState = BattleState.Idle;
    public BattleEnvironment CurrentEnv;
    
    // 전투에 참여중인 모든 유닛 (속도 순서 정렬용)
    private List<BattleUnit> _allUnits = new List<BattleUnit>();
    
    // 진영별 리스트 (타겟팅 용도)
    private List<BattleUnit> _playerUnits = new List<BattleUnit>();
    private List<BattleUnit> _enemyUnits = new List<BattleUnit>();

    public float TimeScale { get; private set; } = 1.0f;

    private const float TURN_THRESHOLD = 100f;
    private bool _isProcessingTurn = false;

    private void Update()
    {
        if (CurrentState == BattleState.InBattle)
        {
            return;
        }

        if (_isProcessingTurn) return;

        ProcessActionGauge();
    }

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
    }
    
    private void InitializeUnits(List<Adventurer> party, List<MonsterData> monsters)
    {
        _allUnits.Clear();
        _playerUnits.Clear();
        _enemyUnits.Clear();

        foreach (var adv in party)
        {
            var advAdapter = new AdventurerAdapter(adv);
            BattleUnit unit =  new BattleUnit(advAdapter);
            _allUnits.Add(unit);
            _playerUnits.Add(unit);
        }

        foreach (var monsterData in monsters)
        {
            Monster mon = new Monster(monsterData);
            var monsterAdapter = new MonsterAdapter(mon);
            BattleUnit unit = new BattleUnit(monsterAdapter);
            _allUnits.Add(unit);
            _playerUnits.Add(unit);
        }
        Debug.Log("Success Battle Initialize");
    }

    private void ProcessActionGauge()
    {
        var readyUnit = GetReadyUnit();

        if (readyUnit != null)
        {
            // 유닛 행동 시작
            StartCoroutine(ProcessTurnRoutine(readyUnit));
        }
        else
        {
            // 아무도 준비 x -> 시간 흘리기 (게이지 충전)
            float deltaTime = Time.deltaTime *  TimeScale;

            foreach (var unit in _allUnits)
            {
                if(unit.IsDead) continue;
                unit.TickGauge(deltaTime);
            }
        }
    }

    private IEnumerator ProcessTurnRoutine(BattleUnit actor)
    {
        _isProcessingTurn = true;
        
        // 턴 시작 처리
        actor.OnTurnStart(TURN_THRESHOLD);
        
        // 상태이상 적용

        
        // 행동 결정
        (LearnedSkill skill, BattleUnit target) action = DecideBsetAction(actor);

        if (action.skill == null || action.target == null)
        {
            actor.OnTurnEnd();
            _isProcessingTurn = false;
            yield break;
        }
        
        // 연출 (이동->공격->복귀) 애니메이션
        yield return StartCoroutine(VisualizeAction(actor, action.target, action.skill));
        
        // 결과 적용 (데미지/이펙트 계산 및 적용)
        ApplyActionResult(actor, action.target, action.skill);
        
        // 턴 종료 
        actor.OnTurnEnd();

        yield return new WaitForSeconds(0.5f);

        CheckBattleEnd();    
        _isProcessingTurn = false;
    }
    
    private (LearnedSkill skill, BattleUnit target) DecideBsetAction(BattleUnit actor)
    {
        // 흐퇴 조건 체크
        
        // 사용 가능한 스킬 필터링 (쿨타임,코스트 체크)
        var availableSkills = actor.GetSkills().Where(s => s.IsReady(actor)).ToList();

        // 우선순위에 따른 스킬 선택

        // 전략에 따른 타겟 변경
        return (null, null);
    }

    private string VisualizeAction(BattleUnit actor, BattleUnit actionTarget, LearnedSkill actionSkill)
    {
        throw new NotImplementedException();
    }

    private void ApplyActionResult(BattleUnit actor, BattleUnit actionTarget, LearnedSkill actionSkill)
    {
        throw new NotImplementedException();
    }
    

    
    private void CheckBattleEnd()
    {
        throw new NotImplementedException();
    }


    // 게이지가 가장 높은 유닛 반환 (초과분 고려)
    private BattleUnit GetReadyUnit()
    {
        return _allUnits
            .Where(u => !u.IsDead && u.ActionGauge >= TURN_THRESHOLD)
            .OrderByDescending(u => u.ActionGauge) // 게이지 높은 순 (선점)
            .ThenByDescending(u => u.GetStat(StatType.Body_Mobility)) // 동점 시 민첩 순
            .FirstOrDefault();
    }
}