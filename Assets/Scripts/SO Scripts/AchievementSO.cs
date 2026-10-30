using UnityEngine;

[CreateAssetMenu(fileName = "NewAchievement")]
public class AchievementSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string title;
    [SerializeField] private string description;
    [SerializeField] private int target;
    [SerializeField] private int award;
    [SerializeField] private Sprite icon;

    public string Id => id;
    public string Title => title;
    public string Description => description;
    public int Target => target;
    public int Award => award;
    public Sprite Icon => icon;
}