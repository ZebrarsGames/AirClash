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
                new WarningMessage("10 минут игры!", "Не забывайте моргать, игра идет уже 10 минут."),
                new WarningMessage("10 минут игры!", "10 минут пролетели. Разминочная сессия завершена успешно."),
                new WarningMessage("10 минут игры!", "Вы в игре уже 10 минут. Чат пока спокойный? Отлично!"),
                new WarningMessage("10 минут игры!", "Первая десятка позади. Надеемся, вы успели настроить управление под себя.")
            }
        },
        { 30, new[]
            {
                new WarningMessage("30 минут игры!", "Не пора ли сделать перерыв?"),
                new WarningMessage("30 минут игры!", "Самое время для разминки."),
                new WarningMessage("30 минут игры!", "Полчаса пролетело! Сделайте глубокий вдох."),
                new WarningMessage("30 минут игры!", "Полчаса у экрана! Время летит быстрее, чем кастуется любой ультимейт."),
                new WarningMessage("30 минут игры!", "30 минут сессии. Твой скилл растет, но глазам все же нужен брейк!"),
                new WarningMessage("30 минут игры!", "Полчаса в игре! Сделайте паузу хотя бы на 10 секунд, чтобы перевести дух."),
                new WarningMessage("30 минут игры!", "Вы играете 30 минут. Проверьте осанку, а то спина спасибо не скажет.")
            }
        },
        { 60, new[]
            {
                new WarningMessage("60 минут игры!", "Может, сделаем разминку?"),
                new WarningMessage("60 минут игры!", "Пора ненадолго отвлечься от игры."),
                new WarningMessage("60 минут игры!", "Уже целый час! Встаньте и потянитесь."),
                new WarningMessage("60 минут игры!", "Час в игре! Скорость твоих реакций поражает, но пора поберечь суставы."),
                new WarningMessage("60 минут игры!", "Достижение разблокировано: 'Час у экрана'. Награды нет, но вы разомнитесь!"),
                new WarningMessage("60 минут игры!", "60 минут непрерывных матчей. Встряхните кисти рук, они вам еще пригодятся."),
                new WarningMessage("60 минут игры!", "Целый час! Защищайте свое здоровье в реальной жизни - сходите попейте воды.")
            }
        },
        { 90, new[]
            {
                new WarningMessage("90 минут игры!", "Самое время пойти отдохнуть и попить чай."),
                new WarningMessage("90 минут игры!", "Может, немного чаю?"),
                new WarningMessage("90 минут игры!", "Полтора часа - отличный повод сделать паузу."),
                new WarningMessage("90 минут игры!", "Полтора часа бешеного темпа. Игра кипит, экран горит, чаю бы!"),
                new WarningMessage("90 минут игры!", "90 минут! Глаза уже должны читать ходы соперников вслепую. Дай им отдых."),
                new WarningMessage("90 минут игры!", "Полтора часа у экрана. Если сидеть в позе креветки, тащить катки будет сложнее!"),
                new WarningMessage("90 минут игры!", "Вы рубитесь уже 90 минут. Сделайте тайм-аут, переведите дух.")
            }
        },
        { 120, new[]
            {
                new WarningMessage("120 минут игры!", "Не пора ли выйти на улицу и подышать воздухом?"),
                new WarningMessage("120 минут игры!", "На улице такая хорошая погода, может выйти?"),
                new WarningMessage("120 минут игры!", "Два часа у экрана! Глазам нужен отдых."),
                new WarningMessage("120 минут игры!", "Два часа! Реальный мир начинает забывать ваше лицо. Напишите маме."),
                new WarningMessage("120 минут игры!", "Вы играете 120 минут. Напоминаем: в реальной жизни графика тоже ничего, и там есть текстуры неба."),
                new WarningMessage("120 минут игры!", "2 часа сессии. Твоя реакция сейчас быстрее звука, но глаза просят пощады."),
                new WarningMessage("120 минут игры!", "Два часа подряд! Сделайте 10 приседаний, разомните спину после такой плотной игровой сессии.")
            }
        },
        { 150, new[]
            {
                new WarningMessage("150 минут игры!", "Ваши глаза явно не скажут вам спасибо, поэтому может отдохнуть?"),
                new WarningMessage("150 минут игры!", "У вас не болят глаза? Может перерыв?"),
                new WarningMessage("150 минут игры!", "Сделайте перерыв, посмотрите в окно пару минут."),
                new WarningMessage("150 минут игры!", "150 минут! Ваши глаза сейчас выглядят как финальные боссы — очень красные."),
                new WarningMessage("150 минут игры!", "Два с половиной часа. Моргните два раза, если вам нужна помощь. И один раз, чтобы просто увлажнить глаза."),
                new WarningMessage("150 минут игры!", "150 минут марафона. Сфокусируйте взгляд на чем-то дальше вашего монитора."),
                new WarningMessage("150 минут игры!", "Вы тут уже два с половиной часа. Сделайте глубокий вдох и отложите устройство.")
            }
        },
        { 180, new[]
            {
                new WarningMessage("180 минут игры!", "Время выйти на улицу и потрогать траву."),
                new WarningMessage("180 минут игры!", "На улице есть трава и можно её потрогать."),
                new WarningMessage("180 минут игры!", "Три часа! Это уже серьезная игровая сессия, пора отдохнуть."),
                new WarningMessage("180 минут игры!", "3 часа! Трава на улице соскучилась по вашим прикосновениям. Серьезно, потрогайте её."),
                new WarningMessage("180 минут игры!", "Три часа в игре! Ваше устройство заслужило покой, а спина вышла из чата."),
                new WarningMessage("180 минут игры!", "180 минут! Осторожно, при выходе из игры реальный мир покажется слишком медленным."),
                new WarningMessage("180 минут игры!", "Три часа безумного онлайна. Устройте себе заслуженный спа-день без экранов.")
            }
        },
        { 210, new[]
            {
                new WarningMessage("210 минут игры!", "Эй, такая сессия может вызвать проблемы со здоровьем, может уже надо наконец-то выключить телефон?"),
                new WarningMessage("210 минут игры!", "Такая сессия вызывает проблемы со здоровьем, не пора ли уже отдохнуть?"),
                new WarningMessage("210 минут игры!", "Пожалуйста, отложите устройство. Ваше здоровье важнее игры."),
                new WarningMessage("210 минут игры!", "3.5 часа! Ваше железо уже горячее, чем лава в финальной локации. Поберегите технику!"),
                new WarningMessage("210 минут игры!", "210 минут! Игра никуда не убежит, а вот здоровье и зрение — могут. Отдохните!"),
                new WarningMessage("210 минут игры!", "Три с половиной часа гейминга. Пожалуйста, выпейте нормальной еды или воды вместо виртуальных побед."),
                new WarningMessage("210 минут игры!", "Вы играете 210 минут. Пора сделать перерыв ради вашего же блага!")
            }
        },
        { 270, new[]
            {
                new WarningMessage("270 минут игры!", "Критическая перегрузка! Вы играете 4.5 часа. Пожалуйста, выключите игру."),
                new WarningMessage("270 минут игры!", "4 часа 30 минут у экрана. Вы вообще человек или ИИ максимального уровня сложности?"),
                new WarningMessage("270 минут игры!", "Вы играете 270 минут подряд. Даже виртуальные текстуры уже стерлись. Сделайте паузу!"),
                new WarningMessage("270 минут игры!", "Внимание! 4.5 часа непрерывных сессий. Немедленно закройте приложение и отойдите от экрана."),
                new WarningMessage("270 минут игры!", "270 минут сессии! Глаза должны быть стерты до пикселей. Объявляется принудительный отдых!"),
                new WarningMessage("270 минут игры!", "Вы в игре уже 4.5 часа. Любое киберспортивное событие длится меньше! Пора спасать зрение."),
                new WarningMessage("270 минут игры!", "270 минут! Вы побили все рекорды усидчивости. А теперь марш спать, гулять или обедать!")
            }
        }
    };

    private void Awake()
    {
        _startPosAchievementPanel = warningPanel.anchoredPosition;
        warningPanel.gameObject.SetActive(false);
    }

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
        VibrationHandler.Vibrate(500, 255);
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