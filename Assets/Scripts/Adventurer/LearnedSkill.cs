using UnityEngine;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;

[System.Serializable]
public class LearnedSkill
{
    [SerializeField, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
    public SkillSO _sourceSkill;
    
    [SerializeField]
    private int _level;

    [SerializeField] 
    private float _currentCooldown;
    
    public SkillSO SourceSkill => _sourceSkill;
    public  int Level => _level;
    public float CurrentCooldown => _currentCooldown;
    
    public LearnedSkill(SkillSO skillSO, int initialLevel = 1)
    {
        _sourceSkill = skillSO;
        _level = initialLevel;
        _currentCooldown = 0;
    }

    public LearnedSkill(LearnedSkill learnedSkill)
    {
        _sourceSkill = learnedSkill._sourceSkill;
        _level = learnedSkill._level;
        _currentCooldown = 0;
    }
  

    public int MaxCoolDown
    {
        get
        {
            float reduction = _sourceSkill.CooldownReductionPerLv * (_level - 1);
            int finalCd = Mathf.FloorToInt(_sourceSkill.Cooldown - reduction);
            return Mathf.Max(0,finalCd);
        }
    }

    public float[] GetCurrentPowerCoefs()
    {
        float[] currentCoefs = new float[_sourceSkill.PowerCoefs.Length];

        for (int i = 0; i < _sourceSkill.PowerCoefs.Length; i++)
        {
            float growth = (_sourceSkill.PowerGrowthPerLv != null && i<_sourceSkill.PowerGrowthPerLv.Length) ? _sourceSkill.PowerGrowthPerLv[i] : 0f;
            currentCoefs[i] = _sourceSkill.PowerCoefs[i] + (growth * (_level - 1));
                
        }

        return currentCoefs;
    }

    public void LevelUp()
    {
        _level++;
        // TODO : 플레이어의 모험가는 이펙트나 로그 출력
    }
    
    public bool IsReady(BattleUnit actor)
    {
        if (_currentCooldown > 0) return false;

        if (!actor.HasEnoughCost(_sourceSkill)) return false;
        
        // 상태이상 체크는 BattleUnit에서
        return true;
    }

    public void Use()
    {
        _currentCooldown = MaxCoolDown;
    }
    
    public void DecreaseCooldown(int amount = 1)
    {
        if (_currentCooldown > 0)
        {
            _currentCooldown -= amount;
            if (_currentCooldown < 0) _currentCooldown = 0;
        }
    }
}