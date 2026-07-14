using System;
using UnityEngine;

public class PlaytimeTracker : MonoBehaviour
{
    public static PlaytimeTracker Instance { get; private set; }

    [Header("Scripts")]
    [SerializeField] private SaveManager saveManager;
    private const string PlaytimeKey = "TotalPlaytimeSeconds";
    private float _sessionStartTime;
    private float _totalPlaytimeSeconds;
    private float _nextAutoSaveTime; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _totalPlaytimeSeconds = saveManager.GetData().Playtime;
            _sessionStartTime = Time.time;

            _nextAutoSaveTime = Time.time + 90f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        SavePlaytime();
    }

    void Update()
    {
        if (Time.time >= _nextAutoSaveTime)
        {
            _nextAutoSaveTime = Time.time + 90f;
            SavePlaytime(); 
        }
    }

    private void SavePlaytime()
    {
        if (Instance != this) return;

        float currentSessionDuration = Time.time - _sessionStartTime;
        
        _totalPlaytimeSeconds += currentSessionDuration;
        
        _sessionStartTime = Time.time; 

        PlayerPrefs.SetFloat(PlaytimeKey, _totalPlaytimeSeconds);
        PlayerPrefs.Save();
        
        if (saveManager != null)
        {
            saveManager.SaveData();
        }
    }

    public string GetFormattedPlaytime()
    {
        float totalTime = _totalPlaytimeSeconds + (Time.time - _sessionStartTime);
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalTime);
        return timeSpan.ToString(@"hh\:mm\:ss");
    }

    public float GetSecondsPlaytime()
    {
        float totalTime = _totalPlaytimeSeconds + (Time.time - _sessionStartTime);
        return totalTime;
    }
}
