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
using Unosquare.WiringPi.Native;
using Microsoft.Win32.SafeHandles;

namespace Winofy.HomeConsole
{
    public class HomeConfig
    {
        public const string ConfigPath = "config.json";

        public Device Device { get; set; }

        public int D7sAddress { get; set; } = D7sAddresses.Address;

        public BcmPin DhtSensorPin { get; set; } = BcmPin.Gpio00;

        public DhtType DhtType { get; set; } = DhtType.Dht11;

        public string AuthorizationToken { get; set; }

        public string Username { get; set; }

        public Threshold Threshold { get; set; } = new Threshold();

        public PWM PWM1 { get; set; } = PWM.GPIO13;

        public PWM PWM2 { get; set; } = PWM.GPIO18;
    }

    public class Threshold
    {
        public float Temperature { get; set; } = 40;

        public float SI { get; set; } = 25;
    }

    public class PWM
    {
        public static PWM GPIO13 => new PWM() { Pin = BcmPin.Gpio13 };

        public static PWM GPIO18 => new PWM() { Pin = BcmPin.Gpio18 };

        //13 or 18
        //See https://github.com/unosquare/raspberryio#using-the-gpio-pins
        public BcmPin Pin { get; set; } = BcmPin.Gpio23;

        public double DutyCycle { get; set; } = 0.5;

        public int DelayMs { get; set; } = 3000;

        //See https://raspberrypi.stackexchange.com/questions/4906/control-hardware-pwm-frequency/9725#9725
        public int Frequency { get; set; } = 2000;
    }

    class Program
    {
        private static double? humidity;
        private static double? temperature;

        static async Task Main(string[] args)
        {
            HomeConfig config = GetConfig();
            var client = await PrepareClientAsync(config);

            await client.RecordAsync(config.Device.Id, 10, Axes.XY, 30, 0.9f, WindowState.Closed);

            Pi.Init<BootstrapWiringPi>();

            using (var dht = DhtSensor.Create(config.DhtType, Pi.Gpio[config.DhtSensorPin]))
            {
                dht.Start();
                dht.OnDataAvailable += Dht_OnDataAvailable;

                Console.WriteLine("Preparing vibration sensor...");
                var d7s = new D7s(config.D7sAddress);
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
                    var h = (float)humidity.Value;
                    var temp = (float)temperature.Value;

                    if (!string.IsNullOrEmpty(devId))
                    {
                        var window = HandleTempAndSI(config, temp, si);
                        await client.RecordAsync(devId, si, (Axes)ax, temp, h, window);
                    }
                }
            }
        }

        private static WindowState HandleTempAndSI(HomeConfig config, float temperature, float si)
        {
            if (si >= config.Threshold.SI)
            {
                Open(config);
                return WindowState.Opened;
            }

            if (temperature >= config.Threshold.Temperature)
            {
                Close(config);
                return WindowState.Closed;
            }

            return WindowState.None;
        }

        private static void SetPWM(PWM pwm, bool on)
        {
            var pin = (Unosquare.WiringPi.GpioPin)Pi.Gpio[pwm.Pin];
            if (on)
            {
                pin.PinMode = GpioPinDriveMode.PwmOutput;
                pin.PwmRange = 1024;
                pin.PwmRegister = (int)(pin.PwmRange * pwm.DutyCycle);
                pin.PwmClockDivisor = (int)(19.2e6 / pwm.Frequency / pin.PwmRange);
            }
            else
            {
                pin.PinMode = GpioPinDriveMode.Output;
                pin.Write(GpioPinValue.Low);
            }
        }

        private static void Open(HomeConfig config)
        {
            SetPWM(config.PWM1, true);
            SetPWM(config.PWM2, false);
            Thread.Sleep(config.PWM1.DelayMs);
        }

        private static void Close(HomeConfig config)
        {
            SetPWM(config.PWM2, true);
            SetPWM(config.PWM1, false);
            Thread.Sleep(config.PWM2.DelayMs);
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
