using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using RealTalk_AI.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RealTalk_AI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Debug.WriteLine($"[DBG] InputEntry = {(InputEntry != null ? "OK" : "NULL")}");
            BindingContext = new MainPageViewModel();
            NavigationPage.SetIconColor(this, Color.FromHex("305FD5"));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await SetShellTitleAsync("SmartTalk AI");
        }

        private async Task SetShellTitleAsync(string title)
        {
            await Task.Delay(100);

            if (Shell.Current is AppShell appShell)
            {
                appShell.SetTitle(title);
                appShell.StopTitleMarquee();
            }
            else
            {
                Shell.Current.Title = "RealTalk AI";
            }
        }

        private async void OnSendMessage(object sender, EventArgs e)
        {
            string userMessage = InputEntry.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            InputEntry.Text = string.Empty;

            var emptyChat = new List<(string sender, string message)>();
            string serializedChatHistory = JsonConvert.SerializeObject(emptyChat);
            string newChatId = Guid.NewGuid().ToString();

           
            InputEntry.Unfocus();

            await Shell.Current.GoToAsync($"//ChatPage?chatId={Uri.EscapeDataString(newChatId)}&chatHistory={Uri.EscapeDataString(serializedChatHistory)}&userMessage={Uri.EscapeDataString(userMessage)}");
        }
    }
}