using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Winofy.ViewModels;
using Winofy.Connection;
using System.IO;
using Winofy.Account;
using System.Threading.Tasks;

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
