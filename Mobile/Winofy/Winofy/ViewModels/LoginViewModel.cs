using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Winofy.Account;
using Winofy.Connection;
namespace Winofy.ViewModels
{
    public class LoginViewModel : Abstracts.ViewModelBase
    {
        private string loginPath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "login.json");

        public LoginViewModel()
        {
            IsAuthorizing = false;
            LoginCommand = new Command(() => AuthorizeAsync(Username, Password));
            RegisterCommand = new Command(() => RegisterAsync(Username, Password));
            if (File.Exists(loginPath))
            {
                using (var fs = new FileStream(loginPath, FileMode.Open, FileAccess.Read))
                {
                    var login = LoginData.Open(fs);
                    if (login == null)
                    {
                        return;
                    }
                    Username = login.Username;
                    Token = login.AuthorizingToken;
                    AuthorizeAsync(login.Username, null, login.AuthorizingToken);
                }

                return;
            }
        }

        public event EventHandler Authorized;

        private WinofyClient Communicator { get; } = new WinofyClient();

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

        public string ErrorMessage
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public string Username
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Password
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Token
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public Command LoginCommand { get; }

        public Command RegisterCommand { get; }

        public async Task AuthorizeAsync(string username, string password, string token = null)
        {
            ErrorMessage = null;
            UsernameErrorMessage = null;
            PasswordErrorMessage = null;
            IsAuthorizing = true;

            if (string.IsNullOrWhiteSpace(username))
            {
                UsernameErrorMessage = "You must provide username";
                IsAuthorizing = false;
                return;
            }

            if (string.IsNullOrEmpty(token) || !(await Communicator.ValidateTokenAsync(username, token)).Valid)
            {
                if (string.IsNullOrWhiteSpace(password) && string.IsNullOrEmpty(token))
                {
                    PasswordErrorMessage = "You must provide password";
                    IsAuthorizing = false;
                    return;
                }

                var result = await Communicator.LoginAsync(username, password);

                if (result.Success)
                {
                    token = result.Token;
                }
            }

            if (!string.IsNullOrEmpty(token))
            {
                using (var fs = new FileStream(loginPath, FileMode.Create, FileAccess.Write))
                {
                    LoginData.Export(new LoginData(username, token), fs);
                }

                Communicator.SetAuthorizationToken(token);
                Authorized?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = "Could not login to " + username;
            }

            IsAuthorizing = false;
        }

        public async Task RegisterAsync(string username, string password)
        {
            ErrorMessage = null;
            UsernameErrorMessage = null;
            PasswordErrorMessage = null;
            IsAuthorizing = true;

            var res = await Communicator.RegisterAsync(username, password);

            foreach (var msg in res.Messages)
            {
                switch (msg)
                {
                    case RegisterMessage.IncorrectUsername:
                        UsernameErrorMessage = "Incorrect username";
                        break;
                    case RegisterMessage.IncorrectPassword:
                        PasswordErrorMessage = "Incorrect password";
                        break;
                    case RegisterMessage.InvalidRequest:
                        ErrorMessage = "Application error";
                        break;
                    case RegisterMessage.Unknown:
                        ErrorMessage = "Unknown error";
                        break;
                    case RegisterMessage.UserExists:
                        UsernameErrorMessage = "User already exists";
                        break;
                }
            }

            if (res.Success)
            {
                await AuthorizeAsync(username, password);
            }

            IsAuthorizing = false;
        }
    }
}
