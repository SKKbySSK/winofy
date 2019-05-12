using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Winofy.ViewModels;
using Winofy.Connection;
using System.IO;
using Winofy.Account;
using System.Threading.Tasks;
using Winofy.Connection.Devices;

namespace Winofy.Pages
{
    public partial class DevicesPage : ContentPage
    {
        private DevicesViewModel ViewModel => (DevicesViewModel)Resources["viewModel"];

        public DevicesPage(AccountCenter account)
        {
            NavigationPage.SetHasNavigationBar(this, true);
            NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();

            ViewModel.Account = account;
            _ = ViewModel.GetDevicesAsync();

            ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Logout",
                Command = new Command(() => _ = LogoutAsync())
            });

            lv.ItemTapped += Lv_ItemTapped;
        }

        async void Lv_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            lv.ItemTapped -= Lv_ItemTapped;
            if (e.Item is Winofy.Connection.Devices.Device dev)
            {
                await Navigation.PushAsync(new DeviceDetailPage(dev));
            }
            lv.ItemTapped += Lv_ItemTapped;
        }

        private async Task LogoutAsync()
        {
            using (var fs = new FileStream(LoginData.DefaultLoginPath, FileMode.Create, FileAccess.Write))
            {
                LoginData.Export(new LoginData(ViewModel.Account.Username, null), fs);
                await ViewModel.Account.LogoutAsync();
                await Navigation.PopAsync();
            }
        }
    }
}
