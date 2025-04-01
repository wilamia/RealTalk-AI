using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealTalk_AI
{
    public partial class MainPage : ContentPage
    {
        private List<(string sender, string message)> chatHistory = new(); 

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void OnSendMessage(object sender, EventArgs e)
        {
            string userMessage = InputEntry.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;
            
            InputEntry.Text = string.Empty;

            await Navigation.PushAsync(new ChatPage(chatHistory, userMessage));
        }
    }
}