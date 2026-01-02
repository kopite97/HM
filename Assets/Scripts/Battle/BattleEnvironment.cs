using System.Collections.Generic;

[System.Serializable]
public class BattleEnvironment
{
    // 현재 적용 중인 환경 태그들
    public List<DungeonTag> CurrentTags = new List<DungeonTag>();
    
    // 날씨나 환경의 강도 (0.0 ~ 1.0)
    public float WeatherIntensity; 

    public BattleEnvironment()
    {
        CurrentTags = new List<DungeonTag>();
        WeatherIntensity = 0f;
    }

    /// <summary>
    /// 특정 태그가 활성화되어 있는지 확인
    /// </summary>
    public bool ContainsTag(DungeonTag tag)
    {
        return CurrentTags.Contains(tag);
    }

    /// <summary>
    /// 태그 추가
    /// </summary>
    public void AddTag(DungeonTag tag)
    {
        if (!CurrentTags.Contains(tag))
            CurrentTags.Add(tag);
    }

    /// <summary>
    /// 태그 제거
    /// </summary>
    public void RemoveTag(DungeonTag tag)
    {
        if (CurrentTags.Contains(tag))
            CurrentTags.Remove(tag);
    }
}