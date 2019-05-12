using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Winofy.ViewModels;

namespace Winofy.Pages
{
    public partial class DeviceDetailPage : ContentPage
    {
        private DeviceDetailViewModel ViewModel => (DeviceDetailViewModel)Resources["viewModel"];

        public DeviceDetailPage(Connection.Devices.Device device)
        {
            InitializeComponent();
            ViewModel.Device = device;
        }
    }
}
