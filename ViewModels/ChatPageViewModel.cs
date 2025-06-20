using RealTalk_AI.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealTalk_AI.ViewModels;

public class ChatPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public ChatPageViewModel()
    {
        LocalizationResourceManager.Instance.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(EnterQuestion));
            OnPropertyChanged(nameof(You));
            OnPropertyChanged(nameof(ChatTopic));
            OnPropertyChanged(nameof(Lama));
            OnPropertyChanged(nameof(LamaThinking1));
            OnPropertyChanged(nameof(LamaThinking2));
            OnPropertyChanged(nameof(LamaThinking3));
            OnPropertyChanged(nameof(LamaThinkingALot));
        };
    }

    public string EnterQuestion => LocalizationResourceManager.Instance["EnterQuestion"];
    public string You => LocalizationResourceManager.Instance["You"];
    public string ChatTopic => LocalizationResourceManager.Instance["ChatTopic"];
    public string Lama => LocalizationResourceManager.Instance["Lama"];
    public string LamaThinking1 => LocalizationResourceManager.Instance["Thinking1"];
    public string LamaThinking2 => LocalizationResourceManager.Instance["Thinking2"];
    public string LamaThinking3 => LocalizationResourceManager.Instance["Thinking3"];
    public string LamaThinkingALot => LocalizationResourceManager.Instance["ThinkingALot"];
    public string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}