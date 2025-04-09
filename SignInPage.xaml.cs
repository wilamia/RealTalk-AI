using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Text.Json;

namespace RealTalk_AI
{
    public partial class SignInPage : ContentPage
    {
        private FirebaseAuthClient authClient;
        private bool isPasswordVisible = false;

        public SignInPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);

            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCdTJg_iSeWoX2Ete3-8emKdqBVnY71AIA",
                AuthDomain = "realtalk-ai.firebaseapp.com",
                Providers = new FirebaseAuthProvider[] { new EmailProvider() },
                UserRepository = new FileUserRepository("RealTalk_AI")
            };

            authClient = new FirebaseAuthClient(config);
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            string email = emailEntry.Text;
            string password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите почту и пароль!", "OK");
                return;
            }

            try
            {
             
                var authResult = await authClient.SignInWithEmailAndPasswordAsync(email, password);
                var user = authResult.User;

                string idToken = await user.GetIdTokenAsync();

                AuthService.SetAuthToken(idToken);

                AuthService.SetUserEmail(user.Info.Email);

                await FetchAndSaveUserInfo(user.Info.Email);

                await DisplayAlert("Успех", $"Добро пожаловать, {user.Info.Email}", "OK");

                Application.Current.MainPage = new NavigationPage(new AppShell());
            }
            catch (FirebaseAuthException ex)
            {
                await DisplayAlert("Ошибка входа", ex.Reason.ToString(), "OK");
            }
        }

      
        private async Task FetchAndSaveUserInfo(string email)
        {
            try
            {
                var httpClient = new HttpClient();
                string encodedEmail = email.Replace("@", "_").Replace(".", "_");

                var response = await httpClient.GetAsync($"https://realtalk-ai-default-rtdb.firebaseio.com/users/{encodedEmail}.json");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(json);
                    // Сохраняем
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось загрузить данные из Firebase", "OK");
                }
            }
            catch (FirebaseException firebaseEx)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки данных из Firebase: {firebaseEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки данных: {ex.Message}", "OK");
            }
        }

        private void OnEyeTapped(object sender, EventArgs e)
        {
            if (isPasswordVisible)
            {
                passwordEntry.IsPassword = true;
                ((Image)sender).Source = "eye_close.png";
            }
            else
            {
                passwordEntry.IsPassword = false;
                ((Image)sender).Source = "eye_open.svg";
            }

            isPasswordVisible = !isPasswordVisible;
        }

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ResetPasswordPage());
        }
        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Username { get; set; } 
    }
}
