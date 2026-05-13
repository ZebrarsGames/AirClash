using UnityEngine;

[CreateAssetMenu(menuName = "Common Quest")]
public class QuestSO : ScriptableObject
{
    public string QuestId;
    public string QuestName;
    public string Description;
    public string TypeOfAward;
    public int Award;
    public int Target;
}
