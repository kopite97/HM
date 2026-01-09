using UnityEngine;
using System.Collections;
using System.Text;

public class GameTester : MonoBehaviour
{
    void Start()
    {
        // 1. 데이터 로드가 완료될 때까지 잠시 대기하거나, 
        // DataManager.Awake가 먼저 돌았다고 가정하고 실행합니다.
        
        Debug.Log("=== 🧪 스킬 시스템 테스트 시작 ===");

        // 테스트 케이스 1: 전사 생성 (ID: 1001)
        TestAdventurerCreation(1001, "전사 테스트");

        // 테스트 케이스 2: 마법사 생성 (ID: 1002)
        TestAdventurerCreation(1002, "마법사 테스트");
    }

    void TestAdventurerCreation(int classId, string testName)
    {
        Debug.Log($"\n--- {testName} ---");

        // 1. 모험가 생성 시도
        Adventurer adv = AdventurerFactory.Instance.CreateRandomAdventurer(classId);

        if (adv == null)
        {
            Debug.LogError("❌ 모험가 생성 실패! ClassData나 ID를 확인하세요.");
            return;
        }

        // 2. 기본 정보 출력
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"이름: {adv.Name} (직업: {adv.JobName})");
        sb.AppendLine($"방어 점수: {adv.DefenseScore} / 선호 포지션: <color=yellow>{adv.PreferredPosition}</color>");

        // 3. 스킬 정보 검증
        if (adv.Skills.Count > 0)
        {
            sb.AppendLine($"\n[보유 스킬 목록 - 총 {adv.Skills.Count}개]");
            
            foreach (var skill in adv.Skills)
            {
                // LearnedSkill의 프로퍼티가 제대로 계산되는지 확인
                sb.AppendLine($"🔹 <b>{skill._sourceSkill.Name}</b> (Lv.{skill.Level})");
                sb.AppendLine($"   - 타입: {skill._sourceSkill.Type} / 사거리: {skill._sourceSkill.Range}");
                sb.AppendLine($"   - 현재 쿨타임: {skill.CurrentCooldown}s (기본: {skill._sourceSkill.Cooldown}s)");
                foreach (var modifier in skill._sourceSkill.Modifiers)
                {
                    sb.AppendLine($"    - 스킬 Modifiers : {modifier}");
                }

                foreach (var action in skill._sourceSkill.Actions)
                {
                    sb.AppendLine($"    - 스킬 Actions : {action}");
                }

                foreach (var effect in skill._sourceSkill.StatusEffects)
                {
                    sb.AppendLine($"    -스킬 Status Effets : {effect}");
                }

                foreach (var trait in skill._sourceSkill.Traits)
                {
                    sb.AppendLine($"    -스킬  Traits : {trait}");
                }
                
                // 계수 배열 출력 확인
                float[] coefs = skill.GetCurrentPowerCoefs();
                string coefStr = string.Join(", ", coefs);
                sb.AppendLine($"   - 현재 계수: [{coefStr}]");
            }
        }
        else
        {
            sb.AppendLine("❌ 보유한 스킬이 없습니다.");
        }

        Debug.Log(sb.ToString());
    }
}