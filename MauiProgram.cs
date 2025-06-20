using Firebase.Auth.Providers;
using Firebase.Auth;
using Microsoft.Extensions.Logging;
namespace RealTalk_AI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        const string firebaseApiKey = "";
        var config = new FirebaseAuthConfig
        {
            ApiKey = firebaseApiKey,
            AuthDomain = "realtalk-ai.firebaseapp.com",
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()  
            }
        };
#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
