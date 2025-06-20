public static class AuthService
{
    private const string TokenKey = "AuthToken";
    private const string EmailKey = "UserEmail";
    private const string UserIdKey = "UserId";
    private const string NicknameKey = "UserNickname";

    public static string GetAuthToken()
    {
        return Preferences.Get(TokenKey, string.Empty);
    }

    public static void SetAuthToken(string token)
    {
        Preferences.Set(TokenKey, token);
    }

    public static bool IsUserAuthenticated()
    {
        return !string.IsNullOrEmpty(GetAuthToken());
    }

    public static void Logout()
    {
        Preferences.Remove(TokenKey);
        Preferences.Remove(EmailKey);
        Preferences.Remove(UserIdKey);
        Preferences.Remove(NicknameKey);
    }

    public static void SetUserEmail(string email)
    {
        Preferences.Set(EmailKey, email);
    }

    public static string GetUserEmail()
    {
        return Preferences.Get(EmailKey, string.Empty);
    }

    public static void SetUserId(string userId)
    {
        Preferences.Set(UserIdKey, userId);
    }

    public static string GetUserId()
    {
        return Preferences.Get(UserIdKey, string.Empty);
    }

    public static void SetUsername(string nickname)
    {
        Preferences.Set(NicknameKey, nickname);
    }

    public static string GetUsername()
    {
        return Preferences.Get(NicknameKey, string.Empty);
    }
}
