using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Bielik.Core;
using Microsoft.Extensions.Logging;

namespace Bielik.ViewModels;

public sealed class ChatViewModel : ObservableObject
{
    private readonly BielikClient _client;
    private readonly ILogger<ChatViewModel> _logger;
    private readonly List<ChatMessage> _history = [];
    private CancellationTokenSource? _generation;
    private string _draft = "";
    private string _error = "";
    private bool _isBusy;
    private int _sequence;

    public ChatViewModel(BielikClient client, AppState state, ILogger<ChatViewModel> logger)
    {
        _client = client;
        State = state;
        _logger = logger;
        SendCommand = new Command(async () => await SendAsync(), () => CanSend);
        StopCommand = new Command(() => _generation?.Cancel(), () => IsBusy);
        NewConversationCommand = new Command(ClearConversation, () => !IsBusy);
        CopyReplyCommand = new Command(async () =>
        {
            var reply = Messages.LastOrDefault(message => !message.IsUser && message.Text.Length != 0);
            if (reply is not null)
            {
                await Clipboard.Default.SetTextAsync(reply.Text);
            }
        }, () => !IsBusy && Messages.Any(message => !message.IsUser && message.Text.Length != 0));
        State.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppState.SavedAddress))
            {
                ClearConversation();
            }
        };
    }

    public AppState State { get; }
    public ObservableCollection<MessageBubble> Messages { get; } = [];
    public string Draft
    {
        get => _draft;
        set
        {
            if (SetProperty(ref _draft, value ?? ""))
            {
                OnPropertyChanged(nameof(CanSend));
                SendCommand.ChangeCanExecute();
            }
        }
    }
    public bool IsBusy => _isBusy;
    public bool IsNotBusy => !_isBusy;
    public bool CanSend => !_isBusy && !string.IsNullOrWhiteSpace(_draft) && _draft.Length <= 2000;
    public bool IsEmpty => Messages.Count == 0;
    public bool HasMessages => !IsEmpty;
    public string ErrorText => _error;
    public bool HasError => _error.Length != 0;
    public Command SendCommand { get; }
    public Command StopCommand { get; }
    public Command NewConversationCommand { get; }
    public Command CopyReplyCommand { get; }

    public void ChooseIdea(string id)
    {
        var idea = PromptCatalog.All.FirstOrDefault(item => item.Id == id);
        if (idea is null)
        {
            SetError("Nie znaleziono tej inspiracji.");
            return;
        }

        if (!_isBusy)
        {
            Draft = idea.Prompt;
            SetError("");
        }
    }

    private async Task SendAsync()
    {
        if (!CanSend)
        {
            SetError("Napisz wiadomość do 2000 znaków i poczekaj na zakończenie poprzedniej odpowiedzi.");
            return;
        }

        var question = Draft.Trim();
        LocalEndpoint endpoint;
        try
        {
            endpoint = State.Endpoint;
        }
        catch (FormatException exception)
        {
            SetError(exception.Message);
            return;
        }

        Draft = "";
        SetError("");
        SetBusy(true);
        using var cancellation = new CancellationTokenSource();
        _generation = cancellation;
        Messages.Add(new MessageBubble(true, ++_sequence, question));
        var answer = new MessageBubble(false, ++_sequence, "");
        Messages.Add(answer);
        RefreshMessages();
        var text = new StringBuilder();
        try
        {
            await _client.CheckConnectionAsync(endpoint, cancellation.Token);
            var conversation = _history.Append(new ChatMessage("user", question)).ToArray();
            await foreach (var chunk in _client.StreamAsync(endpoint, conversation, cancellation.Token))
            {
                text.Append(chunk.Text);
                answer.Text = text.ToString();
                if (chunk.IsComplete)
                {
                    _history.Add(new ChatMessage("user", question));
                    _history.Add(new ChatMessage("assistant", answer.Text));
                    if (_history.Count > 12)
                    {
                        _history.RemoveRange(0, _history.Count - 12);
                    }

                    answer.Status = chunk.Metrics is { } metrics
                        ? $"BIELIK · {metrics.TokensPerSecond.ToString("F1", CultureInfo.GetCultureInfo("pl-PL"))} TOK/S"
                        : "BIELIK · LOKALNIE";
                }
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            answer.Status = "PRZERWANO · NIE DODANO DO KONTEKSTU";
            if (answer.Text.Length == 0)
            {
                answer.Text = "Generowanie zatrzymane.";
            }
        }
        catch (Exception exception) when (AppState.IsConnectionException(exception))
        {
            _logger.LogWarning(exception, "Local Bielik generation failed");
            answer.Status = "NIEUKOŃCZONA ODPOWIEDŹ";
            if (answer.Text.Length == 0)
            {
                answer.Text = "Nie udało się wygenerować odpowiedzi.";
            }

            SetError(AppState.DescribeError(exception));
            Draft = question;
        }
        finally
        {
            _generation = null;
            SetBusy(false);
        }
    }

    private void ClearConversation()
    {
        if (_isBusy)
        {
            return;
        }

        Messages.Clear();
        _history.Clear();
        _sequence = 0;
        Draft = "";
        SetError("");
        RefreshMessages();
        CopyReplyCommand.ChangeCanExecute();
    }

    private void SetBusy(bool value)
    {
        _isBusy = value;
        State.SetGenerating(value);
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(IsNotBusy));
        OnPropertyChanged(nameof(CanSend));
        SendCommand.ChangeCanExecute();
        StopCommand.ChangeCanExecute();
        NewConversationCommand.ChangeCanExecute();
        CopyReplyCommand.ChangeCanExecute();
    }

    private void SetError(string value)
    {
        _error = value;
        OnPropertyChanged(nameof(ErrorText));
        OnPropertyChanged(nameof(HasError));
    }

    private void RefreshMessages()
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasMessages));
    }
}
