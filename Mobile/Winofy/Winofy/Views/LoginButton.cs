using System;
using Xamarin.Forms;

namespace Winofy.Views
{
    public class LoginButton : Flex.Controls.FlexButton
    {
        public LoginButton()
        {
            UpdateIcon();
        }

        public static readonly BindableProperty ServiceProperty = BindableProperty.Create(nameof(Service), typeof(Services.ServiceType), typeof(LoginButton), default(Services.ServiceType), propertyChanged: (obj, o, n) => ((LoginButton)obj).ServicePropertyChanged((Services.ServiceType)o, (Services.ServiceType)n));

        private void ServicePropertyChanged(Services.ServiceType oldValue, Services.ServiceType newValue)
        {
            UpdateIcon();
        }

        public Services.ServiceType Service
        {
            get { return (Services.ServiceType)GetValue(ServiceProperty); }
            set { SetValue(ServiceProperty, value); }
        }

        private void UpdateIcon()
        {
            switch (Service)
            {
                case Services.ServiceType.Google:
                    Icon = "Google";
                    break;
            }
        }
    }
}
