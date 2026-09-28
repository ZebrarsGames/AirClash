using UnityEngine;

public static class VibrationHandler
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject vibrator;
    private static AndroidJavaClass vibrationEffectClass;
    private static int sdkVersion;

    static VibrationHandler()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }

        using (AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            sdkVersion = buildVersion.GetStatic<int>("SDK_INT");
        }

        if (sdkVersion >= 26)
        {
            vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
        }
    }

    /// <summary>
    /// Вызов вибрации
    /// </summary>
    /// <param name="milliseconds">Длительность в мс (например, 40 мс)</param>
    /// <param name="amplitude">Сила от 1 до 255 (по умолчанию -1 = дефолтная сила системы)</param>
    public static void Vibrate(long milliseconds = 40, int amplitude = -1)
    {
        if (vibrator == null || !vibrator.Call<bool>("hasVibrator")) return;

        // Для Android 8.0 (API 26) и новее (включая Android 14/15 на POCO)
        if (sdkVersion >= 26 && vibrationEffectClass != null)
        {
            // VibrationEffect.DEFAULT_AMPLITUDE = -1
            int DEFAULT_AMPLITUDE = vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE");
            int targetAmplitude = (amplitude > 0 && amplitude <= 255) ? amplitude : DEFAULT_AMPLITUDE;

            AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                "createOneShot", 
                milliseconds, 
                targetAmplitude
            );

            vibrator.Call("vibrate", effect);
        }
        else
        {
            // Для старых устройств (API < 26)
            vibrator.Call("vibrate", milliseconds);
        }
    }
#else
    public static void Vibrate(long milliseconds = 40, int amplitude = -1)
    {
        // Лог для проверки в редакторе Unity
        // Debug.Log($"[Vibrate] {milliseconds}ms");
    }
#endif
}