using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Winofy.Extensions;
using Winofy.ViewModels;

namespace Winofy.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private LoginViewModel ViewModel => (LoginViewModel)Resources["viewModel"];

        public LoginPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            ViewModel.Authorized += ViewModel_Authorized;
        }

        async void ViewModel_Authorized(object sender, EventArgs e)
        {
            try
            {
                var notification = await Notification.SetNotificationAsync(ViewModel.Client, Connection.NotificationType.FCM);

                if (!notification.Success)
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Failed to register notification for this device");
                }
            }
            catch (Exception ex)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(ex.ToString(), "Unknown error occured");
            }

            var devices = new DevicesPage(ViewModel.Client, ViewModel.Username);
            await Navigation.PushAsync(devices);
        }
    }
}