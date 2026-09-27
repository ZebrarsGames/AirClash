using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

[DisallowMultipleComponent]
public sealed class ExitHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform exitPanelRect;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    [Header("Input Settings")]
    [SerializeField] private InputAction exitAction;
    [SerializeField] private float doubleClickDelay = 0.5f;

    [Header("Dependencies")]
    [SerializeField] private SaveManager saveManager;

    private float _lastClickTime;
    private Tween _fadeTween;
    private bool _isPanelActive;

    private void Awake()
    {
        if(exitPanelRect != null)
        {
            exitPanelRect.gameObject.SetActive(false);
            exitPanelRect.localScale = Vector3.zero;
        }
        
        if(exitAction == null || exitAction.bindings.Count == 0)
        {
            exitAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
        }
    }

    private void OnEnable() => exitAction.Enable();
    private void OnDisable() => exitAction.Disable();

    private void Update()
    {
        if(!exitAction.WasPressedThisFrame()) return;

        if(_isPanelActive)
        {
            HideExitPanel();
        }
        else
        {
            HandleBackButton();
        }
    }

    private void HandleBackButton()
    {
        if(Time.time - _lastClickTime < doubleClickDelay)
        {
            ShowExitPanel();
        }
        else
        {
            _lastClickTime = Time.time;
        }
    }

    public void ShowExitPanel()
    {
        if(_isPanelActive) return;
        _isPanelActive = true;

        _fadeTween?.Kill();

        exitPanelRect.gameObject.SetActive(true);
        
        _fadeTween = exitPanelRect.DOScale(Vector3.one, animationDuration)
            .SetEase(showEase)
            .SetLink(gameObject);
    }

    public void HideExitPanel()
    {
        if(!_isPanelActive) return;
        _isPanelActive = false;

        _fadeTween?.Kill();

        _fadeTween = exitPanelRect.DOScale(Vector3.zero, animationDuration)
            .SetEase(hideEase)
            .OnComplete(() => exitPanelRect.gameObject.SetActive(false))
            .SetLink(gameObject);
    }

    public void ConfirmExit()
    {
        PlayerPrefs.SetFloat("Music", 0);
        saveManager.SaveData();
        if(saveManager != null)
        {
            saveManager.SaveData();
        }
        
        PlayerPrefs.Save();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CancelExit() => HideExitPanel();
}