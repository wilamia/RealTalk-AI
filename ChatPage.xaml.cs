using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Firebase.Auth;

namespace RealTalk_AI
{
    [QueryProperty(nameof(ChatHistory), "chatHistory")]
    [QueryProperty(nameof(UserMessage), "userMessage")]
    public partial class ChatPage : ContentPage
    {
        private string chatTitle = "����� ���";
        private const string apiKey = "tgp_v1_yhnsDXnowgpVXFZPXauX11R2zGYOvwCF4EIDOPDP3To";
        private const string apiUrl = "https://api.together.ai/v1/chat/completions";
        private CancellationTokenSource cancellationTokenSource;
        private bool isSending;

        private List<(string sender, string message)> chatHistory = new();
        private string firstMessage;
        public string ChatTitle
        {
            get => chatTitle;
            set
            {
                chatTitle = value;
                OnPropertyChanged();
            }
        }

        public string ChatHistory
        {
            get => JsonConvert.SerializeObject(chatHistory); // ������������ � ������
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // �������������� �� ������
                    chatHistory = JsonConvert.DeserializeObject<List<(string sender, string message)>>(
                        Uri.UnescapeDataString(value)) ?? new List<(string sender, string message)>(); // ��������� ������ �� null
                }
                OnPropertyChanged();
            }
        }
        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            // Check if chat history exists
            if (chatHistory.Any())
            {
                // Retrieve the user's information from AuthService
                string userEmail = AuthService.GetUserEmail();
                string userId = AuthService.GetUserId();
                string nickname = AuthService.GetNickname();

                // Ensure the user information is available
                if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(nickname))
                {
                    await DisplayAlert("������", "�� ������� �������� ������ ������������.", "OK");
                    return;
                }

                // Save chat history to Firebase
                await FirebaseService.SaveChatHistoryAsync(userId, userEmail, nickname, chatHistory);
            }
        }


        public string UserMessage
        {
            get => firstMessage;
            set
            {
                firstMessage = value;
                OnPropertyChanged();
            }
        }
        public ChatPage()
        {
            InitializeComponent();
            BindingContext = this;
        }
        private void UpdateChatTitle()
        {
            if (chatHistory.Count > 0)
            {
                // ��������� ������� ��������� �� ������ ������ ���������� ���� ���������
                var initialMessage = chatHistory.First().message;
                chatTitle = $"��� �� ����: {GetChatTopic(initialMessage)}";
                var appShell = (AppShell)Shell.Current;
                appShell.SetTitle(chatTitle);
            }
        }
        private string GetChatTopic(string message)
        {
            // ���������� ������ ��� ��������� �������� ���� (����� ��������)
            var words = message.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 3)
            {
                return string.Join(" ", words.Take(3)); // ����� ������ ��� �����
            }
            return message; 
        }
        // �������� ��������� ��� �������� ��������
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // ���� ��������� ���� ��������, ��������� ������ ���������
            if (!string.IsNullOrEmpty(firstMessage))
            {
                cancellationTokenSource = new CancellationTokenSource();
                HandleIncomingMessage(firstMessage, cancellationTokenSource.Token);
            }
            UpdateChatTitle();
        }


        private async void OnSendOrStopMessage(object sender, EventArgs e)
        {
            if (isSending)
            {
                StopSending();
            }
            else
            {
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            string userMessage = InputEntry.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            InputEntry.Text = string.Empty;

            cancellationTokenSource = new CancellationTokenSource();
            await HandleIncomingMessage(userMessage, cancellationTokenSource.Token);
        }

        private void StopSending()
        {
            cancellationTokenSource?.Cancel();
            isSending = false;
            UpdateSendButton();
        }

        private void UpdateSendButton()
        {
            SendButton.Source = isSending ? "stop_icon.svg" : "send_icon.svg";
        }

        private async Task HandleIncomingMessage(string message, CancellationToken token)
        {
            isSending = true;
            UpdateSendButton();
            if (chatHistory.Exists(m => m.message == message)) return;

            AddMessageToChat("��", message, Color.FromRgb(48, 95, 213), Colors.White);
            chatHistory.Add(("��", message));
            await ScrollToBottom();

            var aiMessageFrame = AddMessageToChat("Lama", "", Colors.Black, Color.FromRgba(0, 0, 0, 20));
            var aiLabel = ((StackLayout)aiMessageFrame.Content).Children[1] as Label;

            if (aiLabel != null)
            {
                // �������� �������� ���� �����
                await AnimateDots(aiLabel, token);

                // �������� ����� �� ��
                string response = await GetTogetherAIResponse(token);

                // ����� ���� ��� ����� �������, ��������� ���
                await AnimateResponse(aiLabel, response, token);

                chatHistory.Add(("Lama", response));
                await ScrollToBottom();
            }

            isSending = false;
            UpdateSendButton();
        }
        private async Task AnimateDots(Label label, CancellationToken token)
        {
            string[] dotVariants = { "Lama ������.", "Lama ������..", "Lama ������..." };
            int i = 0;

       
            int maxIterations = 30; 

            while (!token.IsCancellationRequested && i < maxIterations)
            {
                label.Text = dotVariants[i % 3];
                i++;

                await Task.Delay(500);
            }
            if (i >= maxIterations)
            {
                label.Text = "Lama ����������..."; 
            }
        }
        private async Task<string> GetTogetherAIResponse(CancellationToken token)
        {
            try
            {
                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) })
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

                    var messages = chatHistory.Select(m => new { role = m.sender == "��" ? "user" : "assistant", content = m.message }).ToList();
                    var requestBody = new { model = "meta-llama/Llama-3.3-70B-Instruct-Turbo-Free", messages, temperature = 0.7, max_tokens = 6000 };
                    string json = JsonConvert.SerializeObject(requestBody);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content, token);

                    string result = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(result);

                    if (jsonResponse == null || jsonResponse.choices == null || jsonResponse.choices.Count == 0)
                    {
                        return "������: ������ ����� �� AI.";
                    }

                    return jsonResponse.choices[0].message.content.ToString();
                }
            }
            catch (TaskCanceledException) 
            {
                return "����� AI ����� ������� ����� �������.";
            }
            catch (Exception ex)
            {
                return "������ ����. ���������� ��� ���.";
            }
        }

        private async Task AnimateResponse(Label label, string message, CancellationToken token)
        {
            label.Text = "";
            foreach (char c in message)
            {
                if (token.IsCancellationRequested) return;
                label.Text += c;
                await Task.Delay(50);
            }
        }

        // ������� ��� ���������� ��������� � ���
        private Frame AddMessageToChat(string sender, string message, Color textColor, Color backgroundColor)
        {
            var senderLabel = new Label
            {
                Text = sender,
                FontFamily = "Nunito-ExtraBold",
                TextColor = textColor,
                FontSize = 14
            };

            var messageLabel = new Label
            {
                Text = string.IsNullOrEmpty(message) ? "Lama ������..." : message,
                FontFamily = "Nunito-Light",
                TextColor = textColor,
                FontSize = 16
            };

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

            return frame;
        }

        // ������� ��������� �� ������ ����� ����
        private async Task ScrollToBottom()
        {
            await Task.Delay(100);
            ChatScrollView.ScrollToAsync(ChatLayout, ScrollToPosition.End, true);
        }
    }
}
