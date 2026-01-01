using UnityEngine;
using System.Collections;
using System.Text;

public class GameTester : MonoBehaviour
{
    void Start()
    {
        // 데이터가 로드될 시간을 벌기 위해 1초 뒤에 테스트 실행
        StartCoroutine(TestRoutine());
    }

    IEnumerator TestRoutine()
    {
        Debug.Log("⏳ 데이터 로드 대기 중...");
        yield return new WaitForSeconds(1.0f); 

        // 데이터가 잘 로드되었는지 확인
        if (DataManager.Instance.ClassDict.Count == 0)
        {
            Debug.LogError("❌ 데이터 로드 실패! URL이나 DataManager를 확인하세요.");
            yield break;
        }

        Debug.Log("🧪 --- [모험가 생성 테스트 시작] ---");

        // 1. 전사(1001) 생성 테스트
        CreateAndLog(1001);

        // 2. 마법사(1002) 생성 테스트
        CreateAndLog(1002);
        
        // 3. 도적(1003) 생성 테스트
        CreateAndLog(1003);

    }

    void CreateAndLog(int classId)
    {
        Adventurer adv = AdventurerFactory.Instance.CreateRandomAdventurer(classId);

        if (adv != null)
        {
            Debug.Log($"\n✨ <b>[{adv.PotentialGrade}급] {adv.JobName} {adv.Name}</b> (나이: {adv.Age}세)");
            Debug.Log($"   💪 <b>잠재력 총합:</b> {adv.TotalPotential} (현재 스탯 총합: {adv.CurrentTotalStat})");
            
            // 주요 스탯 몇 개만 로그 찍어보기
            string statLog = "   📊 <b>주요 스탯:</b> ";
            StringBuilder sb = new StringBuilder();

            foreach (var val in adv.Stats)
            {
                sb.Append(val.Key + ": " + val.Value).Append("\n");
            }
            
            
            foreach (var nature in adv.Natures)
            {
                sb.Append(nature.Key + ": " + nature.Value).Append("\n");
            }
            
            Debug.Log(sb.ToString());
        }
    }
}