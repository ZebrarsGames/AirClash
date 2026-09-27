using DG.Tweening;
using UnityEngine;

public class ConsoleSettingsScr : MonoBehaviour
{
    [Header("Console")]
    [SerializeField] private GameObject console;

    private RectTransform rect;

    private void Awake()
    {
        if(console != null)
        {
            rect = console.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        bool isActive = PlayerPrefs.GetInt("IsShowConsole", 0) != 0;
        if(console != null)
        {
            console.SetActive(isActive);
        }
    }

    private void OnDestroy()
    {
        if(rect != null)
        {
            rect.DOKill();
        }
    }

    public void SetConsoleVisibility(bool active)
    {
        if(rect == null || console == null) return;

        rect.DOKill();

        if(active)
        {
            rect.localScale = Vector3.zero;
            console.SetActive(true);
            rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            rect.localScale = Vector3.one;
            rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => console.SetActive(false));
        }
    }
}