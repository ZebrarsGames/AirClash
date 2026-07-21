using UnityEngine;
using UnityEngine.Android;
using Unity.Notifications.Android;

public class PushNotificationsHandler : MonoBehaviour
{
    void Awake()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
        
        AndroidNotificationChannel channel = new AndroidNotificationChannel()
        {
            Name = "News | Новости",
            Description = "Уведомления о новостях игры.",
            Id = "news",
            Importance = Importance.Default
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    public void SendNotification()
    {
        AndroidNotification notification = new AndroidNotification()
        {
            Title = "Заголовок",
            Text = "Текст уведомления",
            FireTime = System.DateTime.Now.AddSeconds(5),
            SmallIcon = "small_icon",
            LargeIcon = "large_icon"
        };

        AndroidNotificationCenter.SendNotification(notification, "news");
    }
}
