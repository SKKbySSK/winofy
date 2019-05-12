using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Winofy.Extensions;
using Winofy.ViewModels;
using Winofy.Account;

namespace Winofy.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private LoginViewModel ViewModel => (LoginViewModel)Resources["viewModel"];

        public AccountCenter Account => ViewModel.Account;

        public LoginPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            ViewModel.Authorized += ViewModel_Authorized;
        }

        async void ViewModel_Authorized(object sender, EventArgs e)
        {
            var devices = new DevicesPage(ViewModel.Account);
            await Navigation.PushAsync(devices);
        }
    }
}