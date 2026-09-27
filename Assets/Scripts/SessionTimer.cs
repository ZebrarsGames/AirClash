using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SessionTimer : MonoBehaviour
{
    public static SessionTimer Instance { get; private set; }

    public event Action<int> OnMinuteChanged;

    private float _sessionStartTime;
    private int _lastTriggeredMinute = -1;
    private Coroutine _timerCoroutine;
    
    private readonly WaitForSeconds _oneSecondDelay = new WaitForSeconds(1.0f);

    public int CurrentSessionMinutes { get; private set; }

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        _sessionStartTime = Time.unscaledTime;
        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private void OnDestroy()
    {
        if(_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }
    }

    private IEnumerator TimerRoutine()
    {
        while(true)
        {
            UpdateSessionTime();
            yield return _oneSecondDelay;
        }
    }

    private void UpdateSessionTime()
    {
        float elapsedSeconds = Time.unscaledTime - _sessionStartTime;
        
        int currentMinute = (int)(elapsedSeconds / 60f);

        if(currentMinute == _lastTriggeredMinute) return;
        
        _lastTriggeredMinute = currentMinute;
        CurrentSessionMinutes = currentMinute;

        OnMinuteChanged?.Invoke(currentMinute);
    }

    private void InitializeSingleton()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}