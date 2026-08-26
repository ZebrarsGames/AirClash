using DG.Tweening;
using UnityEngine;

public class ConsoleSettingsScr : MonoBehaviour
{
    [Header("Console")]
    [SerializeField] private GameObject console;

    private RectTransform rect;

    void Awake()
    {
        rect = console.GetComponent<RectTransform>();
    }

    void Start()
    {
        bool isActive = PlayerPrefs.GetInt("IsShowConsole", 0) != 0;
        console.SetActive(isActive);
    }

    public void SetConsoleVisibility(bool active)
    {
        if(active)
        {
            rect.DOKill();
            rect.localScale = Vector3.zero;
            console.SetActive(true);
            rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        } else
        {
            rect.DOKill();
            rect.localScale = Vector3.one;
            console.SetActive(true);
            rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => console.SetActive(false));
        }
    }
}
