using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class ClickerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Elements")]
    [SerializeField] private Image skinImage;
    [SerializeField] private TextMeshProUGUI timesClickedText;
    [SerializeField] private RectTransform clickButton;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private Canvas mainCanvas;

    [Header("Animation Settings")]
    [SerializeField] private float scaleDuration = 0.1f;
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private Ease pressEase = Ease.OutQuad;
    [SerializeField] private Ease releaseEase = Ease.OutBack;

    [Header("Floating Text Settings")]
    [SerializeField] private TextMeshProUGUI textPrefab; 
    [SerializeField] private Transform textContainer;   
    [SerializeField] private float textDuration = 0.6f;
    [SerializeField] private float targetRadius = 360f;

    [Header("Scripts")]
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private AchievementsHandler achievementsHandler;

    private Tweener _rotatingTweener;
    private Tweener _scaleTweener;
    private int _clickCount = 0;
    private Vector3 _initialScale;

    private readonly List<TextMeshProUGUI> _textPool = new List<TextMeshProUGUI>();
    private readonly string[] ClicksAchievementsIds = { "click_1", "click_10", "click_100", "click_500", "click_1000", "click_3000", "click_7000", "click_10000", "click_50000", "click_100000" };

    private void Start()
    {
        if(saveManager != null) _clickCount = saveManager.GetData().TotalClicks;
        
        timesClickedText.SetText("Нажали {0} раз", _clickCount);
        
        if(clickButton != null)
        {
            _initialScale = clickButton.localScale;
            InitAnimation();
        }
        
        InitSkin(); 
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(clickButton == null) return;

        _scaleTweener?.Kill();
        _scaleTweener = clickButton.DOScale(_initialScale * pressedScale, scaleDuration)
            .SetEase(pressEase)
            .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(clickButton == null) return;

        _clickCount++;
        timesClickedText.SetText("Нажали {0} раз", _clickCount);
        UpdateClicksAchievements(1);

        if(soundManager != null) soundManager.PlayClickSound();

        SpawnFloatingText(eventData.position, "+1");

        _scaleTweener?.Kill();
        _scaleTweener = clickButton.DOScale(_initialScale, scaleDuration)
            .SetEase(releaseEase)
            .SetUpdate(true);
    }

    public int GetClickCount()
    {
        return _clickCount;
    }

    private void SpawnFloatingText(Vector2 screenPosition, string value)
    {
        if(textPrefab == null || textContainer == null || clickButton == null) return;

        TextMeshProUGUI floatingText = null;

        for(int i = 0; i < _textPool.Count; i++)
        {
            if(!_textPool[i].gameObject.activeSelf)
            {
                floatingText = _textPool[i];
                break;
            }
        }

        if(floatingText == null)
        {
            floatingText = Instantiate(textPrefab, textContainer);
            _textPool.Add(floatingText);
        }

        floatingText.gameObject.SetActive(true);
        floatingText.transform.localScale = Vector3.one;
        floatingText.SetText(value);

        if(!floatingText.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = floatingText.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;

        RectTransform containerRect = textContainer as RectTransform;
        if(containerRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, screenPosition, mainCanvas.worldCamera, out Vector2 spawnPointLocal);
        floatingText.rectTransform.anchoredPosition = spawnPointLocal;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, RectTransformUtility.WorldToScreenPoint(mainCanvas.worldCamera, clickButton.position), mainCanvas.worldCamera, out Vector2 buttonCenterLocal);

        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 targetOffset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * targetRadius;
        Vector2 finalTargetPosition = buttonCenterLocal + targetOffset;

        float randomZRotation = Random.Range(-20f, 20f);
        floatingText.transform.localRotation = Quaternion.Euler(0f, 0f, randomZRotation);

        Sequence textSeq = DOTween.Sequence();
        
        textSeq.Join(floatingText.rectTransform.DOLocalMove(finalTargetPosition, textDuration).SetEase(Ease.OutCubic));
        textSeq.Join(floatingText.transform.DOScale(1.3f, textDuration * 0.2f).SetEase(Ease.OutQuad)
            .OnComplete(() => floatingText.transform.DOScale(0.7f, textDuration * 0.8f).SetEase(Ease.InQuad)));
        textSeq.Insert(textDuration * 0.4f, canvasGroup.DOFade(0f, textDuration * 0.6f));
        
        textSeq.OnComplete(() => floatingText.gameObject.SetActive(false));
    }

    private void InitAnimation()
    {
        float duration = 360f / rotationSpeed; 
        clickButton.localRotation = Quaternion.identity;

        _rotatingTweener = clickButton.DOLocalRotate(new Vector3(0f, 0f, 360f), duration, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Normal, true);
    }

    private void InitSkin()
    {
        if(skinImage == null) return;

        string skinName = PlayerPrefs.GetString("CurrentSkin", "DefSkin");
        
        Sprite currentSkinSprite = Resources.Load<Sprite>($"SkinsImages/{skinName}");
        
        if(currentSkinSprite != null) skinImage.sprite = currentSkinSprite;
        else Debug.LogWarning($"Скин не найден по пути: Resources/SkinsImages/{skinName}");
    }

    private void OnDestroy()
    {
        _rotatingTweener?.Kill();
        _scaleTweener?.Kill();
        
        for(int i = 0; i < _textPool.Count; i++)
        {
            if(_textPool[i] != null) _textPool[i].transform.DOKill();
        }
    }

    private void UpdateClicksAchievements(int amount)
    {
        if(achievementsHandler != null)
        {
            foreach(var key in ClicksAchievementsIds)
                achievementsHandler.UpdateProgress(key, amount);
        }
    }
}
