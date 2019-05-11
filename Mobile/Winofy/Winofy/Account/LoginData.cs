using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Winofy.Account
{
    public partial class LoginData
    {
        public LoginData() { }

        public LoginData(string username, string authorizingToken)
        {
            Username = username;
            AuthorizingToken = authorizingToken;
        }

        public string AuthorizingToken { get; set; }

        public string Username { get; set; }
    }

    public partial class LoginData
    {
        public static string LoginPath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "login.json");

        public static LoginData Open(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<LoginData>(sr.ReadToEnd());
            }
        }

        public static async Task<LoginData> OpenAsync(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<LoginData>(await sr.ReadToEndAsync());
            }
        }

        public static void Export(LoginData data, Stream output)
        {
            using (var sw = new StreamWriter(output))
            {
                sw.Write(JsonConvert.SerializeObject(data));
            }
        }
    }
}
