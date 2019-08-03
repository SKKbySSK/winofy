using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Winofy.Connection.Devices;
using System.Net;

namespace Winofy.Connection
{
    public class UnauthorizedException : Exception
    {
    }

    public class WinofyClientException : Exception
    {
        public WinofyClientException(HttpStatusCode statusCode, string url)
        {
            StatusCode = statusCode;
            Url = url;
        }

        public HttpStatusCode StatusCode { get; }

        public string Url { get; }
    }

    public class WinofyClient : IDisposable
    {
        public string ApiEndPoint { get; private set; } = "https://4eiot.ksprogram.work/";
        //public const string Endpoint = "http://127.0.0.1:8080/";

        public string InternalEndpoint => ApiEndPoint + "internal/";

        public string DevicesEndpoint => ApiEndPoint + "devices/";

        private HttpClient Client { get; } = new HttpClient();

        private HttpClient AuthorizedClient { get; } = new HttpClient();

        public WinofyClient() { }

        public WinofyClient(string apiEndPoint)
        {
            if (!apiEndPoint.EndsWith("/", StringComparison.Ordinal))
            {
                apiEndPoint += "/";
            }

            ApiEndPoint = apiEndPoint;
        }

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
                ThrowIfNotFound(url, resp);
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
                ThrowIfNotFound(url, resp);
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
                ThrowIfNotFound(url, resp);
                var res = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TokenValidationResult>(res);
            }
        }

        public async Task<ListDevicesResult> ListDevicesAsync()
        {
            var url = DevicesEndpoint + $"list";

            using (var resp = await AuthorizedClient.GetAsync(url))
            {
                ThrowIfNotFound(url, resp);
                ThrowIfUnauthorized(resp);
                var res = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListDevicesResult>(res);
            }
        }

        public async Task<RegisterDeviceResult> RegisterDeviceAsync(Device device, bool generateDeviceId)
        {
            var url = DevicesEndpoint + "register";

            if (generateDeviceId)
            {
                device.Id = Guid.NewGuid().ToString();
            }

            var content = new Dictionary<string, string>()
            {
                { "device_id", device.Id },
                { "name", device.Name },
                { "description", device.Description },
            };

            using (var body = new FormUrlEncodedContent(content))
            using (var resp = await AuthorizedClient.PutAsync(url, body))
            {
                ThrowIfNotFound(url, resp);
                ThrowIfUnauthorized(resp);
                var res = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RegisterDeviceResult>(res);
            }
        }

        public async Task<NotificationResult> UpdateNotificationAsync(string deviceToken, NotificationType notification)
        {
            var url = InternalEndpoint + "notification";

            int type = (int)notification;

            var content = new Dictionary<string, string>()
            {
                { "device_token", deviceToken },
                { "type", type.ToString() },
            };

            using (var body = new FormUrlEncodedContent(content))
            using (var resp = await AuthorizedClient.PutAsync(url, body))
            {
                ThrowIfNotFound(url, resp);
                ThrowIfUnauthorized(resp);
                var res = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NotificationResult>(res);
            }
        }

        public async Task<bool> RecordAsync(string deviceId, float si, Axes axes, float temperature, float humidity)
        {
            var url = DevicesEndpoint + "record";
            var content = new Dictionary<string, string>()
            {
                { "device_id", deviceId },
                { "SI", si.ToString() },
                { "axes", ((int)axes).ToString() },
                { "temp", temperature.ToString() },
                { "humidity", humidity.ToString() },
            };

            using (var body = new FormUrlEncodedContent(content))
            using (var resp = await AuthorizedClient.PostAsync(url, body))
            {
                ThrowIfNotFound(url, resp);
                ThrowIfUnauthorized(resp);
                return resp.IsSuccessStatusCode;
            }
        }

        public void SetAuthorizationToken(string token)
        {
            AuthorizedClient.DefaultRequestHeaders.Remove("token");

            if (token != null)
                AuthorizedClient.DefaultRequestHeaders.Add("token", token);
        }

        public void Dispose()
        {
            Client.Dispose();
            AuthorizedClient.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ThrowIfNotFound(string url, HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new WinofyClientException(response.StatusCode, url);
            }
        }

        private void ThrowIfUnauthorized(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
        }
    }
}
