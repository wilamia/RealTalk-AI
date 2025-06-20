using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealTalk_AI.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Firebase.Auth.Requests;

namespace RealTalk_AI.ViewModels;

public class SignUpViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public SignUpViewModel()
    {
        LocalizationResourceManager.Instance.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CreateAccount));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(EnterEmail));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(EnterPassword));
            OnPropertyChanged(nameof(RepeatPassword));
            OnPropertyChanged(nameof(AlreadyHave));
            OnPropertyChanged(nameof(Login));
        };
    }

    public string CreateAccount => LocalizationResourceManager.Instance["CreateAccount"];
    public string Email => LocalizationResourceManager.Instance["Email"];
    public string EnterEmail => LocalizationResourceManager.Instance["EnterEmail"];
    public string Password => LocalizationResourceManager.Instance["Password"];
    public string EnterPassword => LocalizationResourceManager.Instance["EnterPassword"];
    public string RepeatPassword => LocalizationResourceManager.Instance["RepeatPassword"];
    public string AlreadyHave => LocalizationResourceManager.Instance["AlreadyHave"];
        public string Login => LocalizationResourceManager.Instance["Login"];
    public string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

