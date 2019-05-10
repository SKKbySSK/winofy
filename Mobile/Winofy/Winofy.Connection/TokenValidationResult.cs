using System;
using Newtonsoft.Json;
namespace Winofy.Connection
{
    public class TokenValidationResult
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }
    }
}
