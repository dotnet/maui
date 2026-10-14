using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Maui.HybridApp;
using Maui.HybridApp.UI;

namespace Maui.Controls.Sample.HybridWebApp;

sealed class MainPage : ContentPage
{
	readonly HybridWebView _view;
	readonly HybridAppTargetObject _hub;
	readonly HybridViewContext _context;
	bool _stopped;
#if DEBUG
	readonly DevelopmentSession? _development;
#endif

	public MainPage(HybridAppTargetObject hub, HybridViewContext context)
	{
		SafeAreaEdges = Microsoft.Maui.SafeAreaEdges.All;
		_hub = hub;
		_context = context;
		context.Bound += BindLocation;
		_hub.DocumentDisconnected += _context.DisconnectRemote;
		_view = new HybridWebView
		{
			HybridRoot = "wwwroot",
			DefaultFile = "index.html",
			BinaryInvoker = new HybridAppBinaryInvoker(hub)
		};
		_hub.NativeEventReceived += SendEvent;
		_view.SetInvokeJavaScriptTarget<IHybridAppTarget>(_hub, ProtocolJsonContext.Default);
		_view.RawMessageReceived += RawMessageReceived;
		_view.HandlerChanging += ViewHandlerChanging;
		Content = context.CreateLayout(_view);
#if DEBUG
		if (MauiProgram.Development is { } settings)
			_development = new DevelopmentSession(_view, settings);
#endif
	}
	void BindLocation()
	{
		_view.DefaultFile = Uri.UnescapeDataString(_context.InitialLocation.Split('?', '#')[0]);
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
		_hub.DocumentDisconnected -= _context.DisconnectRemote;
		_context.Bound -= BindLocation;
		_view.RawMessageReceived -= RawMessageReceived;
		_view.HandlerChanging -= ViewHandlerChanging;
#if DEBUG
		_development?.Dispose();
#endif
	}
}
