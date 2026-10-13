using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Maui.HybridApp;

namespace Maui.Controls.Sample.HybridWebApp;

sealed class MainPage : ContentPage
{
	readonly HybridWebView _view;
	readonly HybridAppTargetObject _hub;
	bool _stopped;
#if DEBUG
	readonly DevelopmentSession? _development;
#endif

	public MainPage(HybridAppTargetObject hub)
	{
		_hub = hub;
		_view = new HybridWebView
		{
			HybridRoot = "wwwroot",
			DefaultFile = "index.html"
		};
		_hub.NativeEventReceived += SendEvent;
		_view.SetInvokeJavaScriptTarget<IHybridAppTarget>(_hub, ProtocolJsonContext.Default);
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
		if (!_stopped && _hub.IsCurrent(message.Session) && _view.Handler is not null)
			_view.SendRawMessage(JsonSerializer.Serialize(message, ProtocolJsonContext.Default.NativeEvent));
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
		if (_stopped)
			return;
		_stopped = true;
		_hub.NativeEventReceived -= SendEvent;
		_view.RawMessageReceived -= RawMessageReceived;
		_view.HandlerChanging -= ViewHandlerChanging;
#if DEBUG
		_development?.Dispose();
#endif
	}
}
