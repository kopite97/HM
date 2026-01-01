using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PartyManager : Singleton<PartyManager>
{
    private const float VANGUARD_THRESHOLD = 150f; 
    public int MaxPartySize = 4;

    [SerializeField]
    private List<Adventurer> _playerParty = new List<Adventurer>();
    public IReadOnlyList<Adventurer> PlayerParty => _playerParty; // 읽기 전용 프로퍼티 추가

    public float GetVanguardThreshold() => VANGUARD_THRESHOLD;

    public void AddMemeber(Adventurer adv)
    {
        if (_playerParty.Count >= MaxPartySize)
        {
            Debug.LogWarning("파티가 이미 가득 찼습니다.");
            return;
        }

        if (!_playerParty.Contains(adv))
        {
            _playerParty.Add(adv);
            AutoAssignPositions(_playerParty);
        }
    }
    
    //선호 포지션과 파티 밸런스를 고려한 스마트 배치
    public void AutoAssignPositions(List<Adventurer> members)
    {
        if (members == null || members.Count == 0) return;

        // 1. 고정되지 않은(자유로운) 멤버들만 추출하여 방어 점수순 정렬
        var freeMembers = members
            .Where(m => !m.IsPositionLocked)
            .OrderByDescending(m => m.GetDefenseScore(DataManager.Instance.DefenseWeight))
            .ToList();

        if (freeMembers.Count == 0) return; // 배치할 사람이 없으면 종료

        // 2. 일단 모두가 원하는 선호 포지션으로 배치 시도
        foreach (var m in freeMembers)
        {
            // 아직 선호 포지션이 없다면(None), 방어 점수 기반으로 임시 결정
            if (m.PreferredPosition == PartyPosition.None)
            {
                // 스킬/데이터가 로드되지 않았을 때를 위한 안전장치
                m.SetPosition(m.DefenseScore >= VANGUARD_THRESHOLD ? PartyPosition.Vanguard : PartyPosition.Midguard);
            }
            else
            {
                m.SetPosition(m.PreferredPosition);
            }
        }

        // 3. 파티가 생존 가능한 구성인가? (전열 유무 체크)
        if (!IsPartySafe(members))
        {
            // 4. 전열이 없다면, 자유 멤버 중 '가장 튼튼한 1명'을 강제로 전열로 보냄
            Debug.LogWarning("[Party] 전열(Vanguard) 부재로 인해 자동 조정을 실행합니다.");
            
            // freeMembers는 이미 방어 점수 내림차순 정렬되어 있음 -> 0번이 가장 튼튼함
            freeMembers[0].SetPosition(PartyPosition.Vanguard);
        }

        // 5. 만약 후열(Rear)이나 중열에 너무 쏠려 있다면? (TODO)
        // 너무 많은 인원이 후열에 있으면 딜러 보호가 안되므로 일부를 중열로 당기는 로직 등을 여기에 추가 가능
        
        LogPartyFormation(members);
    }
    
    // 파티에 최소한의 탱커(전열)가 존재하는지 확인
    private bool IsPartySafe(List<Adventurer> members)
    {
        // 고정된 멤버와 자동 배치된 멤버 모두를 포함해 전열 수를 확인
        int vanguardCount = members.Count(m => m.AssignedPosition == PartyPosition.Vanguard);
        
        // 전열이 1명이라도 있으면 안전하다고 판단
        return vanguardCount > 0;
    }

    // --- 수동 조작 (Drag & Drop) ---
    public void ManualSwap(Adventurer targetAdv, PartyPosition targetPos)
    {
        // 1. 해당 자리에 이미 누가 있는지 확인
        var currentOccupant = _playerParty.Find(m => m.AssignedPosition == targetPos);

        // 2. 자리에 주인이 있고, 그게 나 자신이 아니라면
        if (currentOccupant != null && currentOccupant != targetAdv)
        {
            // 밀려나는 사람은 잠금 해제 (자동 배치 로직에 맡김)
            currentOccupant.UnlockPosition();
            
            // 밀려나는 사람을 타겟이 있던 자리로 보냄 (단순 스왑)
            currentOccupant.SetPosition(targetAdv.AssignedPosition);
        }
        
        // 3. 타겟 캐릭터 고정 배치
        targetAdv.SetManualPosition(targetPos);
        
        // 4. 한 명이 고정되었으니, 나머지 인원들의 밸런스를 다시 맞춤
        // (예: 탱커를 후열로 뺐다면, 누군가 대신 전열로 가야 함)
        AutoAssignPositions(_playerParty); // 수동으로 바꾸려면 빼는 것도 고려
        
        Debug.Log($"[Manual] {targetAdv.Name} -> {targetPos} 고정 완료.");
    }

    private void LogPartyFormation(List<Adventurer> members)
    {
        int v = members.Count(m => m.AssignedPosition == PartyPosition.Vanguard);
        int m = members.Count(m => m.AssignedPosition == PartyPosition.Midguard);
        int r = members.Count(m => m.AssignedPosition == PartyPosition.Rearguard);
        Debug.Log($"[Party] 배치 결과: 전열{v} / 중열{m} / 후열{r}");
    }
}