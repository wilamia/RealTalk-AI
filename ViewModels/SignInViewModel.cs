using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealTalk_AI.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealTalk_AI.ViewModels;

public class SignInViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public SignInViewModel()
    {
        LocalizationResourceManager.Instance.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(LoginTo));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(EnterEmail));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(EnterPassword));
            OnPropertyChanged(nameof(ForgotPassword));
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(NoAccount));
            OnPropertyChanged(nameof(CreateAccount));
        };
    }

    public string LoginTo => LocalizationResourceManager.Instance["LoginTo"];
    public string Email => LocalizationResourceManager.Instance["Email"];
    public string EnterEmail => LocalizationResourceManager.Instance["EnterEmail"];
    public string Password => LocalizationResourceManager.Instance["Password"];
    public string EnterPassword => LocalizationResourceManager.Instance["EnterPassword"];
    public string ForgotPassword => LocalizationResourceManager.Instance["ForgotPassword"];
    public string Login => LocalizationResourceManager.Instance["Login"];
    public string NoAccount => LocalizationResourceManager.Instance["NoAccount"];
    public string CreateAccount => LocalizationResourceManager.Instance["CreateAccount"];
    public string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
