using UnityEngine;

public enum DailyQuestSeries
{
    money,
    xp,
    win,
    goal,
    roulette
}

[CreateAssetMenu(menuName = "Daily Quest")]
public class DailyQuestSO : ScriptableObject
{
    public string QuestId;
    public string QuestName;
    public string Description;
    public AwardType AwardType;
    public int Award;
    public string SkinAward;
    public int Target;
    public DailyQuestSeries DailyQuestSeries;
    public Sprite QuestLogo;
}
