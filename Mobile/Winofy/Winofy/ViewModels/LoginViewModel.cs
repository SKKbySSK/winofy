using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace Winofy.ViewModels
{
    public class LoginViewModel : Abstracts.ViewModelBase
    {
        public LoginViewModel()
        {
            IsAuthorizing = false;
            LoginCommand = new Command(() => AuthorizeAsync(Username, Password));
            var loginPath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "login.json");
            if (File.Exists(loginPath))
            {
                using (var fs = new FileStream(loginPath, FileMode.Open, FileAccess.Read))
                {
                    LoginData = Account.LoginData.Open(fs);
                }
            }
        }

        public Account.LoginData LoginData
        {
            get => GetValue<Account.LoginData>();
            set => SetValue(value);
        }

        public bool IsAuthorizing
        {
            get => GetValue<bool>();
            private set => SetValue(value);
        }

        public string UsernameErrorMessage
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public string PasswordErrorMessage
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public string Username
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public string Password
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public Command LoginCommand { get; }

        public async Task AuthorizeAsync(string username, string password)
        {
            UsernameErrorMessage = null;
            PasswordErrorMessage = null;
            IsAuthorizing = true;
            await Task.Delay(3000);
            IsAuthorizing = false;
            UsernameErrorMessage = "Incorrect username";
            PasswordErrorMessage = "Incorrect password";
            LoginData = new Account.LoginData("", DateTime.Now);
        }
    }
}
