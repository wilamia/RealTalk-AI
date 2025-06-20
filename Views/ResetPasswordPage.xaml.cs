using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;

namespace RealTalk_AI;

public partial class ResetPasswordPage : ContentPage
{
    private FirebaseAuthClient authClient;
    public ResetPasswordPage()
	{
		InitializeComponent();
        BindingContext = new ViewModels.ResetPasswordViewModel();
        NavigationPage.SetHasNavigationBar(this, false);

        var config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyCdTJg_iSeWoX2Ete3-8emKdqBVnY71AIA",
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
        await Navigation.PushAsync(new SignInPage());
    }
    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text;

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Ошибка", "Введите почту", "OK");
            return;
        }

        try
        {
            await authClient.ResetEmailPasswordAsync(email);
            await DisplayAlert("Успех", "Письмо для сброса пароля отправлено!", "OK");
            await Navigation.PushAsync(new SignInPage());
        }
        catch (FirebaseAuthException ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка сброса пароля: {ex.Reason}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }
}