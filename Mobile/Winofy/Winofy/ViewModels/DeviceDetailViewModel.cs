using System;
using Winofy.ViewModels.Abstracts;
using Winofy.Connection.Devices;
namespace Winofy.ViewModels
{
    public class DeviceDetailViewModel : ViewModelBase
    {
        public Device Device
        {
            get => GetValue<Device>();
            set => SetValue(value);
        }
    }
}
