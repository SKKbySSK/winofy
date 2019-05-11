using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Winofy.ViewModels;
using Winofy.Connection;
using System.IO;
using Winofy.Account;

namespace Winofy.Pages
{
    public partial class DevicesPage : ContentPage
    {
        private DevicesViewModel ViewModel => (DevicesViewModel)Resources["viewModel"];

        private string Username { get; }

        public DevicesPage(WinofyClient client, string username)
        {
            NavigationPage.SetHasNavigationBar(this, true);
            NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();

            Username = username;
            ViewModel.Client = client;
            _ = ViewModel.GetDevicesAsync();

            ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Logout",
                Command = new Command(Logout)
            });
        }

        private void Logout()
        {
            using (var fs = new FileStream(LoginData.LoginPath, FileMode.Create, FileAccess.Write))
            {
                LoginData.Export(new LoginData(Username, null), fs);
                Navigation.PopAsync();
            }
        }
    }
}
