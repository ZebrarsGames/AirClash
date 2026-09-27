using UnityEngine;
using TMPro;
using DG.Tweening;

public class EloTextAnimScr : MonoBehaviour
{
    [SerializeField] private TMP_Text eloText;

    [Header("Time Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float countDuration = 0.7f;

    [Header("Offset Settings")]
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

        UpdateEloTextString(oldRating, newRating, countingDifference);

        eloSequence = DOTween.Sequence();

        eloSequence.Append(DOTween.To(() => currentOffset, x => currentOffset = x, 0f, slideDuration).SetEase(Ease.OutCubic));
        eloSequence.Join(DOTween.To(() => currentAlpha, x => currentAlpha = x, 1f, slideDuration).SetEase(Ease.OutCubic));

        eloSequence.OnUpdate(() => UpdateEloTextString(oldRating, newRating, countingDifference));

        eloSequence.Append(DOTween.To(() => countingDifference, x => countingDifference = x, targetDifference, countDuration)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => UpdateEloTextString(oldRating, newRating, countingDifference)));
            
        eloSequence.OnComplete(() => {
            countingDifference = targetDifference;
            currentAlpha = 1f;
            currentOffset = 0f;
            UpdateEloTextString(oldRating, newRating, countingDifference);
        });
    }

    private void UpdateEloTextString(int oldRating, int finalNewRating, int currentDiff)
    {
        int offsetInt = (int)currentOffset;
        
        int alphaByte = (int)(currentAlpha * 255f); 
        string alphaHex = alphaByte.ToString("X2");
        
        string diffStr = currentDiff.ToString("+#;-#;+0"); 

        eloText.text = $"Ваш новый эло: {finalNewRating}<voffset={offsetInt}px><color=#c9c9c9><alpha=#{alphaHex}>={oldRating}{diffStr}</color></voffset>";
    }

    private void OnDestroy()
    {
        eloSequence?.Kill();
    }
}