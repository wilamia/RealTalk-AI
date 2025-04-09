using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace RealTalk_AI
{
    public partial class AppShell : Shell
    {

        public AppShell()
        {
            InitializeComponent();
            BindingContext = this;
            TitleMain.Text = "SmartTalk AI";
            Username.Text = AuthService.GetNickname() ?? "Имя";
            PageTitle = "Главная";
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
 
        }
        private string _pageTitle;
        public string PageTitle
        {
            get => _pageTitle;
            set 
            {
                _pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        public void SetTitle(string title)
        {
            PageTitle = title;
            TitleMain.Text = PageTitle;
        }
        private async void Logout(object sender, EventArgs e)
        {
            AuthService.Logout();

            Microsoft.Maui.Controls.Application.Current.MainPage = new SignInPage();
        }

        private async void OnTapped(object sender, EventArgs e)
        {          
            await Shell.Current.GoToAsync("//MainPage");
            Shell.Current.FlyoutIsPresented = false;
        }

        //private void OnMenuButtonClicked(object sender, EventArgs e)
        //{
        //    Current.FlyoutIsPresented = !Current.FlyoutIsPresented; // Открывает или закрывает меню
        //}

        //private async void OnLogoutClicked(object sender, EventArgs e)
        //{
        //    bool confirm = await DisplayAlert("Выход", "Вы уверены, что хотите выйти?", "Да", "Нет");
        //    if (confirm)
        //    {
        //        await Shell.Current.GoToAsync("//LoginPage");
        //    }
        //}
    }
}
