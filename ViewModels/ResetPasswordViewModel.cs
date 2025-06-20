using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using RealTalk_AI.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealTalk_AI.ViewModels
{
    class ResetPasswordViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ResetPasswordViewModel()
        {
            LocalizationResourceManager.Instance.CultureChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(RecoverPassword));
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(EnterEmail));
                OnPropertyChanged(nameof(AlreadyHave));
                OnPropertyChanged(nameof(Login));
            };
        }

        public string RecoverPassword => LocalizationResourceManager.Instance["RecoverPassword"];
        public string Email => LocalizationResourceManager.Instance["Email"];
        public string EnterEmail => LocalizationResourceManager.Instance["EnterEmail"];
        public string AlreadyHave => LocalizationResourceManager.Instance["AlreadyHave"];
        public string Login => LocalizationResourceManager.Instance["Login"];
        public string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
