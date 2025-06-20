using System.ComponentModel;
using System.Globalization;

namespace RealTalk_AI.Resources
{
    public class LocalizationResourceManager : INotifyPropertyChanged
    {
        private static LocalizationResourceManager _instance;
        public static LocalizationResourceManager Instance => _instance ??= new LocalizationResourceManager();

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler CultureChanged; // 👈 новое событие

        private CultureInfo _culture;
        public CultureInfo CurrentCulture
        {
            get => _culture;
            private set
            {
                if (_culture != value)
                {
                    _culture = value;
                    OnPropertyChanged(null);
                    CultureChanged?.Invoke(this, EventArgs.Empty); // 👈 уведомим всех подписчиков
                }
            }
        }

        public string this[string key] => AppResources.ResourceManager.GetString(key, _culture);

        public void SetCulture(CultureInfo culture)
        {
            CurrentCulture = culture;
        }

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}