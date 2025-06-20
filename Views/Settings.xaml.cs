using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Storage;
using RealTalk_AI.Resources; // ��� Preferences

namespace RealTalk_AI;

public partial class Settings : ContentPage
{
    public Settings()
    {
        InitializeComponent();

        // ��������� ���������� ���� ��� ���������� ������� ���� �� �� ���������
        string lang = Preferences.Get("AppLanguage", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName ?? "en");
        SetLanguage(lang);
    }

    private void OnRussianTapped(object sender, TappedEventArgs e)
    {
        SetLanguage("ru");
    }

    private void OnEnglishTapped(object sender, TappedEventArgs e)
    {
        SetLanguage("en");
    }
    private void OnLithuanianTapped(object sender, TappedEventArgs e)
    {
        SetLanguage("lt");
    }

    private void OnSpanishTapped(object sender, TappedEventArgs e)
    {
        SetLanguage("es");
    }

    private void SetLanguage(string lang)
    {
        var culture = new CultureInfo(lang);
        LocalizationResourceManager.Instance.SetCulture(culture);
        Preferences.Set("AppLanguage", lang);

        // ��������� ��������� �������
        RussianCheck.IsVisible = lang == "ru";
        EnglishCheck.IsVisible = lang == "en";
        LithuanianCheck.IsVisible = lang == "lt";
        SpanishCheck.IsVisible = lang == "es";// ��� ���������� �����
    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        ((AppShell)Shell.Current)?.LoadChatTitlesAsync();
    }
}