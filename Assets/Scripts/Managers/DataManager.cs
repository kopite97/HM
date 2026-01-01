using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public static DataManager Instance;
    
    public Dictionary<int,ClassData> ClassDict = new Dictionary<int,ClassData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        LoadClassData();
    }
    
    private void LoadClassData()
    {
        // Resources/Data/ClassData.csv 파일을 텍스트로 읽어옴
        TextAsset textAsset = Resources.Load<TextAsset>("Data/ClassData");
        if (textAsset == null)
        {
            Debug.LogError("❌ ClassData.csv 파일을 찾을 수 없습니다. [Tools] 메뉴에서 다운로드 하세요.");
            return;
        }

        string[] lines = textAsset.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            
            string[] row = line.Split(',');
            
            
            ClassData data = new  ClassData();

            try
            {
                data.ID = int.Parse(row[0]);
                data.NameKR = row[1];
                data.NameEN = row[2];
                data.DescKR = row[3];
                data.DescEN = row[4];

                // 가중치 매핑
                data.W_Might = int.Parse(row[5]);
                data.W_Endurance = int.Parse(row[6]);


                ClassDict.Add(data.ID, data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[파싱 에러] {i}번째 줄: {e.Message}");
            }
            Debug.Log($"✅ ClassData 로드 완료! 총 {ClassDict.Count}개 직업.");
        }
    }
}