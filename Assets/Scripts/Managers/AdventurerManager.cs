using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AdventurerManager : Singleton<AdventurerManager>
{
    [Title("Adventurer Class Data")]
    [ShowInInspector,ReadOnly]
    private Dictionary<int,ClassData> _classDataDict = new Dictionary<int, ClassData>();
    
    [Title("All Adventurers")]
    [ShowInInspector,ReadOnly]
    private Dictionary<int,Adventurer> _adventurers = new Dictionary<int, Adventurer>();

    public override void Initialize()
    {
        _classDataDict = DataManager.Instance.GetClassDict();
        AdventurerFactory.Instance.Initialize();
    }

    public bool ContainsKey(int id)
    {
        return _classDataDict.ContainsKey(id);
    }

    public ClassData GetClassData(int id)
    {
        return _classDataDict.TryGetValue(id,out ClassData classData)  ? classData : null;
    }

    public Dictionary<int, ClassData> GetClassDataDict()
    {
        return  _classDataDict;
    }

    public Adventurer GetAdventurer(int id)
    {
        return _adventurers.TryGetValue(id,out Adventurer adventurer) ? adventurer : null;
    }
}