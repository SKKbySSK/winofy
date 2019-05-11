using System;
using Newtonsoft.Json;
namespace Winofy.Connection.Devices
{
    public enum RegisterDeviceMessage
    {
        Ok,
        DeviceExists,
        Unknown
    }

    public class RegisterDeviceResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public RegisterDeviceMessage Message { get; set; }
    }
}
