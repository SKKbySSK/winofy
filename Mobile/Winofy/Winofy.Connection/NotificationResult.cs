using System;
using Newtonsoft.Json;
namespace Winofy.Connection
{
    public class NotificationResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public enum NotificationType
    {
        Disabled = 0,
        APNs = 1,
        FCM = 2,
    }
}
