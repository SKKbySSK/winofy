using System;
using System.IO;
using System.Threading.Tasks;
using Winofy.Connection;
using System.Diagnostics;
namespace Winofy.Account
{
    public class AccountCenter
    {
        public WinofyClient Client { get; } = new WinofyClient();

        public string Username { get; private set; }

        public string Token { get; private set; }

        public bool IsAuthorized { get; private set; } = false;

        public async Task<string> LoginAsync(string username, string password, NotificationType notificationType = NotificationType.FCM)
        {
            Token = null;
            IsAuthorized = false;

            if (IsAuthorized)
            {
                await LogoutAsync();
            }

            var result = await Client.LoginAsync(username, password);

            if (result.Success)
            {
                IsAuthorized = true;
                Username = username;
                Token = result.Token;

                Client.SetAuthorizationToken(Token);
                await SetNotificationAsync(notificationType);
            }

            return Token;
        }

        public async Task<bool> LoginWithTokenAsync(string username, string token, NotificationType notificationType = NotificationType.FCM)
        {
            Token = null;
            IsAuthorized = false;

            if (IsAuthorized)
            {
                await LogoutAsync();
            }

            var result = await Client.ValidateTokenAsync(username, token);

            if (result.Valid)
            {
                IsAuthorized = true;
                Username = username;
                Token = token;

                Client.SetAuthorizationToken(Token);
                await SetNotificationAsync(notificationType);
            }

            return result.Valid;
        }

        public async Task LogoutAsync()
        {
            try
            {
                Username = null;
                Token = null;
                IsAuthorized = false;

                var res = await Client.UpdateNotificationAsync(Notification.FcmToken, NotificationType.Disabled);

                if (res.Success)
                {
                    Debug.WriteLine("Notification Unregistered");
                }
                else
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Failed to unregister notification");
                }

                Client.SetAuthorizationToken(null);
            }
            catch (Exception ex)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(ex.ToString(), "Unknown error occured");
            }
        }

        private async Task SetNotificationAsync(NotificationType type)
        {
            try
            {
                var notification = await Notification.SetNotificationAsync(Client, type);

                if (notification.Success)
                {
                    Debug.WriteLine("Notification Registered");
                }
                else
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Failed to set notification");
                }
            }
            catch (Exception ex)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(ex.ToString(), "Unknown error occured");
            }
        }
    }
}
