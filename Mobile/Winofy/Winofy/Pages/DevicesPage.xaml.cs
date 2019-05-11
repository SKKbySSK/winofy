using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Winofy.ViewModels;
using Winofy.Connection;

namespace Winofy.Pages
{
    public partial class DevicesPage : ContentPage
    {
        private DevicesViewModel ViewModel => (DevicesViewModel)Resources["viewModel"];

        public DevicesPage(WinofyClient client)
        {
            NavigationPage.SetHasNavigationBar(this, true);
            InitializeComponent();

            ViewModel.Client = client;
            _ = ViewModel.GetDevicesAsync();
        }
    }
}
