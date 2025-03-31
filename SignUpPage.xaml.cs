using Firebase.Auth.Providers;
using Firebase.Auth;
namespace RealTalk_AI;

public partial class SignUpPage : ContentPage
{
    private readonly FirebaseAuthClient _authClient;
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
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()
            }
        });
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
    private void OnFirstEyeTapped(object sender, EventArgs e)
    {
        if (firstIsPasswordVisible)
        {

            passwordEntry.IsPassword = true;

            ((Image)sender).Source = "eye_close.png";
        }
        else
        {

            passwordEntry.IsPassword = false;

            ((Image)sender).Source = "eye_open.svg";
        }

        firstIsPasswordVisible = !firstIsPasswordVisible;
    }
    private void OnEyeTapped(object sender, EventArgs e)
    {
        if (isPasswordVisible)
        {

            confirmPasswordEntry.IsPassword = true;

            ((Image)sender).Source = "eye_close.png";
        }
        else
        {

            confirmPasswordEntry.IsPassword = false;

            ((Image)sender).Source = "eye_open.svg";
        }

        // Меняем флаг на противоположный
        isPasswordVisible = !isPasswordVisible;
    }
    private async void OnSignInClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage());
    }
}