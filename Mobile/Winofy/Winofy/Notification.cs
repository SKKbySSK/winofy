using System;
using Winofy.Connection;
using System.Threading.Tasks;
namespace Winofy
{
    public static class Notification
    {
        public static string FcmToken { get; set; }

        public static Task<NotificationResult> SetNotificationAsync(WinofyClient client, NotificationType notification)
        {
            return client.UpdateNotificationAsync(FcmToken, notification);
        }
    }
}
