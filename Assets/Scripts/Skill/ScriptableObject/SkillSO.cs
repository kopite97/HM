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
   [ReadOnly] public string Name;
    
   [TabGroup("Sync", "Basic Info")]
   [ReadOnly, TextArea(4, 10)] public string Description;

   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly] public SkillType Type;
    
   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly] public SkillTarget Target;
    
   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly] public DamageType DamageType;

   [TabGroup("Sync", "Combat Stats")]
   [ReadOnly, SuffixLabel("sec")] public float Cooldown;

   [TabGroup("Sync", "Resource")]
   [ReadOnly] public SkillResourceType ResourceType;
    
   [TabGroup("Sync", "Resource")]
   [ReadOnly] public int ResourceCost;

   [TabGroup("Sync", "Combat Stats")] [ReadOnly]
   public float Range;

   // 복잡한 배열 데이터는 접이식 그룹으로 관리
   [TabGroup("Sync", "Details")]
   [FoldoutGroup("Sync/Details/Modifiers")]
   [ReadOnly]
   public SkillModifier[] Modifiers;
   

   [TabGroup("Sync", "Details")]
   [FoldoutGroup("Sync/Details/Actions")]
   [ReadOnly] public SkillAction[] Actions;

   [TabGroup("Sync", "Details")]
   [FoldoutGroup("Sync/Details/Status Effects")]
   [ReadOnly] public StatusEffect[] StatusEffects;
   
   [TabGroup("Sync", "Details")]
   [FoldoutGroup("Sync/Details/Traits")]
   [ReadOnly] public Trait[] Traits;

   [TabGroup("Sync", "Growth")]
   [LabelText("Base Stats (Lv.1)")]
   [ReadOnly] public StatType[] BaseStats;

   [TabGroup("Sync", "Growth")]
   [LabelText("Power Coefs")]
   [InfoBox("데미지 계산 공식에 사용되는 계수입니다.")]
   [ReadOnly] public float[] PowerCoefs;

   [TabGroup("Sync", "Growth")]
   [LabelText("Cooldown Reduction / Lv")]
   [ReadOnly] public float CooldownReductionPerLv;

   [TabGroup("Sync", "Growth")]
   [LabelText("Power Growth / Lv")]
   [ReadOnly] public float[] PowerGrowthPerLv;
   public void SetData(SkillData data)
   {
      Name = data.NameKR; // TODO 나중에 언어 설정 바꾸기
      Description = data.DescKR; // TODO 얘도 바꾸기
      Type = data.Type;
      Target = data.Target;
      DamageType = data.Damage_Type;
      Cooldown = data.Cooldown;
      ResourceType = data.Resource_Type;
      ResourceCost = data.Resource;
      Range = data.Range;

      Modifiers = data.Skill_Modifiers;
      Actions = data.Skill_Actions;
      StatusEffects = data.Status_Effects;
      BaseStats = data.Base_Stats;
      PowerCoefs = data.Power_Coefs;
      Traits = data.Traits;

      CooldownReductionPerLv = data.Cooldown_Reduction;
      PowerGrowthPerLv = data.Power_Growth;
   }
}
