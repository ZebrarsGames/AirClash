using UnityEngine;


public enum AwardType
{
    Money,
    Xp,
    Skin
}
[CreateAssetMenu(menuName = "Common Quest")]
public class QuestSO : ScriptableObject
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
