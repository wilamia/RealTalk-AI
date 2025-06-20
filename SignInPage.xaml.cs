using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
namespace RealTalk_AI;

    public partial class SignInPage : ContentPage
    {
        private FirebaseAuthClient authClient;
    private bool isPasswordVisible = false;

    public SignInPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            var config = new FirebaseAuthConfig
            {
                ApiKey = "",
                AuthDomain = "realtalk-ai.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
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
                await DisplayAlert("������", "������� ����� � ������!", "OK");
                return;
            }

            try
            {
                var authResult = await authClient.SignInWithEmailAndPasswordAsync(email, password);
                var user = authResult.User;

                await DisplayAlert("�����", $"����� ����������, {user.Info.Email}", "OK");

                await Navigation.PushAsync(new MainPage());
            }
            catch (FirebaseAuthException ex)
            {
                await DisplayAlert("������ �����", ex.Reason.ToString(), "OK");
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
    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}