using UnityEngine;

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
    public Sprite QuestLogo;
}
