using RealTalk_AI.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealTalk_AI.ViewModels
{
    public class AppShellViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string username;
        public string Username
        {
            get => username;
            set
            {
                if (username != value)
                {
                    username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public AppShellViewModel()
        {
            Username = AuthService.GetUsername() ?? "Имя";

            LocalizationResourceManager.Instance.CultureChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(Home));
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(Search));
                OnPropertyChanged(nameof(AppName));
                OnPropertyChanged(nameof(LatestChats));
                OnPropertyChanged(nameof(Settings));
                OnPropertyChanged(nameof(ChatTopic));
            };
        }

        public void UpdateUsername()
        {
            Username = AuthService.GetUsername() ?? "Имя";
        }

        public string Home => LocalizationResourceManager.Instance["Home"];
        public string PageTitle => LocalizationResourceManager.Instance["PageTitle"];
        public string Search => LocalizationResourceManager.Instance["Search"];
        public string AppName => LocalizationResourceManager.Instance["AppName"];
        public string LatestChats => LocalizationResourceManager.Instance["LatestChats"];
        public string Settings => LocalizationResourceManager.Instance["Settings"];
        public string ChatTopic => LocalizationResourceManager.Instance["ChatTopic"];
        public string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
