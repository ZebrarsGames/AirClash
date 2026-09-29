using System.Text;
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
    private int targetDiff = 0;

    private Sequence eloSequence;
    
    private readonly StringBuilder textBuilder = new StringBuilder(128);

    public void StartTestAnim1()
    {
        AnimateElo(450, 526, 76);
    }

    public void StartTestAnim2()
    {
        AnimateElo(526, 450, -76);
    }

    public void AnimateElo(int oldRating, int newRating, int targetDifference)
    {
        eloSequence?.Kill();

        currentAlpha = 0f;
        currentOffset = startVerticalOffset;
        countingDifference = 0;
        targetDiff = targetDifference;

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

        textBuilder.Clear();

        textBuilder.Append("Ваш новый эло: ").Append(finalNewRating);
        textBuilder.Append("<voffset=").Append(offsetInt).Append("px>");
        textBuilder.Append("<color=#c9c9c9><alpha=#");
        
        AppendByteAsHex(textBuilder, alphaByte);
        
        textBuilder.Append(">=").Append(oldRating);

        if(currentDiff > 0)
        {
            textBuilder.Append('+').Append(currentDiff);
        }
        else if(currentDiff < 0)
        {
            textBuilder.Append(currentDiff);
        }
        else
        {
            textBuilder.Append(targetDiff < 0 ? "-0" : "+0");
        }

        textBuilder.Append("</color></voffset>");

        eloText.SetText(textBuilder);
    }

    private static void AppendByteAsHex(StringBuilder sb, int value)
    {
        const string hexDigits = "0123456789ABCDEF";
        sb.Append(hexDigits[(value >> 4) & 0xF]);
        sb.Append(hexDigits[value & 0xF]);
    }

    private void OnDestroy()
    {
        eloSequence?.Kill();
    }
}