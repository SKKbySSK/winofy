using System;
using Winofy.ViewModels.Abstracts;
using Winofy.Connection;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Winofy.Connection.Devices;
namespace Winofy.ViewModels
{
    public class DevicesViewModel : ViewModelBase
    {
        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public WinofyClient Client { get; set; }

        public async Task GetDevicesAsync()
        {
            try
            {
                var dev = await Client.ListDevicesAsync();
                Devices.Clear();

                foreach (var d in dev.Devices)
                {
                    Devices.Add(d);
                }
            }
            catch (WinofyClientException clex)
            {
                Acr.UserDialogs.UserDialogs.Instance.Alert(clex.Url, "Client Error : " + clex.StatusCode);
            }
            catch (Exception ex)
            {
                Acr.UserDialogs.UserDialogs.Instance.Alert(ex.ToString());
            }
        }
    }
}
