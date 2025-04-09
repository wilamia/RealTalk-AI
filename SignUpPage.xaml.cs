using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Threading.Tasks;

namespace RealTalk_AI
{
    public partial class SignUpPage : ContentPage
    {
        private readonly FirebaseAuthClient _authClient;
        private readonly FirebaseClient _databaseClient;
        private bool firstIsPasswordVisible = false;
        private bool isPasswordVisible = false;

        public SignUpPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _authClient = new FirebaseAuthClient(new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCdTJg_iSeWoX2Ete3-8emKdqBVnY71AIA",
                AuthDomain = "realtalk-ai.firebaseapp.com",
                Providers = new FirebaseAuthProvider[] { new EmailProvider() }
            });

            _databaseClient = new FirebaseClient("https://realtalk-ai-default-rtdb.firebaseio.com/");
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            string email = emailEntry.Text;
            string password = passwordEntry.Text;
            string confirmPassword = confirmPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите почту и пароль!", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Ошибка", "Пароли не совпадают!", "OK");
                return;
            }

            try
            {
                var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);

                string randomUsername = GenerateRandomUsername();

                await SaveUserToDatabase(email, randomUsername);

                await DisplayAlert("Успех", "Аккаунт создан!", "OK");
                await Navigation.PushAsync(new SignInPage());
            }
            catch (FirebaseAuthException ex)
            {
                await DisplayAlert("Ошибка регистрации", ex.Reason.ToString(), "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private string GenerateRandomUsername()
        {
            string[] adjectives = { "Fast", "Happy", "Smart", "Brave", "Cool", "Funny", "Lucky", "Clever" };
            string[] nouns = { "Tiger", "Panda", "Eagle", "Wolf", "Dolphin", "Fox", "Hawk", "Lion" };
            Random rnd = new Random();
            return $"{adjectives[rnd.Next(adjectives.Length)]}{nouns[rnd.Next(nouns.Length)]}{rnd.Next(1000, 9999)}";
        }

        private async Task SaveUserToDatabase(string email, string username)
        {
            
            string encodedEmail = email.Replace("@", "_").Replace(".", "_");

            var user = new
            {
                Email = email,
                Username = username,
            };

            
            await _databaseClient
                .Child("users")
                .Child(encodedEmail)  
                .PutAsync(user);
        }

        private void OnFirstEyeTapped(object sender, EventArgs e)
        {
            passwordEntry.IsPassword = !firstIsPasswordVisible;
            ((Image)sender).Source = !firstIsPasswordVisible ? "eye_close.png" : "eye_open.svg";
            firstIsPasswordVisible = !firstIsPasswordVisible;
        }

        private void OnEyeTapped(object sender, EventArgs e)
        {
            confirmPasswordEntry.IsPassword = !isPasswordVisible;
            ((Image)sender).Source = isPasswordVisible ? "eye_close.png" : "eye_open.svg";
            isPasswordVisible = !isPasswordVisible;
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignInPage());
        }
    }
}
