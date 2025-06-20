using System.Text;
using Newtonsoft.Json;
using Firebase.Database;
using Firebase.Database.Query;
using System.Diagnostics;

namespace RealTalk_AI
{
    [QueryProperty(nameof(ChatHistory), "chatHistory")]
    [QueryProperty(nameof(UserMessage), "userMessage")]
    [QueryProperty(nameof(ChatId), "chatId")]
    public partial class ChatPage : ContentPage, IQueryAttributable
    {
        private ViewModels.ChatPageViewModel ViewModel => BindingContext as ViewModels.ChatPageViewModel;
        private const string ApiKey = "tgp_v1_yhnsDXnowgpVXFZPXauX11R2zGYOvwCF4EIDOPDP3To";
        private const string ApiUrl = "https://api.together.ai/v1/chat/completions";
        private const string DefaultChatTitle = "Новый чат";

        private CancellationTokenSource cancellationTokenSource;
        private bool isSending;
        private string chatTitle = DefaultChatTitle;
        private string _chatId;
        private string firstMessage;

        private List<(string sender, string message)> chatHistory = new();

        public string ChatTitle
        {
            get => chatTitle;
            set { chatTitle = value; OnPropertyChanged(); }
        }

        public string ChatId
        {
            get => _chatId;
            set
            {

                if (!string.IsNullOrEmpty(value))
                {
                    _chatId = Uri.UnescapeDataString(value);
                    LoadChat(_chatId);
                }
            }
        }

        public string ChatHistory
        {
            get => JsonConvert.SerializeObject(chatHistory);
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    chatHistory = JsonConvert.DeserializeObject<List<(string sender, string message)>>(
                        Uri.UnescapeDataString(value)) ?? new();
                }
                OnPropertyChanged();
            }
        }

        public string UserMessage
        {
            get => firstMessage;
            set { firstMessage = value; OnPropertyChanged(); }
        }

        public ChatPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.ChatPageViewModel();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("chatId", out var chatIdObj) && chatIdObj is string chatIdStr)
            {
                ChatId = Uri.UnescapeDataString(chatIdStr);
                Debug.WriteLine($"[ApplyQueryAttributes] chatId = {ChatId}");
                LoadChat(ChatId);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (string.IsNullOrEmpty(ChatId))
            {
                ClearChatState();
            }
            else if (!chatHistory.Any())
            {
                LoadChat(ChatId);
            }

            if (!string.IsNullOrEmpty(firstMessage))
            {
                cancellationTokenSource = new();
                HandleIncomingMessage(firstMessage, cancellationTokenSource.Token);
            }
            UpdateChatTitle();
        }


        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            if (await TrySaveCurrentChatAsync())
            {
                ClearChatState();

                if (Shell.Current is AppShell appShell)
                {
                    appShell.LoadChatTitlesAsync();
                    appShell.SetTitle("RealTalk AI");
                }
            }
        }
        public async Task<bool> TrySaveCurrentChatAsync()
        {
            if (!chatHistory.Any()) return false;

            string userEmail = AuthService.GetUserEmail();
            string userId = AuthService.GetUserId();
            string nickname = AuthService.GetUsername();
            string topic = GetChatTopic(chatHistory.First().message);

            string effectiveChatId = ChatId;

            if (string.IsNullOrEmpty(effectiveChatId))
            {
                effectiveChatId = Guid.NewGuid().ToString();
                ChatId = effectiveChatId;
            }

            await FirebaseService.SaveChatHistoryAsync(userEmail, nickname, chatHistory, topic, chatId: effectiveChatId);

            Debug.WriteLine($"? Чат сохранён под ID: {effectiveChatId}");

            return true;
        }
        public void ClearChatState()
        {
            chatHistory.Clear();
            ChatLayout.Children.Clear();
            ChatTitle = DefaultChatTitle;
            InputEntry.Text = string.Empty;

            ((AppShell)Shell.Current)?.SetTitle(ChatTitle);
        }

        private void UpdateChatTitle()
        {
            if (chatHistory.Count > 0)
            {
                var topic = GetChatTopic(chatHistory.First().message);
                ChatTitle = $"{ViewModel.ChatTopic} {topic}";
                ((AppShell)Shell.Current)?.SetTitle(ChatTitle);
                ((AppShell)Shell.Current)?.StartTitleMarquee();
            }
        }

        private string GetChatTopic(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Без темы";

            var words = message.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            string topic = words.Length > 3 ? string.Join(" ", words.Take(3)) : message;

            // Ограничение по символам (например, 30)
            int maxLength = 30;
            if (topic.Length > maxLength)
                topic = topic.Substring(0, maxLength) + "...";

            return topic;
        }


        private async void OnSendOrStopMessage(object sender, EventArgs e)
        {
            if (isSending)
                StopSending();
            else
                await SendMessage();
        }

        private async void OnSendMessage(object sender, EventArgs e)
        {
            await SendMessage();
        }
        private async Task SendMessage()
        {
            string userMessage = InputEntry.Text;

            await Task.Delay(50);

            if (string.IsNullOrWhiteSpace(userMessage)) return;

            InputEntry.Text = string.Empty;

            cancellationTokenSource = new();
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
            InputEntry.Unfocus();
            if (chatHistory.Exists(m => m.message == message)) return;

            isSending = true;
            UpdateSendButton();

            AddMessageToChat(ViewModel.You, message, Color.FromRgb(48, 95, 213), Colors.White);
            chatHistory.Add((ViewModel.You, message));
            await ScrollToBottom();

            var aiFrame = AddMessageToChat(ViewModel.Lama, "", Colors.Black, Color.FromRgba(0, 0, 0, 20));
            var aiLabel = ((StackLayout)aiFrame.Content).Children[1] as Label;

            if (aiLabel != null)
            {
                await AnimateDots(aiLabel, token);
                string response = await GetTogetherAIResponse(token);
                await AnimateResponse(aiLabel, response, token);
                await ScrollToBottom();

                chatHistory.Add((ViewModel.Lama, response));
            }

            isSending = false;
            UpdateSendButton();
        }

        private async Task AnimateDots(Label label, CancellationToken token)
        {
            string[] variants = { ViewModel.LamaThinking1, ViewModel.LamaThinking2, ViewModel.LamaThinking3};
            int i = 0;
            int max = 30;

            while (!token.IsCancellationRequested && i < max)
            {
                label.Text = variants[i % 3];
                await Task.Delay(500);
                i++;
            }
            if (i >= max) label.Text = ViewModel.LamaThinkingALot;
        }

        private async Task<string> GetTogetherAIResponse(CancellationToken token)
        {
            try
            {
                using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(15) };
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);

                var messages = chatHistory.Select(m => new
                {
                    role = m.sender == ViewModel.You ? "user" : "assistant",
                    content = m.message
                });

                var request = new
                {
                    model = "meta-llama/Llama-3.3-70B-Instruct-Turbo-Free",
                    messages,
                    temperature = 0.7,
                    max_tokens = 6000
                };

                var response = await client.PostAsync(ApiUrl,
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"), token);

                var result = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(result);

                return json?.choices?[0]?.message?.content?.ToString() ?? "Ошибка: пустой ответ от AI.";
            }
            catch (TaskCanceledException)
            {
                return "Ответ AI занял слишком много времени.";
            }
            catch
            {
                return "Ошибка сети. Попробуйте еще раз.";
            }
        }

        private async Task AnimateResponse(Label label, string message, CancellationToken token)
        {
            label.Text = "";

            double previousHeight = label.Height;

            foreach (char c in message)
            {
                if (token.IsCancellationRequested) return;

                label.Text += c;
                await Task.Delay(20);

                await Task.Yield();

                if (Math.Abs(label.Height - previousHeight) > 1)
                {
                    previousHeight = label.Height;
                    await ScrollToBottom();
                }
            }

            await ScrollToBottom();
        }

        public async void LoadChat(string chatId)
        {
            if (string.IsNullOrEmpty(chatId)) return;

            try
            {
                var userEmail = AuthService.GetUserEmail().Replace("@", "_").Replace(".", "_");
                var chatRef = new FirebaseClient("https://realtalk-ai-default-rtdb.firebaseio.com/")
                    .Child("users").Child(userEmail).Child("chats").Child(chatId);

                var chatData = await chatRef.OnceSingleAsync<dynamic>();

                if (chatData?.Messages == null) return;

                chatHistory.Clear();
                ChatLayout.Children.Clear();

                foreach (var msg in chatData.Messages)
                {
                    string sender = msg.sender;
                    string message = msg.message;
                    chatHistory.Add((sender, message));

                    var isUser = sender == ViewModel.You;
                    AddMessageToChat(sender, message,
                        isUser ? Color.FromRgb(48, 95, 213) : Colors.Black,
                        isUser ? Colors.White : Color.FromRgba(0, 0, 0, 20));
                }

                ChatTitle = $"{ViewModel.ChatTopic} {chatData.Topic ?? "Без темы"}";
                ((AppShell)Shell.Current)?.SetTitle(ChatTitle);
                ((AppShell)Shell.Current)?.StartTitleMarquee();

                await ScrollToBottom();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке чата: {ex.Message}");
            }
        }

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
                Text = string.IsNullOrEmpty(message) ? "Lama думает..." : message,
                FontFamily = "Nunito-Light",
                TextColor = textColor,
                FontSize = 16
            };

            var frame = new Frame
            {
                BackgroundColor = backgroundColor,
                BorderColor = Color.FromRgba(0, 0, 0, 20),
                CornerRadius = 15,
                Padding = 10,
                Margin = new Thickness(10, 2),
                Content = new StackLayout { Spacing = 2, Children = { senderLabel, messageLabel } },
                HorizontalOptions = sender == ViewModel.You ? LayoutOptions.End : LayoutOptions.Start,
                HasShadow = true
            };

            ChatLayout.Children.Add(frame);
            return frame;
        }

        private async Task ScrollToBottom()
        {
            await Task.Delay(100);
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                await ChatScrollView.ScrollToAsync(ChatLayout, ScrollToPosition.End, true);
            });
        }
    }
}
