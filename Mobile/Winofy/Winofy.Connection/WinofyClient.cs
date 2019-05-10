using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Winofy.Connection
{
    public class WinofyClient : IDisposable
    {
        public const string Endpoint = "https://4eiot.ksprogram.work/";
        private const string InternalEndpoint = Endpoint + "internal/";

        private HttpClient Client { get; } = new HttpClient();

        private HttpClient AuthorizedClient { get; } = new HttpClient();

        public async Task<RegisterResult> RegisterAsync(string username, string password)
        {
            var url = InternalEndpoint + "register";

            var content = new Dictionary<string, string>()
            {
                { "username", username },
                { "password", password }
            };

            using (var body = new FormUrlEncodedContent(content))
            using (var resp = await Client.PutAsync(url, body))
            {
                var res = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RegisterResult>(res);
            }
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var url = InternalEndpoint + "login";

            var content = new Dictionary<string, string>()
            {
                { "username", username },
                { "password", password }
            };

            using (var body = new FormUrlEncodedContent(content))
            using (var resp = await Client.PostAsync(url, body))
            {
                var res = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LoginResult>(res);
            }
        }

        public async Task<TokenValidationResult> ValidateTokenAsync(string username, string token)
        {
            var url = InternalEndpoint + "valid";

            var content = new Dictionary<string, string>()
            {
                { "username", username },
                { "token", token },
            };

            using (var body = new FormUrlEncodedContent(content))
            using (var resp = await Client.PostAsync(url, body))
            {
                var res = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TokenValidationResult>(res);
            }
        }

        public void SetAuthorizationToken(string token)
        {
            AuthorizedClient.DefaultRequestHeaders.Remove("token");
            AuthorizedClient.DefaultRequestHeaders.Add("token", token);
        }

        public void Dispose()
        {
            Client.Dispose();
            AuthorizedClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
