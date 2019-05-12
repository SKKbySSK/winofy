using System;

using Xamarin.Forms;
using Winofy.Account;

namespace Winofy.Pages
{
    public class AccountPage : ContentPage
    {
        private ScrollView ScrollView { get; } = new ScrollView();
        private StackLayout StackLayout { get; } = new StackLayout()
        {
            Padding = 10
        };

        private Label UsernameLabel { get; } = new Label()
        {
            FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
            TextColor = Color.White,
        };

#if DEBUG
        private Label TokenLabel { get; } = new Label()
        {
            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            TextColor = Color.White,
        };

        private Label NotificationLabel { get; } = new Label()
        {
            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            TextColor = Color.White,
        };
#endif

        private AccountCenter Account { get; }

        public AccountPage(AccountCenter account)
        {
            Title = "Account";
            Account = account;

            Account.LoggedIn += Account_LoggedIn;
            Account.LoggedOut += Account_LoggedOut;

            Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
            BackgroundColor = Color.Black;
            StackLayout.Children.Add(UsernameLabel);

#if DEBUG
            StackLayout.Children.Add(new Label() { Text = "Authorization Token", TextColor = Color.White });
            StackLayout.Children.Add(TokenLabel);

            StackLayout.Children.Add(new Label() { Text = "Notification Token", TextColor = Color.White });
            StackLayout.Children.Add(NotificationLabel);
            NotificationLabel.Text = Notification.FcmToken;
#endif

            ScrollView.Content = StackLayout;

            Content = ScrollView;
        }

        void Account_LoggedOut(object sender, EventArgs e)
        {
            UsernameLabel.Text = null;

#if DEBUG
            TokenLabel.Text = Account.Token;
            NotificationLabel.Text = Notification.FcmToken;
#endif
        }

        void Account_LoggedIn(object sender, EventArgs e)
        {
            UsernameLabel.Text = Account.Username;

#if DEBUG
            TokenLabel.Text = Account.Token;
            NotificationLabel.Text = Notification.FcmToken;
#endif
        }

    }
}

