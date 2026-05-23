using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private Text fpsText;
    [SerializeField] private Toggle trailToggle;
    public float volumeLevel = 0.5f;

    public void Start() {
        QualitySettings.vSyncCount = 0;
        if(PlayerPrefs.GetInt("FPS") == 0) PlayerPrefs.SetInt("FPS", 60);
        else Application.targetFrameRate = PlayerPrefs.GetInt("FPS");
        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        fpsSlider.value = PlayerPrefs.GetInt("FPS");
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            if(PlayerPrefs.GetInt("Trail", 1) == 0) trailToggle.isOn = false;
            else trailToggle.isOn = true;
        }
        
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

    public void ShowTelegram()
    {
        Application.OpenURL("https://t.me/airclash_dev");
    }

    public void ShowGitHub()
    {
        Application.OpenURL("https://github.com/Zebraar/AirClash");
    }
}
