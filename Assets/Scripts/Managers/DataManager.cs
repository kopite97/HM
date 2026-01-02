using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection; // 리플렉션 사용을 위해 필요

public class DataManager : Singleton<DataManager>
{
    public Dictionary<int, ClassData> ClassDict = new Dictionary<int, ClassData>();
    [Header("Game Settings")] public DefenseWeightData DefenseWeight;
    
    public Dictionary<int,SkillData> SkillDict = new Dictionary<int, SkillData>();
    public Dictionary<int, MonsterData> MonsterDict = new Dictionary<int, MonsterData>();
    private void Awake()
    {
        LoadClassData();
        LoadSkillData();
    }

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

private void LoadSkillData()
    {
        SkillDict.Clear();
        
        TextAsset textAsset = Resources.Load<TextAsset>("Data/SkillData");
        if (textAsset == null) 
        {
            Debug.LogError("SkillData CSV 파일을 찾을 수 없습니다.");
            return;
        }

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

        // 3. 데이터 파싱 (리플렉션 활용)
        FieldInfo[] fields = typeof(SkillData).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 1; i < lines.Length; i++)
        {
            // 쉼표로 분리 (단, 설명 등에 쉼표가 들어갈 경우 복잡한 정규식 필요하지만, 여기선 기본 Split 사용)
            string[] row = lines[i].Split(',');
            SkillData data = new SkillData();
            bool parseSuccess = true;

            foreach (FieldInfo field in fields)
            {
                // CSV 헤더에 필드 이름이 존재하는지 확인
                if (headerMap.TryGetValue(field.Name, out int index))
                {
                    if (index >= row.Length) continue;

                    string value = row[index].Trim();
                    
                    // 빈 값인 경우 기본값 유지
                    if (string.IsNullOrEmpty(value)) continue;

                    try
                    {
                        // --- 타입별 자동 대입 로직 ---
                        Type type = field.FieldType;

                        // 1. 기본 타입 (int, float, string)
                        if (type == typeof(int))
                        {
                            field.SetValue(data, int.Parse(value));
                        }
                        else if (type == typeof(float))
                        {
                            field.SetValue(data, float.Parse(value));
                        }
                        else if (type == typeof(string))
                        {
                            field.SetValue(data, value);
                        }
                        // 2. Enum 타입 처리 (SkillType, SkillRange 등)
                        else if (type.IsEnum)
                        {
                            field.SetValue(data, Enum.Parse(type, value));
                        }
                        // 3. 배열 타입 처리 (세미콜론 ';' 구분자 사용)
                        else if (type.IsArray)
                        {
                            string[] arrayValues = value.Split(';');
                            Type elementType = type.GetElementType(); // 배열의 요소 타입 확인

                            if (elementType == typeof(float))
                            {
                                // float[] 파싱
                                float[] floatArr = new float[arrayValues.Length];
                                for(int k=0; k<arrayValues.Length; k++) floatArr[k] = float.Parse(arrayValues[k]);
                                field.SetValue(data, floatArr);
                            }
                            else if (elementType.IsEnum)
                            {
                                // Enum[] 파싱 (StatType[] 등)
                                // 리플렉션으로 해당 Enum 타입의 배열 인스턴스 생성
                                Array enumArr = Array.CreateInstance(elementType, arrayValues.Length);
                                for(int k=0; k<arrayValues.Length; k++)
                                {
                                    object enumVal = Enum.Parse(elementType, arrayValues[k].Trim());
                                    enumArr.SetValue(enumVal, k);
                                }
                                field.SetValue(data, enumArr);
                            }
                            // 필요하다면 int[] 등 추가
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[LoadSkillData] 파싱 오류! 필드: {field.Name}, 값: {value} / {e.Message}");
                        parseSuccess = false;
                    }
                }
            }

            // ID 중복 체크 및 추가
            if (parseSuccess && !SkillDict.ContainsKey(data.ID))
            {
                SkillDict.Add(data.ID, data);
            }
        }

        Debug.Log($"✅ [SkillData] 총 {SkillDict.Count}개 스킬 로드 완료.");
    }

private void LoadMonsterData()
    {
        MonsterDict.Clear();
        TextAsset textAsset = Resources.Load<TextAsset>("Data/MonsterData");
        if (textAsset == null) return;

        // 1. 줄 분리
        string[] lines = textAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        // 2. 헤더 분석 (어떤 열이 어떤 스탯인지 미리 파악)
        string[] headers = lines[0].Split(',');
        
        // 헤더 인덱스 매핑 정보 생성
        var statColumnMap = new Dictionary<int, StatType>();
        var natureColumnMap = new Dictionary<int, NatureType>();
        var fieldColumnMap = new Dictionary<int, FieldInfo>(); // 일반 필드(ID, Name 등)

        FieldInfo[] fields = typeof(MonsterData).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i].Trim();

            // A. 일반 필드인지 확인 (ID, NameKR, MaxHP 등)
            FieldInfo field = Array.Find(fields, f => f.Name == header);
            if (field != null)
            {
                fieldColumnMap[i] = field;
                continue;
            }

            // B. StatType Enum인지 확인 (Body_Might 등)
            if (Enum.TryParse(header, out StatType statType))
            {
                statColumnMap[i] = statType;
                continue;
            }

            // C. NatureType Enum인지 확인 (Nature_Duty 등)
            if (Enum.TryParse(header, out NatureType natureType))
            {
                natureColumnMap[i] = natureType;
                continue;
            }
        }

        // 3. 데이터 파싱
        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            MonsterData data = new MonsterData();

            for (int col = 0; col < row.Length; col++)
            {
                if (col >= headers.Length) break;
                string value = row[col].Trim();
                if (string.IsNullOrEmpty(value)) continue;

                // 3-1. 일반 필드 매핑 (ID, HP 등)
                if (fieldColumnMap.TryGetValue(col, out FieldInfo field))
                {
                    try
                    {
                        if (field.FieldType == typeof(int)) field.SetValue(data, int.Parse(value));
                        else if (field.FieldType == typeof(float)) field.SetValue(data, float.Parse(value));
                        else if (field.FieldType == typeof(string)) field.SetValue(data, value);
                    }
                    catch { Debug.LogError($"[MonsterData] 필드 파싱 에러: {field.Name} 값={value}"); }
                }
                // 3-2. 스탯 딕셔너리 매핑
                else if (statColumnMap.TryGetValue(col, out StatType sType))
                {
                    if (int.TryParse(value, out int val))
                        data.BaseStats[sType] = val;
                }
                // 3-3. 성격 딕셔너리 매핑
                else if (natureColumnMap.TryGetValue(col, out NatureType nType))
                {
                    if (int.TryParse(value, out int val))
                        data.BaseNatures[nType] = val;
                }
            }

            // 4. 스킬 ID 문자열 별도 처리 (1001;1002 형태)
            if (!string.IsNullOrEmpty(data.SkillIDs_Str))
            {
                foreach (var idStr in data.SkillIDs_Str.Split(';'))
                {
                    if (int.TryParse(idStr.Trim(), out int skillId)) 
                        data.SkillIDs.Add(skillId);
                }
            }

            // 5. 딕셔너리 등록
            if (!MonsterDict.ContainsKey(data.ID))
                MonsterDict.Add(data.ID, data);
        }

        Debug.Log($"✅ [MonsterData] 총 {MonsterDict.Count}개 몬스터 로드 완료 (컬럼형).");
    }
}