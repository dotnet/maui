---
name: maui-speech-to-text
description: >
  Add speech-to-text voice input to .NET MAUI apps using CommunityToolkit.Maui.
  Covers speech recognition, microphone permissions, and hands-free text entry.
  Works with any UI pattern (XAML/MVVM, C# Markup, MauiReactor).
  USE FOR: "speech to text", "voice input", "speech recognition", "microphone input",
  "voice command", "dictation", "SpeechToText", "hands-free text", "transcribe speech".
  DO NOT USE FOR: text-to-speech output (different feature), media playback
  (use maui-media-picker), or general permissions (use maui-permissions).
---

# .NET MAUI Speech-to-Text Implementation

Add on-device speech recognition to any .NET MAUI app using CommunityToolkit.Maui.

## Quick Start

### 1. Install Package

Look up the current version of `CommunityToolkit.Maui` on NuGet before adding:

```xml
<PackageReference Include="CommunityToolkit.Maui" Version="[CURRENT_VERSION]" />
```

### 2. Configure MauiProgram.cs

```csharp
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;

builder.UseMauiCommunityToolkit();

// Register services
builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default);
builder.Services.AddSingleton<ISpeechRecognitionService, SpeechRecognitionService>();
```

### 3. Platform Permissions

**iOS (Info.plist):**
```xml
<key>NSSpeechRecognitionUsageDescription</key>
<string>App uses speech recognition for hands-free input.</string>
<key>NSMicrophoneUsageDescription</key>
<string>App needs microphone access to hear your voice.</string>
```

**Android (AndroidManifest.xml):**
```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
```

## Service Interface

```csharp
public interface ISpeechRecognitionService
{
    SpeechRecognitionState State { get; }
    event EventHandler<SpeechRecognitionState>? StateChanged;
    event EventHandler<string>? PartialResultReceived;
    Task<bool> IsAvailableAsync();
    Task<bool> RequestPermissionsAsync();
    Task<SpeechRecognitionResultDto> StartListeningAsync(CancellationToken cancellationToken = default);
    Task StopListeningAsync();
}
```

## Supporting Types

```csharp
public enum SpeechRecognitionState
{
    Idle = 0,
    Listening = 1,
    Processing = 2,
    Error = 3
}

public record SpeechRecognitionResultDto
{
    public bool Success { get; init; }
    public string? Transcript { get; init; }
    public double Confidence { get; init; }
    public string? ErrorMessage { get; init; }
}
```

## Service Implementation

```csharp
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.Logging;
using System.Globalization;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    private readonly ISpeechToText _speechToText;
    private readonly ILogger<SpeechRecognitionService> _logger;
    private SpeechRecognitionState _state = SpeechRecognitionState.Idle;
    private CancellationTokenSource? _currentCts;
    private TaskCompletionSource<SpeechRecognitionResultDto>? _recognitionTcs;

    public SpeechRecognitionState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<SpeechRecognitionState>? StateChanged;
    public event EventHandler<string>? PartialResultReceived;

    public SpeechRecognitionService(ISpeechToText speechToText, ILogger<SpeechRecognitionService> logger)
    {
        _speechToText = speechToText;
        _logger = logger;
        _speechToText.RecognitionResultCompleted += OnRecognitionResultCompleted;
    }

    public Task<bool> IsAvailableAsync() => Task.FromResult(true);

    public async Task<bool> RequestPermissionsAsync()
    {
        try
        {
            // Check current status first
            var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            var speechStatus = await Permissions.CheckStatusAsync<Permissions.Speech>();

            // If not granted, request permissions from user
            if (micStatus != PermissionStatus.Granted)
                micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
            
            if (speechStatus != PermissionStatus.Granted)
                speechStatus = await Permissions.RequestAsync<Permissions.Speech>();

            return micStatus == PermissionStatus.Granted && speechStatus == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting speech recognition permissions");
            return false;
        }
    }

    public async Task<SpeechRecognitionResultDto> StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (State == SpeechRecognitionState.Listening)
        {
            return new SpeechRecognitionResultDto { Success = false, ErrorMessage = "Already listening" };
        }

        _currentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _recognitionTcs = new TaskCompletionSource<SpeechRecognitionResultDto>();
        State = SpeechRecognitionState.Listening;

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_currentCts.Token, timeoutCts.Token);

        try
        {
            _speechToText.RecognitionResultUpdated += OnRecognitionResultUpdated;

            var options = new SpeechToTextOptions
            {
                Culture = CultureInfo.CurrentCulture,
                ShouldReportPartialResults = true
            };

            await _speechToText.StartListenAsync(options, combinedCts.Token);

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60), combinedCts.Token);
            var completedTask = await Task.WhenAny(_recognitionTcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                await StopListeningAsync();
                return new SpeechRecognitionResultDto { Success = false, ErrorMessage = "Listening timed out." };
            }

            return await _recognitionTcs.Task;
        }
        catch (OperationCanceledException)
        {
            State = SpeechRecognitionState.Idle;
            return new SpeechRecognitionResultDto { Success = false, ErrorMessage = "Cancelled" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during speech recognition");
            State = SpeechRecognitionState.Error;
            return new SpeechRecognitionResultDto { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            _speechToText.RecognitionResultUpdated -= OnRecognitionResultUpdated;
            _currentCts?.Dispose();
            _currentCts = null;
        }
    }

    public async Task StopListeningAsync()
    {
        try { await _speechToText.StopListenAsync(CancellationToken.None); }
        catch (Exception ex) { _logger.LogWarning(ex, "Error stopping speech recognition"); }
        State = SpeechRecognitionState.Idle;
    }

    private void OnRecognitionResultUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        PartialResultReceived?.Invoke(this, args.RecognitionResult);
    }

    private void OnRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        State = SpeechRecognitionState.Processing;
        var result = args.RecognitionResult;

        if (result.IsSuccessful && !string.IsNullOrEmpty(result.Text))
        {
            State = SpeechRecognitionState.Idle;
            _recognitionTcs?.TrySetResult(new SpeechRecognitionResultDto
            {
                Success = true,
                Transcript = result.Text,
                Confidence = 1.0
            });
        }
        else
        {
            State = SpeechRecognitionState.Error;
            _recognitionTcs?.TrySetResult(new SpeechRecognitionResultDto
            {
                Success = false,
                ErrorMessage = result.Exception?.Message ?? "No speech recognized"
            });
        }
    }
}
```

## UI Integration Patterns

The service works with any .NET MAUI UI approach. Below are patterns for each.

### MVVM with XAML

**ViewModel:**
```csharp
public partial class MyViewModel : ObservableObject
{
    private readonly ISpeechRecognitionService _speechService;
    private CancellationTokenSource? _voiceCts;

    [ObservableProperty] private bool _isRecording;
    [ObservableProperty] private string _voiceTranscript = "";
    [ObservableProperty] private SpeechRecognitionState _voiceState;

    public MyViewModel(ISpeechRecognitionService speechService)
    {
        _speechService = speechService;
    }

    [RelayCommand]
    private async Task ToggleRecordingAsync()
    {
        if (IsRecording)
            await StopRecordingAsync();
        else
            await StartRecordingAsync();
    }

    private async Task StartRecordingAsync()
    {
        if (!await _speechService.RequestPermissionsAsync())
            return;

        _voiceCts = new CancellationTokenSource();
        IsRecording = true;
        VoiceTranscript = "";
        VoiceState = SpeechRecognitionState.Listening;
        
        _speechService.PartialResultReceived += OnPartialResult;
        _ = ListenLoopAsync();
    }

    private async Task ListenLoopAsync()
    {
        try
        {
            while (IsRecording && !(_voiceCts?.IsCancellationRequested ?? true))
            {
                var result = await _speechService.StartListeningAsync(_voiceCts?.Token ?? default);
                if (result.Success && !string.IsNullOrWhiteSpace(result.Transcript))
                    await ProcessTranscriptAsync(result.Transcript);
                if (IsRecording) await Task.Delay(100);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _speechService.PartialResultReceived -= OnPartialResult;
            IsRecording = false;
            VoiceState = SpeechRecognitionState.Idle;
        }
    }

    private async Task StopRecordingAsync()
    {
        _voiceCts?.Cancel();
        await _speechService.StopListeningAsync();
        _speechService.PartialResultReceived -= OnPartialResult;
        IsRecording = false;
        VoiceState = SpeechRecognitionState.Idle;
    }

    private void OnPartialResult(object? sender, string text) => VoiceTranscript = text;
    private Task ProcessTranscriptAsync(string transcript) => Task.CompletedTask; // Your logic
}
```

**XAML:**
```xml
<Button Text="{Binding IsRecording, Converter={StaticResource BoolToRecordText}}"
        Command="{Binding ToggleRecordingCommand}" />
<Label Text="{Binding VoiceTranscript}" />
```

### MVVM with C# Markup

```csharp
public class MyPage : ContentPage
{
    public MyPage(MyViewModel vm)
    {
        BindingContext = vm;
        Content = new VerticalStackLayout
        {
            new Button()
                .Text("Record")
                .BindCommand(nameof(vm.ToggleRecordingCommand)),
            new Label()
                .Bind(Label.TextProperty, nameof(vm.VoiceTranscript))
        };
    }
}
```

### MauiReactor

```csharp
partial class MyPage : Component<MyPageState>
{
    [Inject] ISpeechRecognitionService _speechService;
    private CancellationTokenSource? _voiceCts;

    public override VisualNode Render() => ContentPage(
        VStack(
            Button(State.IsRecording ? "Stop" : "Record")
                .OnClicked(ToggleRecordingAsync),
            Label(State.VoiceTranscript)
        )
    );

    private async void ToggleRecordingAsync()
    {
        if (State.IsRecording) await StopRecordingAsync();
        else await StartRecordingAsync();
    }

    private async Task StartRecordingAsync()
    {
        if (!await _speechService.RequestPermissionsAsync()) return;
        _voiceCts = new CancellationTokenSource();
        SetState(s => { s.IsRecording = true; s.VoiceTranscript = ""; });
        _speechService.PartialResultReceived += OnPartialResult;
        _ = ListenLoopAsync();
    }

    private async Task ListenLoopAsync()
    {
        try
        {
            while (State.IsRecording && !(_voiceCts?.IsCancellationRequested ?? true))
            {
                var result = await _speechService.StartListeningAsync(_voiceCts?.Token ?? default);
                if (result.Success && !string.IsNullOrWhiteSpace(result.Transcript))
                    await ProcessTranscriptAsync(result.Transcript);
                if (State.IsRecording) await Task.Delay(100);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _speechService.PartialResultReceived -= OnPartialResult;
            SetState(s => s.IsRecording = false);
        }
    }

    private async Task StopRecordingAsync()
    {
        _voiceCts?.Cancel();
        await _speechService.StopListeningAsync();
        _speechService.PartialResultReceived -= OnPartialResult;
        SetState(s => s.IsRecording = false);
    }

    private void OnPartialResult(object? sender, string text) => SetState(s => s.VoiceTranscript = text);
    private Task ProcessTranscriptAsync(string transcript) => Task.CompletedTask;
}

class MyPageState { public bool IsRecording; public string VoiceTranscript = ""; }
```

## Key Implementation Notes

1. **Permission Handling**: Always call `RequestPermissionsAsync()` before starting speech recognition. It checks status first, then prompts the user if not already granted.

2. **60-Second Timeout**: Built-in safety timeout prevents indefinite listening sessions.

3. **Partial Results**: Subscribe to `PartialResultReceived` for live transcription feedback during speech.

4. **Continuous Listening**: Loop `StartListeningAsync` with small delays for continuous conversation mode.

5. **Cancellation**: Always use `CancellationTokenSource` for clean shutdown and proper resource cleanup.

6. **Natural Language Output**: CommunityToolkit.Maui's `ISpeechToText` returns normalized, punctuated text—not raw phonemes or garbled noise.

7. **UI Agnostic**: The `ISpeechRecognitionService` interface works identically regardless of UI framework (XAML, C# Markup, MauiReactor).
