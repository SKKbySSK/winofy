using System;
using Winofy.ViewModels.Abstracts;
using Winofy.Connection;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Winofy.Connection.Devices;
using Xamarin.Forms;
using Winofy.Account;

namespace Winofy.ViewModels
{
    public class DevicesViewModel : ViewModelBase
    {
        public DevicesViewModel()
        {
            RefreshDevices = new Command(() => _ = GetDevicesAsync());
        }

        public ObservableCollection<Connection.Devices.Device> Devices { get; } = new ObservableCollection<Connection.Devices.Device>();

        public AccountCenter Account { get; set; }

        public Command RefreshDevices { get; }

        public string Username
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool IsRefreshing
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public async Task GetDevicesAsync()
        {
            IsRefreshing = true;

            try
            {
                var dev = await Account.Client.ListDevicesAsync();
                Devices.Clear();

                if (dev.Devices == null) dev.Devices = new Connection.Devices.Device[0];
                foreach (var d in dev.Devices)
                {
                    Devices.Add(d);
                }
            }
            catch (WinofyClientException clex)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(clex.Url, "Client Error : " + clex.StatusCode);
            }
            catch (Exception ex)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(ex.ToString());
            }

            IsRefreshing = false;
        }
    }
}
