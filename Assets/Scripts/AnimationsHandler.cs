using DG.Tweening;
using TMPro;
using UnityEngine;

public class AchievementPopup : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Animation Settings")]
    [SerializeField] private Vector2 targetAnchoredPosition;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InQuad;

    private Vector2 _initialAnchoredPosition;
    private Sequence _animationSequence;

    private void Awake()
    {
        _initialAnchoredPosition = panelTransform.anchoredPosition;
        panelTransform.gameObject.SetActive(false);
    }

    public void ShowAchievement(string achievementTitle)
    {
        if(string.IsNullOrWhiteSpace(achievementTitle)) return;

        _animationSequence?.Kill(complete: false);

        titleText.text = achievementTitle;

        VibrationHandler.Vibrate(250, 30);

        _animationSequence = DOTween.Sequence()
            .Append(panelTransform.DOAnchorPos(targetAnchoredPosition, moveDuration).SetEase(showEase))
            .AppendInterval(displayDuration)
            .Append(panelTransform.DOAnchorPos(_initialAnchoredPosition, moveDuration).SetEase(hideEase))
            .OnComplete(() => panelTransform.gameObject.SetActive(false) )
            .SetLink(gameObject);
    }

    private void OnDestroy()
    {
        _animationSequence?.Kill();
    }
}