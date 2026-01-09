using UnityEngine;

public class GameBootstrapper
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
        
        // 프리펩 넣고
        
        // 초기화 
    }
}
