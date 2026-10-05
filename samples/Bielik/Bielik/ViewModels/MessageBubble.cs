namespace Bielik.ViewModels;

public sealed class MessageBubble(bool isUser, int sequence, string text) : ObservableObject
{
    private string _text = text;
    private string _status = isUser ? "TY" : "BIELIK · LOKALNIE";

    public bool IsUser { get; } = isUser;
    public string AutomationId { get; } = $"message-{(isUser ? "user" : "assistant")}-{sequence}";
    public string TextAutomationId { get; } = $"message-text-{sequence}";
    public string StatusAutomationId { get; } = $"message-status-{sequence}";
    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value))
            {
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }
    public string DisplayText => _text.Length == 0 ? "Przygotowuję odpowiedź…" : _text;
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}
