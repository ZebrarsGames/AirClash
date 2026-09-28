using UnityEngine;

public static class VibrationHandler
{
    private static bool isVibrationEnabled = true;

    public static void SetVibrationEnabled(bool enabled)
    {
        isVibrationEnabled = enabled;
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static readonly AndroidJavaObject vibrator;
    private static readonly int sdkVersion;
    private static readonly AndroidJavaClass vibrationEffectClass;
    private static readonly int defaultAmplitude;
    private static readonly bool hasVibrator;

    static VibrationHandler()
    {
        isVibrationEnabled = PlayerPrefs.GetInt("Vibration", 1) == 1;

        using(var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }

        hasVibrator = vibrator != null && vibrator.Call<bool>("hasVibrator");

        using(var buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            sdkVersion = buildVersion.GetStatic<int>("SDK_INT");
        }

        if(sdkVersion >= 26 && hasVibrator)
        {
            vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
            defaultAmplitude = vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE");
        }
    }

    public static void Vibrate(long milliseconds = 40, int amplitude = -1)
    {
        if(!isVibrationEnabled || !hasVibrator) return;

        if(sdkVersion >= 26 && vibrationEffectClass != null)
        {
            int targetAmplitude = (amplitude > 0 && amplitude <= 255) ? amplitude : defaultAmplitude;

            using(AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, targetAmplitude))
            {
                vibrator.Call("vibrate", effect);
            }
        }
        else
        {
            vibrator.Call("vibrate", milliseconds);
        }
    }
#else
    static VibrationHandler()
    {
        isVibrationEnabled = PlayerPrefs.GetInt("Vibration", 1) == 1;
    }

    public static void Vibrate(long milliseconds = 40, int amplitude = -1)
    {
        if(!isVibrationEnabled) return;
        Debug.Log($"[Vibrate] {milliseconds}ms and {amplitude}am");
    }
#endif
}