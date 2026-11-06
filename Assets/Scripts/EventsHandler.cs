using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EventsHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI eventTitleText;
    [SerializeField] private RawImage[] backgroundRawImages;
    [SerializeField] private RawImage[] eventIconRawImages;

    [Header("Buttons & Outlines")]
    [SerializeField] private Button[] eventButtons;   
    [SerializeField] private Image[] panels;
    [SerializeField] private Outline[] panelsOutlines;
    [SerializeField] private Outline[] eventOutlines;         

    [Header("Audio")]
    [SerializeField] private AudioSource eventAudioSource;    

    [Header("Popup")]
    [SerializeField] private GameObject eventPopupPanel;         
    [SerializeField] private TextMeshProUGUI popupTitleText;       
    [SerializeField] private TextMeshProUGUI popupDescriptionText; 
    [SerializeField] private Button popupCloseButton;
    [SerializeField] private TextMeshProUGUI popupCloseButtonText;
    [SerializeField] private float tweenDuration = 0.3f;           

    private const string EVENT_URL = "https://airclashserver.onrender.com/event/current";
    private const string LAST_SEEN_EVENT_KEY = "LastSeenEventId";

    private int _activeDownloadsCount = 0;

    private void Start()
    {
        if(eventPopupPanel != null) eventPopupPanel.SetActive(true);
        popupTitleText.SetText("Загрузка ивента...");
        popupDescriptionText.SetText("Пожалуйста, немного подождите...");
        popupCloseButton.interactable = false;
        popupCloseButtonText.SetText("Загрузка...");

        StartCoroutine(FetchCurrentEventRoutine());
    }

    private IEnumerator FetchCurrentEventRoutine()
    {
        using(UnityWebRequest www = UnityWebRequest.Get(EVENT_URL))
        {
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[EventsHandler] Не удалось загрузить данные ивента: {www.error}");
                HandleLoadingFailure();
            }
            else
            {
                string json = www.downloadHandler.text;
                EventResponse response = JsonUtility.FromJson<EventResponse>(json);

                if(response != null && response.status == "success" && response.@event != null)
                {
                    ApplyEventData(response.@event);
                    
                    yield return new WaitUntil(() => _activeDownloadsCount == 0);

                    CheckAndShowNewEventPopup(response.@event);
                }
                else
                {
                    HandleLoadingFailure();
                }
            }
        }
    }

    private void ApplyEventData(EventData eventData)
    {
        if(eventTitleText != null)
        {
            eventTitleText.SetText(eventData.title);
        }

        ApplyCustomColors(eventData.buttonColor, eventData.outlineColor);

        if (!string.IsNullOrEmpty(eventData.background)) _activeDownloadsCount++;
        if (!string.IsNullOrEmpty(eventData.music) && eventAudioSource != null) _activeDownloadsCount++;
        if (!string.IsNullOrEmpty(eventData.appIcon)) _activeDownloadsCount++;

        if(!string.IsNullOrEmpty(eventData.background))
        {
            StartCoroutine(LoadBackgroundRoutine(eventData.background));
        }

        if(!string.IsNullOrEmpty(eventData.music) && eventAudioSource != null)
        {
            StartCoroutine(LoadAndPlayAudioRoutine(eventData.music));
        }

        if(!string.IsNullOrEmpty(eventData.appIcon))
        {
            StartCoroutine(LoadAppIconRoutine(eventData.appIcon));
        }
    }

    private IEnumerator LoadBackgroundRoutine(string url)
    {
        using(UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            www.timeout = 15;
            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(www);
                foreach(var rawImage in backgroundRawImages)
                {
                    if(rawImage != null)
                    {
                        rawImage.texture = tex;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[EventsHandler] Не удалось загрузить фоновую картинку: {www.error}");
            }
        }
        _activeDownloadsCount--;
    }

    private IEnumerator LoadAppIconRoutine(string url)
    {
        using(UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            www.timeout = 15;
            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(www);
                foreach(var iconImg in eventIconRawImages)
                {
                    if(iconImg != null)
                    {
                        iconImg.texture = tex;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[EventsHandler] Не удалось загрузить иконку ивента: {www.error}");
            }
        }
        _activeDownloadsCount--;
    }

    private IEnumerator LoadAndPlayAudioRoutine(string url)
    {
        using(UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if(eventAudioSource != null && clip != null)
                {
                    eventAudioSource.clip = clip;
                    eventAudioSource.loop = true;
                    eventAudioSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"[EventsHandler] Не удалось загрузить аудиоклип: {www.error}");
            }
        }
        _activeDownloadsCount--;
    }

    private void CheckAndShowNewEventPopup(EventData eventData)
    {
        string lastSeenId = PlayerPrefs.GetString(LAST_SEEN_EVENT_KEY, string.Empty);

        if(lastSeenId != eventData.id)
        {
            if(popupTitleText != null) popupTitleText.SetText(eventData.title);
            if(popupDescriptionText != null) popupDescriptionText.SetText(eventData.description);
            if(popupCloseButtonText != null) popupCloseButtonText.SetText("Закрыть");
            
            popupCloseButton.interactable = true;

            PlayerPrefs.SetString(LAST_SEEN_EVENT_KEY, eventData.id);
            PlayerPrefs.Save();
        } 
        else
        {
            ClosePanel(eventPopupPanel);
        }
    }

    private void HandleLoadingFailure()
    {
        if(popupTitleText != null) popupTitleText.SetText("Ошибка загрузки");
        if(popupDescriptionText != null) popupDescriptionText.SetText("Не удалось обновить данные. Проверьте интернет.");
        if(popupCloseButtonText != null) popupCloseButtonText.SetText("Закрыть");
        popupCloseButton.interactable = true;
    }

    private void ApplyCustomColors(string hexButtonColor, string hexOutlineColor)
    {
        Color btnColor = Color.white;
        if(!string.IsNullOrEmpty(hexButtonColor))
        {
            ColorUtility.TryParseHtmlString(hexButtonColor, out btnColor);
        }

        Color outColor = Color.black;
        if(!string.IsNullOrEmpty(hexOutlineColor))
        {
            ColorUtility.TryParseHtmlString(hexOutlineColor, out outColor);
        }

        Color highlightedColor = ChangeColorBrightness(btnColor, 0.1f);
        Color pressedColor = ChangeColorBrightness(btnColor, -0.1f);

        if(eventButtons != null)
        {
            foreach(var btn in eventButtons)
            {
                if(btn != null)
                {
                    var colors = btn.colors;
                    colors.normalColor = btnColor;
                    colors.highlightedColor = highlightedColor;
                    colors.selectedColor = highlightedColor;
                    colors.pressedColor = pressedColor;
                    btn.colors = colors;
                }
            }
        }

        if(eventOutlines != null)
        {
            foreach(var outline in eventOutlines)
            {
                if(outline != null)
                {
                    outline.effectColor = outColor;
                }
            }
        }

        if(panels != null)
        {
            foreach(var panel in panels)
            {
                if(panel != null)
                {
                    panel.color = btnColor;
                }
            }
        }

        if(panelsOutlines != null)
        {
            foreach(var outline in panelsOutlines)
            {
                if(outline != null)
                {
                    outline.effectColor = outColor;
                }
            }
        }
    }

    private Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        float r = Mathf.Clamp01(color.r + correctionFactor);
        float g = Mathf.Clamp01(color.g + correctionFactor);
        float b = Mathf.Clamp01(color.b + correctionFactor);
        return new Color(r, g, b, color.a);
    }

    public void ClosePanel(GameObject panel)
    {
        if(panel == null) return;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.DOKill();
        rect.DOScale(Vector3.zero, tweenDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                rect.localScale = Vector3.one;
                popupCloseButton.interactable = true;
            });
    }
}

[Serializable]
public class EventResponse
{
    public string status;
    public EventData @event;
}

[Serializable]
public class EventData
{
    public string id;
    public string title;
    public string description;
    public string background;  
    public string appIcon;
    public string music;       
    public string buttonColor; 
    public string outlineColor;
}