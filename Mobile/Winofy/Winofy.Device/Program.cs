using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Winofy.Connection;
using Winofy.Connection.Devices;

namespace Winofy.Device
{
    class Program
    {
        private static WinofyClient Client = new WinofyClient();
        
        static async Task Main(string[] args)
        {
            if (args.Contains("-h") || args.Contains("--help"))
            {
                Console.WriteLine("Winofy client device application");
                Console.WriteLine("You can use these arguments");
                Console.WriteLine("----------common----------");
                Console.WriteLine("-u: Username [Not Recommended. Use -token instead]");
                Console.WriteLine("-p: Password [Not Recommended. Use -token instead]");
                Console.WriteLine("-token: Authorization token");
                Console.WriteLine("-h, --help: Show help");
                Console.WriteLine("----------command----------");
                Console.WriteLine("init: Initialize device file and save to the device.json");
                Console.WriteLine("register: Register device to the server");
                Console.WriteLine("create: Create new account");
                Console.WriteLine("record: Send record to the server (this needs -SI, -axes, -temp, -humidity parameters)");
                Console.WriteLine("list: List the registered devices");
                Console.WriteLine("----------record command----------");
                Console.WriteLine("-SI: SI value");
                Console.WriteLine("-axes: Axes (0=YZ, 1=XZ, 2=XY)");
                Console.WriteLine("-temp: Temperature (Celsius)");
                Console.WriteLine("-humidity: Humidity (0~1)");
                return;
            }

            var token = await GetTokenAsync(args);
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            Client.SetAuthorizationToken(token);

            if (args.Contains("list"))
            {
                var dev = await Client.ListDevicesAsync();

                foreach (var d in dev.Devices)
                {
                    Console.WriteLine($"----------");
                    Console.WriteLine($"ID:{d.Id}");
                    Console.WriteLine($"Name:{d.Name}");
                    Console.WriteLine($"Description:{d.Description}");
                }

                return;
            }

            var device = GetOrInitDevice(args);
            if (device == null) return;
            
            if (args.Contains("register"))
            {
                await RegisterDeviceAsync(device);
                return;
            }
            
            if (args.Contains("record"))
            {
                await SendRecordAsync(args, device);
                return;
            }
        }

        private static async Task RegisterDeviceAsync(Connection.Devices.Device device)
        {
            var devReg = await Client.RegisterDeviceAsync(device, false);

            if (devReg.Success)
            {
                Console.WriteLine("Device was successfully registered");
            }
            else
            {
                switch (devReg.Message)
                {
                    case RegisterDeviceMessage.Unknown:
                        Console.WriteLine("Unknown error");
                        break;
                    case RegisterDeviceMessage.Unauthorized:
                        Console.WriteLine("Failed to authorize");
                        break;
                    case RegisterDeviceMessage.DeviceExists:
                        Console.WriteLine("This device was already registered");
                        break;
                }
            }
        }

        private static Connection.Devices.Device GetOrInitDevice(string[] args)
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

                Console.WriteLine("Json successfully exported to device.json");
            }
            else
            {
                if (!GetArgument(args, "-dev", out var path))
                {
                    path = "device.json";
                }

                if (!File.Exists(path))
                {
                    Console.WriteLine("Device configuration file does not exist.");
                    Console.WriteLine("Please provide device.json at the current directory or use \"-dev <PATH>\" parameter.");
                    Console.WriteLine("If you don't have any file, add \"init\" argument to initialize new one");
                    return null;
                }

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs))
                {
                    var js = sr.ReadToEnd();
                    device = JsonConvert.DeserializeObject<Connection.Devices.Device>(js);
                }
            }

            return device;
        }

        static async Task<string> GetTokenAsync(string[] args, bool createAccount = true)
        {
            GetArgument(args, "-token", out var token);

            string username = null, password = null;
            if (args.Contains("create") && createAccount)
            {
                Console.WriteLine("New Account");
                username = WaitForInput("Username : ");
                password = WaitForInput("Password : ", true);
                Console.WriteLine("Registering...");
                var res = await Client.RegisterAsync(username, password);

                if (res.Success)
                {
                    Console.WriteLine("You've created new account \"" + username + "\"");
                }
                else
                {
                    Console.WriteLine($"Failed to create account");

                    int i = 1;
                    foreach (var msg in res.Messages)
                    {
                        Console.WriteLine($"Reason {i++} : {msg}");
                    }

                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                if (username == null)
                    GetArgument(args, "-u", out username);

                if (string.IsNullOrWhiteSpace(username))
                {
                    username = WaitForInput("Username : ");
                }

                if (password == null)
                    GetArgument(args, "-p", out password);

                if (string.IsNullOrWhiteSpace(password))
                {
                    password = WaitForInput("Password : ", true);
                }

                var loginRes = await Client.LoginAsync(username, password);

                if (loginRes.Success)
                {
                    token = loginRes.Token;
                    Console.WriteLine("Authorization Token : " + loginRes.Token);
                }
                else
                {
                    Console.WriteLine("Failed to login. Make sure you've entered username or password correctly");
                    return await GetTokenAsync(args, false);
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