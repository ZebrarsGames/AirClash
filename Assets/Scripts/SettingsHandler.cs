using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class IsAnimToggleEvent : UnityEvent<bool> { }
public class SettingsHandler : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider fpsSlider;

    [Header("Texts")]
    [SerializeField] private Text fpsText;

    [Header("Toggles")]
    [SerializeField] private Toggle animBgToggle;
    [SerializeField] private Toggle trailToggle;

    [Header("Other")]
    [SerializeField] private IsAnimToggleEvent isAnimToggleEvent;

    public void Start() 
    {
        QualitySettings.vSyncCount = 0;
        if(PlayerPrefs.GetInt("FPS") == 0) PlayerPrefs.SetInt("FPS", 60);
        else Application.targetFrameRate = PlayerPrefs.GetInt("FPS");
        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        fpsSlider.value = PlayerPrefs.GetInt("FPS");
        trailToggle.isOn = PlayerPrefs.GetInt("Trail", 1) != 0;
        animBgToggle.isOn = PlayerPrefs.GetInt("isAnimBg", 1) != 0;
    }

    public void OnVolumeSliderChanged() {
        PlayerPrefs.SetFloat("MusicVolume", volumeSlider.value);
        AudioListener.volume = volumeSlider.value;
    }
    public void OnFpsSliderChanged()
    {
        PlayerPrefs.SetInt("FPS", Convert.ToInt32(fpsSlider.value));
        fpsText.text = Convert.ToString(Convert.ToInt32(fpsSlider.value));
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS");
    }

    public void OnTrailToggleChanged()
    {
        PlayerPrefs.SetInt("Trail", trailToggle.isOn ? 1 : 0);
    }

    public void OnAnimBgToggleChanged()
    {
        PlayerPrefs.SetInt("isAnimBg", animBgToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        isAnimToggleEvent.Invoke(animBgToggle.isOn);
    }

    public void ShowTelegram()
    {
        Application.OpenURL("https://t.me/airclash_dev");
    }

    public void ShowGitHub()
    {
        Application.OpenURL("https://github.com/Zebraar/AirClash");
    }
}
