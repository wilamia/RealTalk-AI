using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RealTalk_AI
{
    public partial class MainPage : ContentPage
    {
        private List<(string sender, string message)> chatHistory = new();
        private string chatTitle = "SmartTalk AI";
        public string ChatTitle
        {
            get => chatTitle;
            set
            {
                chatTitle = value;
                OnPropertyChanged();
            }
        }
        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetIconColor(this, Color.FromHex("305FD5"));
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            changetitle();
        }
        private async void changetitle()
        {
            // Ожидаем, пока Shell будет доступен
            await Task.Delay(100); // Небольшая задержка для уверенности, что интерфейс инициализирован

            try
            {
                var appShell = Shell.Current as AppShell;
                if (appShell != null)
                {
                    appShell.SetTitle(chatTitle);
                }
                else
                {
                    Debug.WriteLine("❌ AppShell is null");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ changetitle() failed: {ex.Message}");
            }
        }
        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
 
        }
        private async void OnSendMessage(object sender, EventArgs e)
        {
            string userMessage = InputEntry.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            InputEntry.Text = string.Empty;

            // Сериализация chatHistory в JSON строку
            string serializedChatHistory = JsonConvert.SerializeObject(chatHistory);

            // Навигация с передачей сериализованных данных через параметры
            await Shell.Current.GoToAsync($"//ChatPage?chatHistory={Uri.EscapeDataString(serializedChatHistory)}&userMessage={Uri.EscapeDataString(userMessage)}");
        }
    }
}