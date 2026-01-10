using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    public DataManager DataManagerPrefab;
    public BattleManager BattleManagerPrefab;
    public AdventurerManager AdventurerManagerPrefab;
    public PartyManager PartyManagerPrefab;
    public ResistanceManager ResistanceManagerPrefab;
    public SkillManager SkillManagerPrefab;
    
    public void Awake()
    {
        Debug.Log("[Bootstrapper] 게임 초기화------");
        
        if (DataManagerPrefab == null) Instantiate(DataManagerPrefab);
        if (AdventurerManagerPrefab == null) Instantiate(AdventurerManagerPrefab);
        if (ResistanceManagerPrefab == null) Instantiate(ResistanceManagerPrefab);
        if (SkillManagerPrefab == null) Instantiate(SkillManagerPrefab);
        
        DataManager.Instance.Initialize(); 

        AdventurerManager.Instance.Initialize();
        SkillManager.Instance.Initialize();
        ResistanceManager.Instance.Initialize();
        
    }
}
