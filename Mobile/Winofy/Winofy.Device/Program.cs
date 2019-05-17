using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Winofy.Connection;

namespace Winofy.Device
{
    class Program
    {
        private static WinofyClient Client = new WinofyClient();
        
        static async Task Main(string[] args)
        {
            Connection.Devices.Device device;
            
            if (args.Contains("init"))
            {
                device = new Connection.Devices.Device();
                device.Id = Guid.NewGuid().ToString();

                device.Name = WaitForInput("Name : ");
                
                Console.Write("Description (optional) : ");
                device.Description = Console.ReadLine();

                var js = JsonConvert.SerializeObject(device, Formatting.Indented);

                using (var fs = new FileStream("device.json", FileMode.Create, FileAccess.Write))
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(js);
                }
                
                Console.WriteLine("Json exported to device.json");
            }
            else
            {
                using (var fs = new FileStream("device.json", FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs))
                {
                    var js = sr.ReadToEnd();
                    device = JsonConvert.DeserializeObject<Connection.Devices.Device>(js);
                }
            }
            
            var token = await GetTokenAsync(args);
            Client.SetAuthorizationToken(token);
            await SendRecordAsync(args, device);
        }

        static async Task<string> GetTokenAsync(string[] args)
        {
            GetArgument(args, "-token", out var token);

            if (string.IsNullOrWhiteSpace(token))
            {
                GetArgument(args, "-u", out var username);

                if (string.IsNullOrWhiteSpace(username))
                {
                    username = WaitForInput("Username : ");
                }

                GetArgument(args, "-p", out var password);

                if (string.IsNullOrWhiteSpace(password))
                {
                    password = WaitForInput("Password : ", true);
                }

                var loginRes = await Client.LoginAsync(username, password);

                if (loginRes.Success)
                {
                    token = loginRes.Token;
                    Console.WriteLine("Authorized : " + loginRes.Token);
                }
                else
                {
                    return await GetTokenAsync(args);
                }
            }

            return token;
        }

        static async Task SendRecordAsync(string[] args, Connection.Devices.Device device)
        {
            GetArgument(args, "-SI", out var siStr);
            GetArgument(args, "-axes", out var axesStr);
            GetArgument(args, "-temp", out var tempStr);
            GetArgument(args, "-humidity", out var humidStr);

            if (float.TryParse(siStr, out var si) && int.TryParse(axesStr, out var axes) &&
                float.TryParse(tempStr, out var temp) && float.TryParse(humidStr, out var humid))
            {
                await Client.RecordAsync(device.Id, si, (Axes) axes, temp, humid);
            }
        }

        static bool GetArgument(string[] args, string name, out string value)
        {
            for (int i = 0; args.Length - 1 > i; i++)
            {
                if (args[i] == name)
                {
                    value = args[i + 1];
                    return true;
                }
            }

            value = null;
            return false;
        }

        static string WaitForInput(string displayText, bool hideInput = false, string oneMoreMessage = null, Func<string, bool> condition = null)
        {
            string input = null;
            bool first = true;
            do
            {
                Console.Write(first ? displayText : (oneMoreMessage ?? displayText));

                if (hideInput)
                {
                    input = "";
                    while (true)
                    {
                        var key = System.Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Enter)
                            break;
                        input += key.KeyChar;
                    }
                    Console.WriteLine();
                }
                else
                {
                    input = Console.ReadLine();
                }
                
            } while (!condition?.Invoke(input) ?? string.IsNullOrWhiteSpace(input));

            return input;
        }
    }
}