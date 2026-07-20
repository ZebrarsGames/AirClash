using UnityEngine;


[CreateAssetMenu(menuName = "Daily Award Item")]
public class DailyAwardSO : ScriptableObject
{
    public AwardType AwardType;
    public int Day;
    public int Award;
    public string SkinAward;
    public Sprite AwardSprite;
}
