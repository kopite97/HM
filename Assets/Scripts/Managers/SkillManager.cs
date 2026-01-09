using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    
    [Title("Data Storage")]
    [ShowInInspector,ReadOnly]
    private Dictionary<int,SkillSO> _skillDict =  new Dictionary<int, SkillSO>();

    [Title("Settings")] [SerializeField, FolderPath]
    private string resourcePath = "/SO/Skill";
    

    public void SetAllSkillData(Dictionary<int,SkillData> skillCSVDatas)
    {
        _skillDict.Clear();
        
        SkillSO[] allSkills = Resources.LoadAll<SkillSO>(resourcePath);

        if (allSkills.Length == 0)
        {
            Debug.LogWarning($"[SkillManager] '{resourcePath}'경로에서 Skill을 찾을 수 없습니다.");
            return;
        }

        foreach (var skill in allSkills)
        {
            if (skillCSVDatas.TryGetValue(skill.ID, out SkillData data))
            {
                skill.SetData(data);
                _skillDict.Add(skill.ID, skill);
            }
            else
            {
                Debug.LogWarning($"[SkillManager] SO 파일(ID:{skill.ID}, {skill.name})은 존재하지만, CSV에 해당 ID 데이터가 없습니다.");
            }
        }

        // CSV에는 있는데 SO 파일이 없는 경우 처리
        // 일단 테스트용으로 임의로 만들어 뒀음.
        foreach (var kvp in skillCSVDatas)
        {
            if (!_skillDict.ContainsKey(kvp.Key))
            {
                SkillSO newRuntimeSO = ScriptableObject.CreateInstance<SkillSO>();
                
                newRuntimeSO.ID = kvp.Key;
                newRuntimeSO.SetData(kvp.Value);
                newRuntimeSO.name = $"RuntimeSkill_{kvp.Key}";
                Debug.Log($"[SkillManager] CSV에는 있지만 파일이 없는 스킬(ID:{kvp.Key})을 런타임 인스턴스로 생성했습니다.");
            }
        }
        
        Debug.Log("[SkillManager] 데이터 동기화 완료.");
    }

    public Dictionary<int, SkillSO> GetSkillDictionary()
    {
        return  _skillDict;
    }

    public bool TryGetSkill(int id,out SkillSO skill)
    {
        return _skillDict.TryGetValue(id, out skill);
    }

    public bool HasSkill(int id)
    {
        return _skillDict.ContainsKey(id);
    }
}