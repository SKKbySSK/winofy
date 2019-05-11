using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Winofy.Connection.Devices
{
    public class ListDevicesResult
    {
        [JsonProperty("devices")]
        public Device[] Devices { get; set; }
    }
}
