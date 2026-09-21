using UnityEngine;
using TMPro;
using DG.Tweening;

public class EloTextAnimScr : MonoBehaviour
{
    [SerializeField] private TMP_Text eloText;

    [Header("Настройки времени")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float countDuration = 0.7f;

    [Header("Настройки выезда")]
    [SerializeField] private float startVerticalOffset = -20f;

    private float currentAlpha = 0f;
    private float currentOffset = 0f;
    private int countingDifference = 0;
    
    private Sequence eloSequence; 

    public void AnimateElo(int oldRating, int newRating, int targetDifference)
    {
        eloSequence?.Kill();

        currentAlpha = 0f;
        currentOffset = startVerticalOffset;
        countingDifference = 0;

        UpdateEloTextString(oldRating, countingDifference);

        eloSequence = DOTween.Sequence();

        eloSequence.Append(DOTween.To(() => currentOffset, x => currentOffset = x, 0f, slideDuration).SetEase(Ease.OutCubic));
        eloSequence.Join(DOTween.To(() => currentAlpha, x => currentAlpha = x, 1f, slideDuration).SetEase(Ease.OutCubic));

        eloSequence.OnUpdate(() => UpdateEloTextString(oldRating, countingDifference));

        eloSequence.Append(DOTween.To(() => countingDifference, x => countingDifference = x, targetDifference, countDuration)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => UpdateEloTextString(oldRating, countingDifference)));
            
        eloSequence.OnComplete(() => {
            countingDifference = targetDifference;
            currentAlpha = 1f;
            currentOffset = 0f;
            UpdateEloTextString(oldRating, countingDifference);
        });
    }

    private void UpdateEloTextString(int oldRating, int diff)
    {
        int offsetInt = (int)currentOffset;
        int alphaPercent = (int)(currentAlpha * 70);
        
        int currentElo = oldRating + diff;

        eloText.SetText("Ваш новый эло: {0}<voffset={1}px><color=#c9c9c9><alpha=#{2:00}>={3}{4:+#;-#;0}</color></voffset>", 
            currentElo, 
            offsetInt, 
            alphaPercent, 
            oldRating, 
            diff);
    }

    private void OnDestroy()
    {
        eloSequence?.Kill();
    }
}