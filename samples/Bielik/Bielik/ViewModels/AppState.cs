using System.Text.Json;
using Bielik.Core;
using Microsoft.Extensions.Logging;

namespace Bielik.ViewModels;

public sealed class AppState : ObservableObject
{
#if DEBUG
    private const bool AllowInsecureHttp = true;
    private const string InitialAddress = ModelInfo.DefaultEndpoint;
#else
    private const bool AllowInsecureHttp = false;
    private const string InitialAddress = "https://127.0.0.1:11434";
#endif
    private readonly BielikClient _client;
    private readonly IPreferences _preferences;
    private readonly ILogger<AppState> _logger;
    private string _addressDraft;
    private string _savedAddress;
    private string _error = "";
    private string _digest = "";
    private bool _isChecking;
    private bool _isReady;
    private bool _isGenerating;

    public AppState(BielikClient client, IPreferences preferences, ILogger<AppState> logger)
    {
        _client = client;
        _preferences = preferences;
        _logger = logger;
        _savedAddress = preferences.Get("bielik.local-endpoint", InitialAddress);
        _addressDraft = _savedAddress;
        ConnectCommand = new Command(async () => await CheckConnectionAsync(), () => CanConfigure);
        SaveCommand = new Command(async () => await SaveAsync(), () => CanConfigure);
        CopyInstallCommand = new Command(async () => await Clipboard.Default.SetTextAsync(ModelInfo.PullCommand));
    }

    public string AddressDraft
    {
        get => _addressDraft;
        set => SetProperty(ref _addressDraft, value ?? "");
    }

    public string SavedAddress => _savedAddress;
    public LocalEndpoint Endpoint => LocalEndpoint.Parse(_savedAddress, AllowInsecureHttp);
    public IReadOnlyList<PromptIdea> Ideas => PromptCatalog.All;
    public string ModelName => $"{ModelInfo.Name} · {ModelInfo.Version}";
    public string InstallCommand => ModelInfo.PullCommand;
    public string Digest => _digest.Length > 12 ? _digest[..12] : _digest;
    public bool IsReady => _isReady;
    public bool IsChecking => _isChecking;
    public bool CanConfigure => !_isChecking && !_isGenerating;
    public string StatusText => _isChecking ? "Sprawdzam połączenie" : _isReady ? "Lokalny Bielik gotowy" : "Połącz lokalnego Bielika";
    public string ShortStatus => _isChecking ? "SPRAWDZAM" : _isReady ? "LOKALNIE · GOTOWY" : "LOKALNIE · OFFLINE";
    public Color StatusColor => Color.FromArgb(_isReady ? "#4B7155" : "#69243F");
    public string ConnectionError => _error;
    public bool HasConnectionError => _error.Length != 0;
    public Command ConnectCommand { get; }
    public Command SaveCommand { get; }
    public Command CopyInstallCommand { get; }

    public void SetGenerating(bool value)
    {
        _isGenerating = value;
        RefreshState();
    }

    public async Task CheckConnectionAsync()
    {
        if (!CanConfigure)
        {
            return;
        }

        _isChecking = true;
        SetError("");
        RefreshState();
        try
        {
            _digest = await _client.CheckConnectionAsync(Endpoint);
            _isReady = true;
            OnPropertyChanged(nameof(Digest));
        }
        catch (Exception exception) when (IsConnectionException(exception))
        {
            _isReady = false;
            _logger.LogWarning(exception, "Local Bielik connection check failed");
            SetError(DescribeError(exception));
        }
        finally
        {
            _isChecking = false;
            RefreshState();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            var endpoint = LocalEndpoint.Parse(AddressDraft, AllowInsecureHttp);
            var address = endpoint.Address.AbsoluteUri.TrimEnd('/');
            AddressDraft = address;
            if (_savedAddress != address)
            {
                _savedAddress = address;
                _preferences.Set("bielik.local-endpoint", _savedAddress);
                OnPropertyChanged(nameof(SavedAddress));
            }
        }
        catch (FormatException exception)
        {
            SetError(exception.Message);
            return;
        }

        await CheckConnectionAsync();
    }

    internal static bool IsConnectionException(Exception exception) =>
        exception is HttpRequestException or OperationCanceledException or FormatException
            or InvalidOperationException or IOException or JsonException;

    internal static string DescribeError(Exception exception) => exception switch
    {
        HttpRequestException => "Nie można połączyć się z lokalnym serwerem. Uruchom Ollama na Macu i sprawdź adres.",
        OperationCanceledException => "Serwer nie odpowiedział na czas. Sprawdź, czy Ollama jest uruchomiona.",
        JsonException => "Serwer zwrócił nieprawidłowe dane. Ten adres musi wskazywać lokalną Ollamę.",
        _ => exception.Message
    };

    private void SetError(string value)
    {
        _error = value;
        OnPropertyChanged(nameof(ConnectionError));
        OnPropertyChanged(nameof(HasConnectionError));
    }

    private void RefreshState()
    {
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(IsChecking));
        OnPropertyChanged(nameof(CanConfigure));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(ShortStatus));
        OnPropertyChanged(nameof(StatusColor));
        ConnectCommand.ChangeCanExecute();
        SaveCommand.ChangeCanExecute();
    }
}
