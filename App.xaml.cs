using RealTalk_AI.Resources;
using System.Globalization;

namespace RealTalk_AI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            string lang = Preferences.Get("AppLanguage", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName ?? "en");
            LocalizationResourceManager.Instance.SetCulture(new CultureInfo(lang));

            MainPage = AuthService.IsUserAuthenticated()
                ? new AppShell()
                : new NavigationPage(new SignInPage());
        }
        protected override void OnSleep()
        {
            base.OnSleep();

            // Сохраняем состояние всего приложения, если это необходимо
            Preferences.Set("appState", "SomeAppState");
        }
    
        protected override void OnResume()
        {
            base.OnResume();

            // Загружаем состояние приложения, если оно нужно
            string appState = Preferences.Get("appState", null);
        }
    }
}
