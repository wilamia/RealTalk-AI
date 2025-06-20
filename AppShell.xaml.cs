using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using RealTalk_AI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace RealTalk_AI
{
    public partial class AppShell : Shell
    {
        private ViewModels.AppShellViewModel ViewModel => BindingContext as ViewModels.AppShellViewModel;
        private CancellationTokenSource titleScrollCts;
        private ObservableCollection<string> _chatTitles;
        private static readonly FirebaseClient _databaseClient = new FirebaseClient("https://realtalk-ai-default-rtdb.firebaseio.com/");

        // Инициализируем словарь сразу, чтобы избежать NullReference
        private Dictionary<string, string> chatTitlesDict = new Dictionary<string, string>();

        private bool isNavigating;

        public ObservableCollection<string> ChatTitles
        {
            get => _chatTitles;
            set
            {
                _chatTitles = value;
                OnPropertyChanged(nameof(ChatTitles));
            }
        }

        public AppShell()
        {
            InitializeComponent();

            BindingContext = new ViewModels.AppShellViewModel();
            ChatTitles = new ObservableCollection<string>();

            TitleMain.Text = "SmartTalk AI";
            PageTitle = ViewModel.Home;

            Routing.RegisterRoute("ChatPage", typeof(ChatPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(Settings), typeof(Settings));

            this.Appearing += async (s, e) => await LoadChatTitlesAsync();
        }

        private string _pageTitle;
        public string PageTitle
        {
            get => _pageTitle;
            set
            {
                _pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        public async Task LoadChatTitlesAsync()
        {
            try
            {
                var userEmail = AuthService.GetUserEmail()?.Replace("@", "_").Replace(".", "_");
                if (string.IsNullOrEmpty(userEmail))
                {
                    Debug.WriteLine("User email is null or empty.");
                    await MainThread.InvokeOnMainThreadAsync(() => ChatTitlesCollectionView.ItemsSource = null);
                    return;
                }

                var userChatsRef = _databaseClient.Child("users").Child(userEmail).Child("chats");

                var chats = await userChatsRef.OnceAsync<dynamic>();

                if (chats == null || !chats.Any())
                {
                    Debug.WriteLine("No chats found.");
                    chatTitlesDict.Clear();
                    await MainThread.InvokeOnMainThreadAsync(() => ChatTitlesCollectionView.ItemsSource = null);
                    return;
                }

                chatTitlesDict.Clear();

                foreach (var chat in chats)
                {
                    dynamic chatData = chat.Object;

                    if (chatData != null && chatData.Topic != null)
                    {
                        chatTitlesDict[chat.Key] = $"{ViewModel.ChatTopic} " + (string)chatData.Topic;
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() => ChatTitlesCollectionView.ItemsSource = chatTitlesDict.Values.ToList());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading chat titles: {ex.Message}");
            }
        }


        private async void OnChatSelected(object sender, EventArgs e)
        {
            if (isNavigating) return;

            try
            {
                if (sender is Label label)
                {
                    string selectedTopic = null;
                    string chatId = null;

                    switch (label.BindingContext)
                    {
                        case string topic:
                            selectedTopic = topic;
                            chatId = chatTitlesDict.FirstOrDefault(kvp => kvp.Value == topic).Key;
                            break;

                        case ChatSearchResult result:
                            selectedTopic = result.Title;
                            chatId = chatTitlesDict.FirstOrDefault(kvp => kvp.Value.Contains(result.Title)).Key;
                            break;
                    }

                    if (!string.IsNullOrEmpty(chatId))
                    {
                        isNavigating = true;

                        if (Shell.Current.CurrentPage is ChatPage currentChatPage)
                        {
                            await currentChatPage.TrySaveCurrentChatAsync();
                        }

                        await Shell.Current.GoToAsync($"//ChatPage?chatId={Uri.EscapeDataString(chatId)}");
                        Shell.Current.FlyoutIsPresented = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
            }
            finally
            {
                isNavigating = false;
                await LoadChatTitlesAsync();
            }
        }

        public void StopTitleMarquee()
        {
            titleScrollCts?.Cancel();
            titleScrollCts = null;
        }
        public void StartTitleMarquee()
        {
            titleScrollCts?.Cancel();
            titleScrollCts = new CancellationTokenSource();
            var token = titleScrollCts.Token;

            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                if (TitleScrollView == null || TitleMain == null || token.IsCancellationRequested)
                    return false;

                double labelWidth = TitleMain.Width;
                double scrollWidth = TitleScrollView.Width;

                if (labelWidth <= scrollWidth)
                    return false;

                double currentOffset = TitleScrollView.ScrollX;
                double newOffset = currentOffset + 1;

                if (newOffset + scrollWidth >= labelWidth)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await TitleScrollView.ScrollToAsync(0, 0, false);
                    });
                }
                else
                {
                    TitleScrollView.ScrollToAsync(newOffset, 0, false);
                }

                return true;
            });
        }
     
        public void SetTitle(string title)
        {
            PageTitle = title;
            if (TitleMain != null)
            {
                TitleMain.Text = PageTitle;
            }
        }

        private async void Logout(object sender, EventArgs e)
        {
            var currentPage = Shell.Current?.CurrentPage as ChatPage;

            AuthService.Logout();

            Microsoft.Maui.Controls.Application.Current.MainPage = new NavigationPage(new SignInPage());

            if (currentPage != null)
            {
                await currentPage.TrySaveCurrentChatAsync();
            }
        }


        private async void OnTapped(object sender, EventArgs e)
        {
            if (Shell.Current.CurrentPage is ChatPage chatPage)
            {
                await chatPage.TrySaveCurrentChatAsync();
                chatPage.ClearChatState();
                chatPage.ChatId = null;
            }

            await Shell.Current.GoToAsync("//MainPage");
            Shell.Current.FlyoutIsPresented = false;

            await Task.Delay(100);

            SetTitle("RealTalk AI");
            await LoadChatTitlesAsync();
        }

        private async void OnTappedSettings(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(Settings));
            Shell.Current.FlyoutIsPresented = false;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string query = InputEntry?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadChatTitlesAsync();
                return;
            }

            var userEmail = AuthService.GetUserEmail()?.Replace("@", "_").Replace(".", "_");
            if (string.IsNullOrEmpty(userEmail)) return;

            var userChatsRef = _databaseClient.Child("users").Child(userEmail).Child("chats");

            try
            {
                var chats = await userChatsRef.OnceAsync<dynamic>();
                var matchedResults = new List<ChatSearchResult>();
                string loweredQuery = query.ToLowerInvariant();

                foreach (var chat in chats)
                {
                    string chatId = chat.Key;
                    dynamic chatData = chat.Object;

                    if (chatData?.Messages == null)
                        continue;

                    foreach (var message in chatData.Messages)
                    {
                        string messageText = message?.message;
                        if (!string.IsNullOrEmpty(messageText) && messageText.ToLowerInvariant().Contains(loweredQuery))
                        {
                            string topic = chatData?.Topic ?? "Без темы";

                            string snippet = System.Net.WebUtility.HtmlEncode(messageText);
                            snippet = System.Text.RegularExpressions.Regex.Replace(snippet,
                                $"({System.Text.RegularExpressions.Regex.Escape(query)})",
                                "<b>$1</b>",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            matchedResults.Add(new ChatSearchResult
                            {
                                Title = topic,
                                Snippet = snippet
                            });

                            break;
                        }
                    }
                }

                if (matchedResults.Any())
                {
                    ChatTitlesCollectionView.ItemsSource = matchedResults;
                }
                else
                {
                    ChatTitlesCollectionView.ItemsSource = new List<ChatSearchResult>
                    {
                        new ChatSearchResult { Title = "Чаты не найдены", Snippet = "" }
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Search Error] {ex.Message}");
            }
        }

        private async void OnSearchCompleted(object sender, EventArgs e)
        {
            string query = InputEntry?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadChatTitlesAsync();
                return;
            }

            var userEmail = AuthService.GetUserEmail()?.Replace("@", "_").Replace(".", "_");
            if (string.IsNullOrEmpty(userEmail)) return;

            try
            {
                ChatTitles.Clear();

                var userChatsRef = _databaseClient.Child("users").Child(userEmail).Child("chats");

                var chats = await userChatsRef.OnceAsync<dynamic>();

                var matchedTitles = new Dictionary<string, string>();
                string loweredQuery = query.ToLowerInvariant();

                foreach (var chat in chats)
                {
                    string chatId = chat.Key;
                    dynamic chatData = chat.Object;

                    if (chatData?.Messages == null)
                        continue;

                    foreach (var message in chatData.Messages)
                    {
                        string messageText = message?.message;
                        if (!string.IsNullOrEmpty(messageText) &&
                            messageText.ToLowerInvariant().Contains(loweredQuery))
                        {
                            string topic = chatData?.Topic ?? "Без темы";
                            matchedTitles[chatId] = $"{ViewModel.ChatTopic} {topic}";
                            break;
                        }
                    }
                }

                if (matchedTitles.Any())
                {
                    chatTitlesDict = matchedTitles;
                    ChatTitlesCollectionView.ItemsSource = matchedTitles.Values.ToList();
                }
                else
                {
                    ChatTitlesCollectionView.ItemsSource = new List<string> { "Чаты не найдены" };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Search Error] {ex.Message}");
            }
        }

    }

        public class ChatSearchResult
    {
        public string Title { get; set; }
        public string Snippet { get; set; }
    }
}
