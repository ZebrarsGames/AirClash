using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SessionTimerUIScr : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject warningPanel;

    [Header("Texts")]
    [SerializeField] private Text warningTextTitle;
    [SerializeField] private Text warningTextBody;

    private Vector2 startPosAchievementPanel;
    private Coroutine animationCoroutine; 

    void Awake()
    {
        startPosAchievementPanel = warningPanel.GetComponent<RectTransform>().anchoredPosition;
    }

    void Start()
    {
        if(SessionTimer.Instance != null)
        {
            SessionTimer.Instance.onMinuteChanged.AddListener(ShowWarning);
        }
    }

    void OnDestroy()
    {
        if(SessionTimer.Instance != null)
        {
            SessionTimer.Instance.onMinuteChanged.RemoveListener(ShowWarning);
        }
    }

    private struct WarningMessage
    {
        public string Title;
        public string Body;

        public WarningMessage(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }

    private readonly Dictionary<int, WarningMessage> warnings = new Dictionary<int, WarningMessage>
    {
        { 30,  new WarningMessage("30 минут игры!", "Не пора ли сделать перерыв?") },
        { 60,  new WarningMessage("60 минут игры!", "Может, сделаем разминку?") },
        { 90,  new WarningMessage("90 минут игры!", "Самое время пойти отдохнуть и попить чай.") },
        { 120, new WarningMessage("120 минут игры!", "Не пора ли выйти на улицу и подышать воздухом?") },
        { 150, new WarningMessage("150 минут игры!", "Ваши глаза явно не скажут вам спасибо, поэтому может отдохнуть?") },
        { 180, new WarningMessage("180 минут игры!", "Время выйти на улицу и потрогать траву.") },
        { 210, new WarningMessage("210 минут игры!", "Эй, такая сессия может вызвать проблемы со здоровьем, может уже надо наконец-то выключить телефон?") }
    };

    public void ShowWarning(int minutes)
    {
        if(warnings.TryGetValue(minutes, out WarningMessage message))
        {
            warningTextTitle.text = message.Title;
            warningTextBody.text = message.Body;

            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateWarningPanel());
        }
    }

    IEnumerator AnimateWarningPanel()
    {
        warningPanel.SetActive(true);
        var rect = warningPanel.GetComponent<RectTransform>();
        rect.DOKill();

        rect.DOAnchorPos(new Vector2(0, -90), 2.0f).SetLink(warningPanel);

        yield return new WaitForSeconds(5f);

        rect.DOAnchorPos(startPosAchievementPanel, 2.0f)
            .SetLink(warningPanel)
            .OnComplete(() => warningPanel.SetActive(false));
    }
}
