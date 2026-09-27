using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

[DisallowMultipleComponent]
public sealed class FPSCounter : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private RectTransform textRectTransform;

    [Header("Settings")]
    [Range(0.1f, 2.0f)] 
    [SerializeField] private float updateInterval = 0.5f;

    private Coroutine calculateCoroutine;
    private WaitForSecondsRealtime cachedWait;
    private GameObject textGameObject;
    private bool isActive;

    private readonly char[] displayBuffer = { 'F', 'P', 'S', ':', ' ', '0', '0', '0' };

    private void Awake()
    {
        if (fpsText == null) fpsText = GetComponentInChildren<TextMeshProUGUI>(true);
        
        if (fpsText != null)
        {
            textGameObject = fpsText.gameObject;
            if (textRectTransform == null) textRectTransform = fpsText.GetComponent<RectTransform>();
        }

        cachedWait = new WaitForSecondsRealtime(updateInterval);
        
        bool shouldBeActive = PlayerPrefs.GetInt("FpsCounter", 0) != 0;
        SetIsActiveImmediate(shouldBeActive);
    }

    private void OnEnable()
    {
        if(isActive)
        {
            StartLoop();
        }
    }

    private void OnDisable()
    {
        StopLoop();
    }

    private void StartLoop()
    {
        StopLoop();
        calculateCoroutine = StartCoroutine(FPSCalculateLoop());
    }

    private void StopLoop()
    {
        if(calculateCoroutine != null)
        {
            StopCoroutine(calculateCoroutine);
            calculateCoroutine = null;
        }
    }

    private IEnumerator FPSCalculateLoop()
    {
        int lastFrameCount = Time.frameCount;
        float lastTime = Time.realtimeSinceStartup;

        while(true)
        {
            yield return cachedWait;

            float currentTime = Time.realtimeSinceStartup;
            int currentFrameCount = Time.frameCount;

            int framesDelta = currentFrameCount - lastFrameCount;
            float timeDelta = currentTime - lastTime;

            if(timeDelta > 0f)
            {
                int fps = Mathf.RoundToInt(framesDelta / timeDelta);
                UpdateFPSTextNonAlloc(fps);
            }

            lastFrameCount = currentFrameCount;
            lastTime = currentTime;
        }
    }

    private void UpdateFPSTextNonAlloc(int fps)
    {
        fps = Mathf.Clamp(fps, 0, 999);

        int hundreds = fps / 100;
        int tens = (fps / 10) % 10;
        int ones = fps % 10;

        if(hundreds > 0)
        {
            displayBuffer[5] = (char)('0' + hundreds);
            displayBuffer[6] = (char)('0' + tens);
            displayBuffer[7] = (char)('0' + ones);
            fpsText.SetText(displayBuffer, 0, 8);
        }
        else if(tens > 0)
        {
            displayBuffer[5] = (char)('0' + tens);
            displayBuffer[6] = (char)('0' + ones);
            fpsText.SetText(displayBuffer, 0, 7);
        }
        else
        {
            displayBuffer[5] = (char)('0' + ones);
            fpsText.SetText(displayBuffer, 0, 6);
        }
    }

    private void SetIsActiveImmediate(bool active)
    {
        isActive = active;
        
        if(textGameObject != null) textGameObject.SetActive(active);
        if(textRectTransform != null) textRectTransform.localScale = active ? Vector3.one : Vector3.zero;

        if(active) StartLoop();
        else StopLoop();
    }

    public void SetIsActive(bool active)
    {
        if(textGameObject == null || textRectTransform == null) return;
        if(isActive == active) return;

        isActive = active;
        textRectTransform.DOKill();

        if(active)
        {
            textGameObject.SetActive(true);
            StartLoop();
            textRectTransform.DOScale(Vector3.one, 0.2f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
        else
        {
            StopLoop();
            textRectTransform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => textGameObject.SetActive(false));
        }
    }
}