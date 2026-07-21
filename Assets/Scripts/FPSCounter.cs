using UnityEngine;
using TMPro;
using DG.Tweening;

public class FPSCounter : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI fpsText;
    private float pollingTime = 0.5f;
    private float timer;
    private int frameCount;

    void Start()
    {
        fpsText.gameObject.SetActive(PlayerPrefs.GetInt("FpsCounter", 0) != 0);
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        frameCount++;

        if (timer >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / timer);
            fpsText.text = $"FPS: {frameRate}";

            timer = 0f;
            frameCount = 0;
        }
    }

    public void SetIsActive(bool isActive)
    {
        if(isActive)
        {
            fpsText.gameObject.SetActive(isActive);
            var rect = fpsText.GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;
            rect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        } else
        {
            var rect = fpsText.GetComponent<RectTransform>();
            rect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => fpsText.gameObject.SetActive(isActive));
        }
    }
}
