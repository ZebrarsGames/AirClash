using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Audio;
using TMPro;

public class SettingsHandler : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private Slider bgMusicSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Toggles")]
    [SerializeField] private Toggle animBgToggle;
    [SerializeField] private Toggle trailToggle;
    [SerializeField] private Toggle puckTrailToggle;
    [SerializeField] private Toggle fpsCounterToggle;
    [SerializeField] private Toggle bgMusicInGameToggle;
    [SerializeField] private Toggle debugConsoleToggle;
    [SerializeField] private Toggle vibrationToggle;

    [Header("Other")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private UnityEvent<bool> isAnimToggleEvent;
    [SerializeField] private UnityEvent<bool> isFpsCounterEvent;

    private static readonly string NameMaster = "Master";
    private static readonly string NameSFX = "SFX";
    private static readonly string NameBgMusic = "BgMusic";

    private static readonly string KeyFps = "FPS";
    private static readonly string KeyMasterVol = "MasterVolume";
    private static readonly string KeySFXVol = "SFXVolume";
    private static readonly string KeyBgMusicVol = "BgMusicVolume";
    private static readonly string KeyTrail = "Trail";
    private static readonly string KeyIsAnimBg = "isAnimBg";
    private static readonly string KeyPuckTrail = "PuckTrail";
    private static readonly string KeyFpsCounter = "FpsCounter";
    private static readonly string KeyBgMusicInGame = "BgMusicInGame";
    private static readonly string KeyVibration = "Vibration";
    private static readonly string KeyIsShowConsole = "IsShowConsole";

    private const string UrlTelegram = "https://t.me/airclash_dev";
    private const string UrlGitHub = "https://github.com/ZebrarsGames/AirClash";
    private const string UrlWebSite = "https://zebrarsgames.github.io/AirClash/";
    private const string UrlYouTube = "https://www.youtube.com/@AirClash-Official";
    private const string UrlTikTok = "https://www.tiktok.com/@airclash_dev";
    private const string UrlItchIo = "https://zebraaar.itch.io/airclash";

    private void Start() 
    {
        QualitySettings.vSyncCount = 0;
        
        int savedFps = PlayerPrefs.GetInt(KeyFps, 60);
        float masterVol = PlayerPrefs.GetFloat(KeyMasterVol, 1.0f);
        float sfxVol = PlayerPrefs.GetFloat(KeySFXVol, 1.0f);
        float bgMusicVol = PlayerPrefs.GetFloat(KeyBgMusicVol, 1.0f);

        Application.targetFrameRate = savedFps;

        audioMixer.SetFloat(NameMaster, ConvertLinearToDb(masterVol));
        audioMixer.SetFloat(NameSFX, ConvertLinearToDb(sfxVol));
        audioMixer.SetFloat(NameBgMusic, ConvertLinearToDb(bgMusicVol));

        masterVolumeSlider.value = masterVol;
        bgMusicSlider.value = bgMusicVol;
        sfxVolumeSlider.value = sfxVol;
        fpsSlider.value = savedFps;
        
        trailToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KeyTrail, 1) != 0);
        animBgToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KeyIsAnimBg, 1) != 0);
        puckTrailToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KeyPuckTrail, 1) != 0);
        fpsCounterToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KeyFpsCounter, 0) != 0);
        bgMusicInGameToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KeyBgMusicInGame, 1) != 0);
        vibrationToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KeyVibration, 1) != 0);
        debugConsoleToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KeyIsShowConsole, 0) != 0);

        UpdateFpsText(savedFps);
    }

    public void OnVolumeSliderChanged() => 
        audioMixer.SetFloat(NameMaster, ConvertLinearToDb(masterVolumeSlider.value));

    public void OnBgMusicVolumeSliderChanged() => 
        audioMixer.SetFloat(NameBgMusic, ConvertLinearToDb(bgMusicSlider.value));

    public void OnSoundEffectsSliderChanged() => 
        audioMixer.SetFloat(NameSFX, ConvertLinearToDb(sfxVolumeSlider.value));

    public void OnFpsSliderChanged()
    {
        int fpsValue = Mathf.RoundToInt(fpsSlider.value);
        UpdateFpsText(fpsValue);
        Application.targetFrameRate = fpsValue;
    }

    public void OnTrailToggleChanged() => PlayerPrefs.SetInt(KeyTrail, trailToggle.isOn ? 1 : 0);

    public void OnAnimBgToggleChanged()
    {
        PlayerPrefs.SetInt(KeyIsAnimBg, animBgToggle.isOn ? 1 : 0);
        isAnimToggleEvent.Invoke(animBgToggle.isOn);
    }

    public void OnPuckTrailToggleChanged() => PlayerPrefs.SetInt(KeyPuckTrail, puckTrailToggle.isOn ? 1 : 0);

    public void OnFPSCounterToggleChanged()
    {
        PlayerPrefs.SetInt(KeyFpsCounter, fpsCounterToggle.isOn ? 1 : 0);
        isFpsCounterEvent.Invoke(fpsCounterToggle.isOn);
    }

    public void OnBgMusicInGameToggleChanged() => PlayerPrefs.SetInt(KeyBgMusicInGame, bgMusicInGameToggle.isOn ? 1 : 0);

    public void OnDebugConsoleToggleChanged() => PlayerPrefs.SetInt(KeyIsShowConsole, debugConsoleToggle.isOn ? 1 : 0);

    public void OnVibrationToggleChanged() 
    {
        PlayerPrefs.SetInt(KeyVibration, vibrationToggle.isOn ? 1 : 0);
        VibrationHandler.SetVibrationEnabled(vibrationToggle.isOn);
    }

    public void ShowTelegram() => Application.OpenURL(UrlTelegram);
    public void ShowGitHub() => Application.OpenURL(UrlGitHub);
    public void ShowWebSite() => Application.OpenURL(UrlWebSite);
    public void ShowYouTube() => Application.OpenURL(UrlYouTube);
    public void ShowTikTok() => Application.OpenURL(UrlTikTok);
    public void ShowItchIo() => Application.OpenURL(UrlItchIo);

    public void SaveSettings()
    {
        PlayerPrefs.SetInt(KeyFps, Mathf.RoundToInt(fpsSlider.value));
        PlayerPrefs.SetFloat(KeySFXVol, sfxVolumeSlider.value);
        PlayerPrefs.SetFloat(KeyBgMusicVol, bgMusicSlider.value);
        PlayerPrefs.SetFloat(KeyMasterVol, masterVolumeSlider.value);
        PlayerPrefs.Save();
    }

    private static float ConvertLinearToDb(float linear) => 
        linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;

    private void UpdateFpsText(int value)
    {
        fpsText.SetText("{0}", value);
    }
}