using System;
using Unosquare.RaspberryIO.Peripherals;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;
using System.Threading;
using Winofy.Connection;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Winofy.Connection.Devices;

namespace Winofy.HomeConsole
{
    public class HomeConfig
    {
        public const string ConfigPath = "config.json";

        public Device Device { get; set; }

        public BcmPin DhtSensorPin { get; set; } = BcmPin.Gpio00;

        public DhtType DhtType { get; set; } = DhtType.Dht11;

        public string AuthorizationToken { get; set; }

        public string Username { get; set; }

        public Threshold Threshold { get; set; } = new Threshold();
    }

    public class Threshold
    {
        public float Temperature { get; set; } = 40;

        public float SI { get; set; } = 25;
    }

    public class PWM
    {
        //13 or 18
        public BcmPin Pin { get; set; } = BcmPin.Gpio18;

        public int Range { get; set; } = 1024;

        public int Register { get; set; } = 512;

        [JsonIgnore()]
        public float DutyCycle => Register / (float)Range;
    }

    class Program
    {
        private static double? humidity;
        private static double? temperature;

        static async Task Main(string[] args)
        {
            HomeConfig config = GetConfig();
            var client = await PrepareClientAsync(config);

            Pi.Init<BootstrapWiringPi>();

            using (var dht = DhtSensor.Create(config.DhtType, Pi.Gpio[config.DhtSensorPin]))
            {
                dht.Start();
                dht.OnDataAvailable += Dht_OnDataAvailable;

                Console.WriteLine("Preparing vibration sensor...");
                var d7s = new D7s();
                d7s.Reset();

                while (d7s.GetState() != D7sState.Normal)
                {
                    Thread.Yield();
                }

                Console.WriteLine("Preparing DHT sensor...");
                while (!humidity.HasValue || !temperature.HasValue)
                {
                    Thread.Yield();
                }

                Console.WriteLine("Devices preparation finished");
                Console.WriteLine("Recording vibration, temperature, humidty will be started");

                var devId = config.Device?.Id;
                while (true)
                {
                    Thread.Yield();
                    var ax = d7s.GetAxis();
                    var si = d7s.ReadSI();
                    var h = humidity.Value;
                    var temp = temperature.Value;

                    if (!string.IsNullOrEmpty(devId))
                    {
                        await client.RecordAsync(devId, si, (Axes)ax, temp, h, )
                    }
                }
            }
        }

        private static void HandleTempAndSI(Threshold threshold, float temperature, float si)
        {
            if (si >= threshold.SI)
            {

                return;
            }

            if (temperature >= threshold.Temperature)
            {

            }
        }

        private static void SetPWM(PWM on, PWM off)
        {
            var pin = Pi.Gpio[]
        }

        private static void Dht_OnDataAvailable(object sender, DhtReadEventArgs e)
        {
            if (e.IsValid)
            {
                humidity = e.HumidityPercentage;
                temperature = e.Temperature;
            }
        }

        private static async Task<WinofyClient> PrepareClientAsync(HomeConfig config)
        {
            var client = new WinofyClient();

            if (!string.IsNullOrEmpty(config.AuthorizationToken))
            {
                var result = await client.ValidateTokenAsync(config.Username, config.AuthorizationToken);
                if (!result.Valid)
                {
                    config.AuthorizationToken = null;
                }
            }

            while (string.IsNullOrEmpty(config.AuthorizationToken))
            {
                Console.Write("Username : ");
                var username = Console.ReadLine();

                Console.Write("Password : ");
                var password = Console.ReadLine();

                var result = await client.LoginAsync(username, password);

                if (result.Success)
                {
                    config.Username = username;
                    config.AuthorizationToken = result.Token;
                }
                else
                {
                    Console.WriteLine("Failed to login. Please check your username and password");
                }
            }

            client.SetAuthorizationToken(config.AuthorizationToken);

            if (string.IsNullOrEmpty(config.Device?.Id))
            {
                Console.WriteLine("This device may not be registered");
                Console.WriteLine("Tell us the device name and description");
                Console.Write("Name : ");
                var name = Console.ReadLine();
                Console.Write("Description : ");
                var desc = Console.ReadLine();

                var device = new Device()
                {
                    Name = name,
                    Description = desc,
                    Notification = true
                };

                var result = await client.RegisterDeviceAsync(device, true);

                if (result.Success)
                {
                    config.Device = device;
                }
                else
                {
                    Console.WriteLine("Failed to register this device. See messages from the server");
                    Console.WriteLine(result.Message.ToString());
                }
            }

            using (var sw = new StreamWriter(HomeConfig.ConfigPath))
            {
                sw.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            return client;
        }

        private static HomeConfig GetConfig()
        {
            HomeConfig config = null;
            if (File.Exists(HomeConfig.ConfigPath))
            {
                using (var sr = new StreamReader(HomeConfig.ConfigPath))
                {
                    config = JsonConvert.DeserializeObject<HomeConfig>(sr.ReadToEnd());
                }
            }

            if (config == null)
            {
                config = new HomeConfig();
                Console.WriteLine("There is no config file");

                Console.WriteLine("Please provide the DHT sensor type");
                Console.WriteLine("DHT11 = 0");
                Console.WriteLine("DHT12 = 1");
                Console.WriteLine("DHT21 = 2");
                Console.WriteLine("DHT22 = 3");
                Console.WriteLine("AM2301 = 4");
                Console.WriteLine("AM2302 = 5");
                Console.Write("DHT sensor type : ");

                if (int.TryParse(Console.ReadLine(), out var dhtNum))
                {
                    config.DhtType = (DhtType)dhtNum;
                }

                Console.Write("DHT sensor pin : ");

                if (int.TryParse(Console.ReadLine(), out var pin))
                {
                    config.DhtSensorPin = (BcmPin)pin;
                }
            }

            return config;
        }
    }
}
