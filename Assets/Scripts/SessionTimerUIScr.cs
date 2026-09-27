using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

[DisallowMultipleComponent]
public sealed class SessionTimerUIScr : MonoBehaviour
{
    private readonly struct WarningMessage
    {
        public readonly string Title;
        public readonly string Body;

        public WarningMessage(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }

    [Header("Panels")]
    [SerializeField] private RectTransform warningPanel;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI warningTextTitle;
    [SerializeField] private TextMeshProUGUI warningTextBody;

    [Header("Animation Settings")]
    [SerializeField] private Vector2 targetAnchorPosition = new Vector2(0, -90f);
    [SerializeField] private float animationDuration = 2.0f;
    [SerializeField] private float displayDuration = 5.0f;

    private Vector2 _startPosAchievementPanel;
    private Tween _showTween;
    private Tween _hideTween;
    private Sequence _animationSequence;

    private readonly Dictionary<int, WarningMessage[]> _warnings = new Dictionary<int, WarningMessage[]>
    {
        { 10, new[]
            {
                new WarningMessage("10 минут игры!", "Ого, уже целых 10 минут в игре!"),
                new WarningMessage("10 минут игры!", "Время летит незаметно, первая десяточка!"),
                new WarningMessage("10 минут игры!", "10 минут позади, полет нормальный."),
                new WarningMessage("10 минут игры!", "Не забывайте моргать, игра идет уже 10 минут.")
            }
        },
        { 30, new[]
            {
                new WarningMessage("30 минут игры!", "Не пора ли сделать перерыв?"),
                new WarningMessage("30 минут игры!", "Самое время для разминки."),
                new WarningMessage("30 минут игры!", "Полчаса пролетело! Сделайте глубокий вдох.")
            }
        },
        { 60, new[]
            {
                new WarningMessage("60 минут игры!", "Может, сделаем разминку?"),
                new WarningMessage("60 минут игры!", "Пора ненадолго отвлечься от игры."),
                new WarningMessage("60 минут игры!", "Уже целый час! Встаньте и потянитесь.")
            }
        },
        { 90, new[]
            {
                new WarningMessage("90 минут игры!", "Самое время пойти отдохнуть и попить чай."),
                new WarningMessage("90 минут игры!", "Может, немного чаю?"),
                new WarningMessage("90 минут игры!", "Полтора часа - отличный повод сделать паузу.")
            }
        },
        { 120, new[]
            {
                new WarningMessage("120 минут игры!", "Не пора ли выйти на улицу и подышать воздухом?"),
                new WarningMessage("120 минут игры!", "На улице такая хорошая погода, может выйти?"),
                new WarningMessage("120 минут игры!", "Два часа у экрана! Глазам нужен отдых.")
            }
        },
        { 150, new[]
            {
                new WarningMessage("150 минут игры!", "Ваши глаза явно не скажут вам спасибо, поэтому может отдохнуть?"),
                new WarningMessage("150 минут игры!", "У вас не болят глаза? Может перерыв?"),
                new WarningMessage("150 минут игры!", "Сделайте перерыв, посмотрите в окно пару минут.")
            }
        },
        { 180, new[]
            {
                new WarningMessage("180 минут игры!", "Время выйти на улицу и потрогать траву."),
                new WarningMessage("180 минут игры!", "На улице есть трава и можно её потрогать."),
                new WarningMessage("180 минут игры!", "Три часа! Это уже серьезная игровая сессия, пора отдохнуть.")
            }
        },
        { 210, new[]
            {
                new WarningMessage("210 минут игры!", "Эй, такая сессия может вызвать проблемы со здоровьем, может уже надо наконец-то выключить телефон?"),
                new WarningMessage("210 минут игры!", "Такая сессия вызывает проблемы со здоровьем, не пора ли уже отдохнуть?"),
                new WarningMessage("210 минут игры!", "Пожалуйста, отложите устройство. Ваше здоровье важнее игры.")
            }
        }
    };

    // private void Awake()
    // {
    //     _startPosAchievementPanel = warningPanel.anchoredPosition;
    //     warningPanel.gameObject.SetActive(false);
    // }

    private void Start()
    {
        _startPosAchievementPanel = new Vector2(0, 80);
        if(SessionTimer.Instance != null)
        {
            SessionTimer.Instance.OnMinuteChanged += ShowWarning;
        }
    }

    private void OnDestroy()
    {
        if(SessionTimer.Instance != null)
        {
            SessionTimer.Instance.OnMinuteChanged -= ShowWarning;
        }

        _animationSequence?.Kill();
    }

    public void ShowWarning(int minutes)
    {
        if(!_warnings.TryGetValue(minutes, out WarningMessage[] messageArray) || messageArray.Length == 0) 
            return;

        int randomIndex = UnityEngine.Random.Range(0, messageArray.Length);
        WarningMessage randomMessage = messageArray[randomIndex];

        warningTextTitle.text = randomMessage.Title;
        warningTextBody.text = randomMessage.Body;

        AnimateWarningPanel();
    }

    private void AnimateWarningPanel()
    {
        _animationSequence?.Kill(false);

        warningPanel.gameObject.SetActive(true);

        _animationSequence = DOTween.Sequence();

        _showTween = warningPanel.DOAnchorPos(targetAnchorPosition, animationDuration);
        _hideTween = warningPanel.DOAnchorPos(_startPosAchievementPanel, animationDuration)
                                  .OnComplete(() => warningPanel.gameObject.SetActive(false));

        _animationSequence.Append(_showTween)
                          .AppendInterval(displayDuration)
                          .Append(_hideTween)
                          .SetLink(warningPanel.gameObject);
    }
}