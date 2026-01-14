using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private Dictionary<int, ClassData> ClassDict = new Dictionary<int, ClassData>();
    private Dictionary<int, SkillData> SkillDict = new Dictionary<int, SkillData>();
    private Dictionary<int, MonsterData> MonsterDict = new Dictionary<int, MonsterData>();

    private Dictionary<DamageType, List<ResistanceFactor>> ResistanceRules =
        new Dictionary<DamageType, List<ResistanceFactor>>();

    private Dictionary<int, EffectConfigData> EffectConfigDict = new Dictionary<int, EffectConfigData>();
    private Dictionary<int, PropertyConfigData> PropertyConfigDict = new Dictionary<int, PropertyConfigData>();

    //[Manager]클래스 에서만 사용 
    public Dictionary<int,ClassData> GetClassDict() => ClassDict;
    public Dictionary<int, SkillData> GetSkillDict() => SkillDict;
    public Dictionary<int, MonsterData> GetMonsterDict() => MonsterDict;
    public Dictionary<DamageType, List<ResistanceFactor>> GetResistanceRules() => ResistanceRules;

    private void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    private void LoadAllData()
    {
        LoadClassData();
        LoadSkillData();
        LoadMonsterData();
        LoadResistanceData();
        LoadEffectConfig();
        LoadPropertyConfig();
    }

    private void LoadClassData()
    {
        LoadData("ClassData", ClassDict);
    }

    private void LoadSkillData()
    {
        LoadData("SkillData", SkillDict);
    }

    private void LoadMonsterData()
    {
        LoadData("MonsterData",MonsterDict);
    }

    private void LoadResistanceData()
    {
        ResistanceRules.Clear();

        TextAsset textAsset = Resources.Load<TextAsset>("Data/ResistanceConfig");
        if (textAsset == null)
        {
            Debug.LogError("ResistnaceConfig.csv 파일을 찾을 수 없습니다.");
            return;
        }

        string[] lines = textAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            if (row.Length < 3) continue;

            // 1. DamageType 파싱
            if (!Enum.TryParse(row[0].Trim(), out DamageType dmgType)) continue;

            // 2. Stat/Nature 파싱
            string typeName = row[1].Trim();
            float coef = float.Parse(row[2].Trim());

            ResistanceFactor factor = new ResistanceFactor();
            factor.Coefficient = coef;

            if (typeName.StartsWith("Nature_"))
            {
                // 성격 파싱
                if (Enum.TryParse(typeName, out NatureType nType))
                {
                    factor.IsNature = true;
                    factor.Nature = nType;
                }
            }
            else
            {
                if (Enum.TryParse(typeName, out StatType sType))
                {
                    factor.IsNature = false;
                    factor.Stat = sType;
                }
            }

            if (!ResistanceRules.ContainsKey(dmgType))
            {
                ResistanceRules[dmgType] = new List<ResistanceFactor>();
            }

            ResistanceRules[dmgType].Add(factor);
        }

        Debug.Log($"✅ [ResistanceData] 총 {ResistanceRules.Count}개 속성의 저항 공식 로드 완료.");
    }

    private void LoadEffectConfig()
    {
        LoadData("EffectConfig", EffectConfigDict);
    }

    private void LoadPropertyConfig()
    {
        LoadData("PropertyConfig", PropertyConfigDict);
    }

    private void LoadData<T>(string csvFileName, Dictionary<int, T> targetDict) where T : class, new()
    {
        targetDict.Clear();

        TextAsset textAsset = Resources.Load<TextAsset>($"Data/{csvFileName}");
        if (textAsset == null)
        {
            Debug.LogError($"[LoadData] {csvFileName} 파일을 찾을 수 없습니다.");
            return;
        }

        string[] lines = textAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        // 헤더 분석
        string[] headers = lines[0].Split(',');
        Dictionary<string, int> headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++) headerMap[headers[i].Trim()] = i;

        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            T data = new T();

            // ID를 저장할 변수 (나중에 딕셔너리 키로 사용)
            int dataID = 0;
            bool hasID = false;

            foreach (FieldInfo field in fields)
            {
                if (headerMap.TryGetValue(field.Name, out int index))
                {
                    if (index >= row.Length) continue;
                    string value = row[index].Trim();
                    if (string.IsNullOrEmpty(value)) continue;

                    try
                    {
                        object parsedValue = ParseValue(field.FieldType, value);
                        field.SetValue(data, parsedValue);
                        
                        if (field.Name.Equals("ID", StringComparison.OrdinalIgnoreCase))
                        {
                            dataID = (int)parsedValue;
                            hasID = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[{csvFileName}] 파싱 오류 필드:{field.Name} 값:{value} / {e.Message}");
                    }
                }
            }

            if (hasID && !targetDict.ContainsKey(dataID))
            {
                targetDict.Add(dataID, data);
            }
        }

        Debug.Log($"✅ [{typeof(T).Name}] {targetDict.Count}개 로드 완료.");
    }

    private object ParseValue(Type type, string value)
    {
        if (type == typeof(int)) return int.Parse(value);
        if (type == typeof(float)) return float.Parse(value);
        if (type == typeof(string)) return value;
        if (type.IsEnum) return Enum.Parse(type, value);

        if (type.IsArray)
        {
            string[] arrayValues = value.Split('+');
            Type elementType = type.GetElementType();

            if (elementType == typeof(int))
                return Array.ConvertAll(arrayValues, int.Parse);
            if (elementType == typeof(float))
                return Array.ConvertAll(arrayValues, float.Parse);
            if (elementType == typeof(string))
                return arrayValues;
            if (elementType.IsEnum)
            {
                Array enumArr = Array.CreateInstance(elementType, arrayValues.Length);
                for (int k = 0; k < arrayValues.Length; k++)
                    enumArr.SetValue(Enum.Parse(elementType, arrayValues[k].Trim()), k);
                return enumArr;
            }
        }

        return null;
    }
}