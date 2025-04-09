namespace RealTalk_AI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();


            if (AuthService.IsUserAuthenticated())
            {
                MainPage = new AppShell();
            }
            else
            {
                MainPage = new NavigationPage(new SignInPage());
            }
        }
    }
}
