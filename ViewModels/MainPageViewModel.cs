using RealTalk_AI.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealTalk_AI.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public MainPageViewModel()
    {
        LocalizationResourceManager.Instance.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Greetings));
            OnPropertyChanged(nameof(GreetingsDescription));
            OnPropertyChanged(nameof(EnterQuestion));
            OnPropertyChanged(nameof(CurrentLanguage));
        };
    }

    public string Greetings => LocalizationResourceManager.Instance["Greetings"];
    public string GreetingsDescription => LocalizationResourceManager.Instance["GreetingsDescription"];
    public string EnterQuestion => LocalizationResourceManager.Instance["EnterQuestion"];

    public string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}