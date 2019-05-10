using System;
using Newtonsoft.Json;
namespace Winofy.Connection
{
    public class LoginResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
