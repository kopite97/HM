using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "new SKill",menuName = "SO/Skill/SkillSO")]
public class SkillSO : SerializedScriptableObject
{
   // 리소스
   [Title("식별자")] [GUIColor(0.3f, 1f, 0.3f)]
   [PropertyOrder(-1)]
   public int ID;

   [HorizontalGroup("Split", Width = 120)]
   [PreviewField(120, ObjectFieldAlignment.Left), HideLabel]
   [AssetsOnly] // 씬 오브젝트 참조 방지
   public Sprite Icon;
   
   [VerticalGroup("Split/Right")]
   [BoxGroup("Split/Right/Prefab Info")]
   [LabelText("Effect Prefab")]
   [AssetsOnly]
   public GameObject effectPrefab;
   
   [VerticalGroup("Split/Right")]
   [BoxGroup("Split/Right/Prefab Info")]
   [LabelText("Hit Sound")]
   public AudioClip hitSound; 
   
   
   // 동기화 데이터
   [Title("Synced Data (From CSV)", "게임 시작 시 자동으로 덮어씌워집니다")]
   
   [TabGroup("Sync", "Basic Info")]
   [ReadOnly] public string skillName;
    
   [TabGroup("Sync", "Basic Info")]
   [ReadOnly, TextArea(4, 10)] public string description;

   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly] public SkillType type;
    
   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly] public SkillTarget target;
    
   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly] public DamageType damageType;

   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly, SuffixLabel("sec")] public float cooldown;

   [TabGroup("Sync", "Resource")]
   [ReadOnly] public SkillResourceType resourceType;
    
   [TabGroup("Sync", "Resource")]
   [ReadOnly] public int resourceCost;

   // 복잡한 배열 데이터는 접이식 그룹으로 관리
   [TabGroup("Sync", "Details")]
   [FoldoutGroup("Sync/Details/Modifiers")]
   [ReadOnly] public SkillModifier[] modifiers;

   [TabGroup("Sync", "Details")]
   [FoldoutGroup("Sync/Details/Actions")]
   [ReadOnly] public SkillAction[] actions;

   [TabGroup("Sync", "Details")]
   [FoldoutGroup("Sync/Details/Status Effects")]
   [ReadOnly] public StatusEffect[] statusEffects;

   [TabGroup("Sync", "Growth")]
   [LabelText("Base Stats (Lv.1)")]
   [ReadOnly] public StatType[] baseStats;

   [TabGroup("Sync", "Growth")]
   [LabelText("Power Coefs")]
   [InfoBox("데미지 계산 공식에 사용되는 계수입니다.")]
   [ReadOnly] public float[] powerCoefs;

   [TabGroup("Sync", "Growth")]
   [LabelText("Cooldown Reduction / Lv")]
   [ReadOnly] public float cooldownReductionPerLv;

   [TabGroup("Sync", "Growth")]
   [LabelText("Power Growth / Lv")]
   [ReadOnly] public float[] powerGrowthPerLv;

   public void SetData(SkillData data)
   {
      skillName = data.NameKR; // TODO 나중에 언어 설정 바꾸기
      description = data.DescKR; // TODO 얘도 바꾸기
      type = data.Type;
      target = data.Target;
      damageType = data.Damage_Type;
      cooldown = data.Cooldown;
      resourceType = data.Resource_Type;
      resourceCost = data.Resource;

      modifiers = data.Skill_Modifiers;
      actions = data.Skill_Actions;
      statusEffects = data.Status_Effects;
      baseStats = data.Base_Stats;
      powerCoefs = data.Power_Coefs;

      cooldownReductionPerLv = data.Cooldown_Reduction;
      powerGrowthPerLv = data.Power_Growth;
   }
}
