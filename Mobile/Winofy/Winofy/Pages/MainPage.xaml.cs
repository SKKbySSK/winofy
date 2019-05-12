using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Winofy.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        private LoginPage LoginPage { get; } = new LoginPage();
        private AccountPage AccountPage { get; }

        public MainPage()
        {
            AccountPage = new AccountPage(LoginPage.Account);
            InitializeComponent();

            Detail = new NavigationPage(LoginPage);
            Master = AccountPage;
        }
    }
}