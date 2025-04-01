using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace RealTalk_AI
{
    public partial class ChatPage : ContentPage
    {
        private const string apiKey = "tgp_v1_yhnsDXnowgpVXFZPXauX11R2zGYOvwCF4EIDOPDP3To";
        private const string apiUrl = "https://api.together.ai/v1/chat/completions";

        private List<(string sender, string message)> chatHistory;
        private string firstMessage;
        private bool isTyping;
        private bool isStopRequested = false;

        public ChatPage(List<(string sender, string message)> messages, string firstMessage = null)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            chatHistory = messages ?? new List<(string sender, string message)>();
            this.firstMessage = firstMessage;

            foreach (var msg in chatHistory)
            {
                AddMessageToChat(
                    msg.sender,
                    msg.message,
                    msg.sender == "��" ? Colors.Blue : Colors.Green,
                    msg.sender == "��" ? Colors.LightBlue : Colors.LightGreen
                );
            }
        }

        private async Task HandleIncomingMessage(string message)
        {
            // �������� �� ��� ����������� ���������, ����� �������� ������������
            if (chatHistory.Exists(m => m.message == message)) return;

            // �������� ��������� � ������� ����
            AddMessageToChat("��", message, Color.FromRgb(48, 95, 213), Colors.White);
            chatHistory.Add(("��", message)); // ���������� � ������� ����
            await ScrollToBottom();

            // �������� �������� ��� ������ AI
            var aiMessageFrame = AddMessageToChat("Lama", "", Colors.Black, Color.FromRgba(0, 0, 0, 20));
            var aiLabel = ((StackLayout)aiMessageFrame.Content).Children[1] as Label;

            if (aiLabel != null)
            {
                // �������� ����� �� AI
                string response = await GetTogetherAIResponse();
                await AnimateResponse(aiLabel, response); // �������� ������
                chatHistory.Add(("Lama", response)); // ���������� ������ AI � ������� ����
                await ScrollToBottom();
            }
        }

        private async Task AnimateResponse(Label label, string message)
        {
            label.Text = "";  // ������� "Lama ��������..."

            foreach (char c in message)
            {
                label.Text += c;  // ��������� �� ����� �����
                await Task.Delay(50);
            }
        }



        private Frame AddMessageToChat(string sender, string message, Color textColor, Color backgroundColor)
        {
            // ������� ������ ��������� (��� �����������)
            var senderLabel = new Label
            {
                Text = sender,
                FontAttributes = FontAttributes.Bold,
                TextColor = textColor,
                FontSize = 14
            };

            // ������� ����� ���������
            var messageLabel = new Label
            {
                Text = string.IsNullOrEmpty(message) ? "Lama ..." : message,
                TextColor = textColor,
                FontSize = 16
            };

            // StackLayout ��� ����������� ��������� � ���������
            var stackLayout = new StackLayout
            {
                Spacing = 2,
                Children = { senderLabel, messageLabel }
            };

            var frame = new Frame
            {
                BackgroundColor = backgroundColor,
                BorderColor = Color.FromRgba(0, 0, 0, 20),
                CornerRadius = 15,
                Padding = 10,
                Margin = new Thickness(10, 2, 10, 2),
                Content = stackLayout,
                HorizontalOptions = sender == "��" ? LayoutOptions.End : LayoutOptions.Start,
                HasShadow = true
            };

            ChatLayout.Children.Add(frame);

            // ���� ��� �������� ��� "Lama", ��������� ShowTypingAnimationInFrame
            if (sender == "Lama" && string.IsNullOrEmpty(message))
            {
                ShowTypingAnimationInFrame(messageLabel);
            }

            return frame;
        }
        private async void ShowTypingAnimationInFrame(Label label)
        {
            string baseText = "Lama ��������";
            string dots = "";
            int dotCount = 0;
            bool movingRight = true;

            while (label.Text == "Lama ..." || label.Text.StartsWith("Lama ��������"))
            {
                label.Text = $"{baseText}{dots}";

                if (movingRight)
                {
                    dots += ".";
                    dotCount++;
                    if (dotCount == 3) movingRight = false;
                }
                else
                {
                    dots = dots.Substring(0, dots.Length - 1);
                    dotCount--;
                    if (dotCount == 0) movingRight = true;
                }

                await Task.Delay(300);
            }
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ���� �������� ��������� ��� �������������, ���������� ���
            if (!string.IsNullOrEmpty(firstMessage) && !chatHistory.Exists(m => m.message == firstMessage))
            {
                string tempMessage = firstMessage;
                firstMessage = null;  // ������� ������ ��������� ����� ���������
                await HandleIncomingMessage(tempMessage);
            }
        }

        private async Task ScrollToBottom()
        {
            await Task.Delay(100);
            ChatScrollView.ScrollToAsync(ChatLayout, ScrollToPosition.End, true);
        }

        private async void OnSendMessage(object sender, EventArgs e)
        {
            SendButton.Source = "stop_icon.svg";
            isStopRequested = false;

            string userMessage = InputEntry.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            InputEntry.Text = string.Empty;

            // �������� ��������� ������������ � ���������� ��� � �������
            await HandleIncomingMessage(userMessage);
        }

        private void OnStopMessage(object sender, EventArgs e)
        {
            SendButton.Source = "send_icon.svg";
            isStopRequested = true;
        }

        private async Task<string> GetTogetherAIResponse()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

                    var messages = new List<object>();
                    foreach (var (sender, message) in chatHistory)
                    {
                        messages.Add(new { role = sender == "��" ? "user" : "assistant", content = message });
                    }

                    var requestBody = new
                    {
                        model = "meta-llama/Llama-3.3-70B-Instruct-Turbo-Free",
                        messages = messages,
                        temperature = 0.7,
                        max_tokens = 6000
                    };

                    string json = JsonConvert.SerializeObject(requestBody);
                    Console.WriteLine("�������� ������� � API: " + json);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("����� API: " + result);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"������ API: {response.StatusCode}");
                        return $"������ API: {response.StatusCode}\n{result}";
                    }

                    dynamic jsonResponse = JsonConvert.DeserializeObject(result);
                    if (jsonResponse?.choices == null || jsonResponse.choices.Count == 0)
                    {
                        Console.WriteLine("������: ������ ����� �� AI.");
                        return "������: ������ ����� �� AI.";
                    }

                    string aiResponse = jsonResponse.choices[0].message.content.ToString();
                    Console.WriteLine("����� AI: " + aiResponse);
                    return aiResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"������ ��� ������� � API: {ex.Message}");
                return "������ ����. ���������� ��� ���.";
            }
        }
    }
}
