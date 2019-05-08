using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
namespace Winofy.ViewModels.Abstracts
{
    public class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            lock (properties)
            {
                if (properties.ContainsKey(propertyName))
                {
                    return (T)properties[propertyName];
                }

                return default;
            }
        }

        protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (!Xamarin.Essentials.MainThread.IsMainThread)
            {
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() => SetValue(value, propertyName));
                return;
            }

            if (GetValue<T>(propertyName)?.Equals(value) ?? false) return;

            lock (properties)
            {
                OnPropertyChanging(propertyName);
                properties[propertyName] = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
