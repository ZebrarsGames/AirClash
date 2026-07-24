using UnityEngine;
using UnityEngine.Events;

public class SessionTimer : MonoBehaviour
{
    public static SessionTimer Instance { get; private set; }
    [Header("Events")]
    [HideInInspector] public UnityEvent<int> onMinuteChanged = new UnityEvent<int>();
    private float sessionStartTime;
    private int lastTriggeredMinute = -1; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sessionStartTime = Time.realtimeSinceStartup;
    }

    void FixedUpdate()
    {
        CheckSessionWarnings();
    }

    public int GetSessionTimeMinutes()
    {
        float currentSessionTime = Time.realtimeSinceStartup - sessionStartTime;

        int minutes = Mathf.FloorToInt(currentSessionTime / 60f);

        return minutes;
    } 

    private void CheckSessionWarnings()
    {
        int currentMinute = GetSessionTimeMinutes();

        if (currentMinute != lastTriggeredMinute)
        {
            lastTriggeredMinute = currentMinute;
            
            onMinuteChanged.Invoke(currentMinute);
        }
    }
}
