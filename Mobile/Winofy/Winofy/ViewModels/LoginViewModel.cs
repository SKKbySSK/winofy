using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Winofy.Account;
using Winofy.Connection;
using System.Diagnostics;
namespace Winofy.ViewModels
{
    public class LoginViewModel : Abstracts.ViewModelBase
    {

        public LoginViewModel()
        {
            IsAuthorizing = false;
            LoginCommand = new Command(() => _ = AuthorizeAsync(Username, Password));
            RegisterCommand = new Command(() => _ = RegisterAsync(Username, Password));
            if (File.Exists(LoginData.LoginPath))
            {
                using (var fs = new FileStream(LoginData.LoginPath, FileMode.Open, FileAccess.Read))
                {
                    var login = LoginData.Open(fs);

                    Username = login.Username;
                    Token = login.AuthorizingToken;
                    if (login == null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.AuthorizingToken))
                    {
                        return;
                    }

                    _ = AuthorizeAsync(login.Username, null, login.AuthorizingToken);
                }

                return;
            }
        }

        public event EventHandler Authorized;

        public WinofyClient Client { get; } = new WinofyClient();

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

            try
            {
                if (string.IsNullOrEmpty(token) || !(await Client.ValidateTokenAsync(username, token)).Valid)
                {
                    if (string.IsNullOrWhiteSpace(password) && string.IsNullOrEmpty(token))
                    {
                        PasswordErrorMessage = "You must provide password";
                        IsAuthorizing = false;
                        return;
                    }

                    var result = await Client.LoginAsync(username, password);

                    if (result.Success)
                    {
                        token = result.Token;
                    }
                }
            }
            catch (WinofyClientException clex)
            {
                Acr.UserDialogs.UserDialogs.Instance.Alert(clex.Url, "Client Error : " + clex.StatusCode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Acr.UserDialogs.UserDialogs.Instance.Alert(ex.ToString());
            }

            if (!string.IsNullOrEmpty(token))
            {
                using (var fs = new FileStream(LoginData.LoginPath, FileMode.Create, FileAccess.Write))
                {
                    LoginData.Export(new LoginData(username, token), fs);
                }

                Client.SetAuthorizationToken(token);
                Authorized?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = "Failed to login";
            }

            IsAuthorizing = false;
        }

        public async Task RegisterAsync(string username, string password)
        {
            ErrorMessage = null;
            UsernameErrorMessage = null;
            PasswordErrorMessage = null;
            IsAuthorizing = true;

            try
            {
                var res = await Client.RegisterAsync(username, password);

                foreach (var msg in res.Messages)
                {
                    switch (msg)
                    {
                        case RegisterMessage.IncorrectUsernameFormat:
                            UsernameErrorMessage = "Incorrect username format";
                            break;
                        case RegisterMessage.IncorrectPasswordFormat:
                            PasswordErrorMessage = "Incorrect password format";
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
            }
            catch (WinofyClientException clex)
            {
                Acr.UserDialogs.UserDialogs.Instance.Alert(clex.Url, "Client Error : " + clex.StatusCode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Acr.UserDialogs.UserDialogs.Instance.Alert(ex.ToString());
            }

            IsAuthorizing = false;
        }
    }
}
