using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Maui.Controls.Sample.HybridWebApp.Playground;

namespace Maui.Controls.Sample.HybridWebApp;

sealed class MainPage : ContentPage
{
	readonly HybridWebView _view;
	readonly HybridAppTargetObject _hub;
#if DEBUG
	readonly DevelopmentSession? _development;
#endif

	public MainPage()
	{
		_view = new HybridWebView
		{
			HybridRoot = "wwwroot",
			DefaultFile = "index.html"
		};
		var registry = new CommandRegistry();
		EssentialsProvider.Register(registry, new MauiPlaygroundPlatform());
		new SampleCommands().Register(registry);
		_hub = new HybridAppTargetObject(registry,
			() => new ReadyResult(registry.Names, EssentialsProvider.PreferenceNamespace, EssentialsProvider.MaximumTextLength),
			SendEvent);
		_view.SetInvokeJavaScriptTarget<IHybridAppTarget>(_hub, PlaygroundJsonContext.Default);
		_view.RawMessageReceived += RawMessageReceived;
		_view.HandlerChanging += ViewHandlerChanging;
		Content = _view;
#if DEBUG
		if (MauiProgram.Development is { } settings)
			_development = new DevelopmentSession(_view, settings);
#endif
	}

	void SendEvent(NativeEvent message) => MainThread.BeginInvokeOnMainThread(() =>
	{
		if (_hub.IsCurrent(message.Session) && _view.Handler is not null)
			_view.SendRawMessage(JsonSerializer.Serialize(message, PlaygroundJsonContext.Default.NativeEvent));
	});

	void RawMessageReceived(object? sender, HybridWebViewRawMessageReceivedEventArgs e) =>
		_hub.ReceiveDocumentMessage(e.Message);

	void ViewHandlerChanging(object? sender, HandlerChangingEventArgs e)
	{
		if (e.OldHandler is not null)
			_hub.Disconnect();
	}

	internal void Stop()
	{
		_view.RawMessageReceived -= RawMessageReceived;
		_view.HandlerChanging -= ViewHandlerChanging;
		_hub.Dispose();
#if DEBUG
		_development?.Dispose();
#endif
	}
}
