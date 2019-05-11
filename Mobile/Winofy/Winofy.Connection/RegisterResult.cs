using System;
using Newtonsoft.Json;

namespace Winofy.Connection
{
    public enum RegisterMessage
    {
        Unknown,
        Ok,
        InvalidRequest,
        UserExists,
        IncorrectUsernameFormat,
        IncorrectPasswordFormat,
    }

    public class RegisterResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("messages")]
        public RegisterMessage[] Messages { get; set; }
    }
}
