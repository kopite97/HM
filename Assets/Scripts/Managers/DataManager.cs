using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection; // 리플렉션 사용을 위해 필요

public class DataManager : Singleton<DataManager>
{
    public Dictionary<int, ClassData> ClassDict = new Dictionary<int, ClassData>();
    [Header("Game Settings")] public DefenseWeightData DefenseWeight;

    private void Awake() => LoadClassData();

    private void LoadClassData()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Data/ClassData");
        if (textAsset == null) return;

        // 1. 줄바꿈 기준 분리
        string[] lines = textAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        // 2. 헤더(첫 줄) 분석: 컬럼명과 인덱스 매핑
        string[] headers = lines[0].Split(',');
        Dictionary<string, int> headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++)
        {
            headerMap[headers[i].Trim()] = i;
        }

        // 3. 데이터 파싱
        FieldInfo[] fields = typeof(ClassData).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            ClassData data = new ClassData();

            foreach (FieldInfo field in fields)
            {
                // 클래스 필드 이름이 CSV 헤더에 있는지 확인
                if (headerMap.TryGetValue(field.Name, out int index))
                {
                    if (index >= row.Length) continue;
                    
                    string value = row[index].Trim();
                    
                    // 타입에 맞게 자동으로 값 대입
                    if (field.FieldType == typeof(int))
                        field.SetValue(data, int.Parse(value));
                    else if (field.FieldType == typeof(string))
                        field.SetValue(data, value);
                    else if (field.FieldType == typeof(float))
                        field.SetValue(data, float.Parse(value));
                }
            }
            
            if (!ClassDict.ContainsKey(data.ID))
                ClassDict.Add(data.ID, data);
        }
        Debug.Log($"✅ [자동 파싱 완료] 총 {ClassDict.Count}개 직업 로드.");
    }
}