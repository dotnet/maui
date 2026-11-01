using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Web;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Storage;
using WebKitView = WebKit.WebView;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="HybridWebViewHandler"/> over a WebKitGTK 6 <c>WebKit.WebView</c> on Linux or a WebView2 child-window host
	/// (<see cref="MauiGtkWebView2Host"/>) on Windows; a <c>Gtk.Label</c> placeholder when neither engine is usable.
	/// </summary>
	/// <remarks>
	/// <para><b>Windows (WebView2).</b> The app origin is <c>https://0.0.0.1/</c>, as on the other WebView2/Android platforms: <c>WebResourceRequested</c>
	/// answers it through the same <c>GtkHybridWebViewResponder</c> (with the <c>X-Maui-Invoke-Token</c>/<c>Origin</c> header check, since WebView2
	/// exposes the headers), and <c>hybridwebview.js</c> talks over <c>chrome.webview.postMessage</c> / <c>WebMessageReceived</c> (see
	/// <c>GtkHybridWebView2Bridge</c>). The environment is the process-wide one of the GTK backend, so the <c>BrowserExecutableFolder</c>, <c>UserDataFolder</c>
	/// and controller options of <see cref="WebViewInitializationStartedEventArgs"/> are not applied. The airspace limits of
	/// <see cref="MauiGtkWebView2Host"/> apply.</para>
	/// <para><b>Linux (WebKitGTK).</b> The web app is served from the app package through the <c>app://0.0.0.1/</c> custom scheme (WebKit does not let an app serve
	/// <c>https</c>; iOS does the same). JavaScript posts messages to <c>window.webkit.messageHandlers.webwindowinterop</c>, which this handler
	/// registers; .NET answers with <c>window.external.receiveMessage(...)</c>. Both match what <c>hybridwebview.js</c> already does for iOS.</para>
	/// <para>Requests to <c>__hwvInvokeDotNet</c> are accepted from pages of the app origin only (WebKitGTK exposes no request headers; see
	/// <c>GtkHybridWebViewResponder</c>). <c>WebResourceRequested</c> interception is not available.</para>
	/// </remarks>
#pragma warning disable IL2026, IL3050 // HybridWebView uses reflection-based JSON; the stock handler carries the same RequiresUnreferencedCode contract.
	public class GtkHybridWebViewHandler : HybridWebViewHandler
	{
		WebKitView? _view;
		IDisposable? _route;
		GtkHybridWebViewResponder? _responder;
		GtkHybridWebView2Bridge? _webView2;
		IGtkWebEngine? _engine;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkHybridWebViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkHybridWebViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IHybridWebView? View => ((IElementHandler)this).VirtualView as IHybridWebView;

		ILogger? Logger => MauiContext?.CreateLogger<GtkHybridWebViewHandler>();

		/// <summary>The WebView2 engine, or <see langword="null"/> on WebKitGTK, for a placeholder and once the handler disconnected.</summary>
		internal IGtkWebEngine? Engine => _engine;

		GtkHybridWebViewResponder CreateResponder(Uri? origin = null) => new(
			() => View?.HybridRoot,
			() => View?.DefaultFile,
			body => InvokeDotNetAsync(new MemoryStream(body, writable: false)),
			async path => await FileSystem.AppPackageFileExistsAsync(path) ? await FileSystem.OpenAppPackageFileAsync(path) : null,
			GetEmbeddedStream,
			Logger,
			origin);

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var widget = GtkWebBackend.CreatePlatformView("HybridWebView");
			if (widget is not WebKitView view)
			{
				if (OperatingSystem.IsWindows())
					AttachWebView2(widget);

				return widget;
			}

			View?.WebViewInitializationStarted(new WebViewInitializationStartedEventArgs());

			var responder = CreateResponder();

			_responder = responder;
			_route = GtkUriSchemes.Register(view, GtkWebViewScripts.AppScheme, request =>
				View is null ? Task.FromResult(GtkUriSchemeResponse.NotFound()) : responder.RespondAsync(request));

			var content = view.UserContentManager;
			content.RegisterScriptMessageHandler(GtkWebViewScripts.HybridMessageHandlerName, null);
			content.ScriptMessageReceived += OnScriptMessageReceived;

			if (DeveloperTools.Enabled)
				view.Settings.EnableDeveloperExtras = true;

			View?.WebViewInitializationCompleted(new WebViewInitializationCompletedEventArgs());

			return view;
		}

		// NoInlining keeps the WebView2 types out of the methods that run on Linux.
		[MethodImpl(MethodImplOptions.NoInlining)]
		void AttachWebView2(global::Gtk.Widget widget)
		{
			var responder = CreateResponder(GtkHybridWebViewResponder.WebView2OriginUri);
			var bridge = GtkHybridWebView2Bridge.TryCreate(widget, responder, MessageReceived, () => DeveloperTools.Enabled, Logger);
			if (bridge is null)
				return;

			View?.WebViewInitializationStarted(new WebViewInitializationStartedEventArgs());

			_responder = responder;
			_webView2 = bridge;
			_engine = bridge.Engine;

			View?.WebViewInitializationCompleted(new WebViewInitializationCompletedEventArgs());
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is WebKitView view)
			{
				_view = view;
				view.LoadUri(GtkHybridWebViewResponder.AppOriginUri.ToString());
			}
			else if (_engine is { } engine)
			{
				// Queued until the host is in a window and its controller exists; the bridge has configured the web view by then.
				engine.LoadUri(GtkHybridWebViewResponder.WebView2OriginUri.ToString());
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (_webView2 is { } bridge)
			{
				_engine?.Stop();
				bridge.Dispose();
				_engine?.Dispose();
			}

			_webView2 = null;
			_engine = null;

			if (platformView is WebKitView view)
			{
				var content = view.UserContentManager;
				content.ScriptMessageReceived -= OnScriptMessageReceived;
				content.UnregisterScriptMessageHandler(GtkWebViewScripts.HybridMessageHandlerName, null);
				view.StopLoading();
			}

			_route?.Dispose();
			_route = null;
			_responder = null;
			_view = null;

			base.DisconnectHandler(platformView);
		}

		void OnScriptMessageReceived(object? sender, WebKit.ScriptMessageReceivedArgs args)
		{
			try
			{
				if (!GtkHybridWebViewResponder.IsFromAppOrigin(_view?.Uri))
				{
					Logger?.LogDebug("Ignoring web message from an unrecognized source.");
					return;
				}

				var value = args.Value;
				if (value is null || !value.IsString)
					return;

				MessageReceived(value.ToString());
			}
			catch (Exception e)
			{
				// A malformed message must not unwind into the GTK signal emission.
				Logger?.LogError(e, "Failed to process a HybridWebView message: {Message}", e.Message);
			}
		}

		static async void EvaluateJavaScriptAsync(IHybridWebViewHandler handler, EvaluateJavaScriptAsyncRequest request)
		{
			var engine = (handler as GtkHybridWebViewHandler)?._engine;
			if (handler.PlatformView is not WebKitView && engine is null)
			{
				request.TrySetCanceled();
				return;
			}

			var inner = new EvaluateJavaScriptAsyncRequest(GtkWebViewScripts.WrapHybridEvaluate(request.Script));
			if (handler.PlatformView is WebKitView view)
				GtkWebViewScripting.Evaluate(view, inner.Script, inner);
			else
				engine!.EvaluateScript(inner.Script, inner);

			try
			{
				request.TrySetResult(GtkWebViewScripts.UnwrapHybridResult(await inner.Task)!);
			}
			catch (Exception e)
			{
				request.TrySetException(e);
			}
		}

		static async void InvokeJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView view, HybridWebViewInvokeJavaScriptRequest request)
		{
			try
			{
				request.SetResult(await MapInvokeJavaScriptAsyncImpl(handler, view, request));
			}
			catch (Exception e)
			{
				request.SetException(e);
			}
		}

		internal static readonly IPropertyMapper<IHybridWebView, IHybridWebViewHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IHybridWebView, IHybridWebViewHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IHybridWebView, IHybridWebViewHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IHybridWebView, IHybridWebViewHandler>(HybridWebViewHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IHybridWebView, IHybridWebViewHandler>(HybridWebViewHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IHybridWebView, IHybridWebViewHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IHybridWebView, IHybridWebViewHandler>(HybridWebViewHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IHybridWebView, IHybridWebViewHandler>(HybridWebViewHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IHybridWebView, IHybridWebViewHandler> mapper, GtkCommandMapperBuilder<IHybridWebView, IHybridWebViewHandler> commands)
		{
			// Widget: the platform view is a WebKit view, a WebView2 host or a placeholder label; the commands tell them apart.
			commands.MapGtk<global::Gtk.Widget>(nameof(IHybridWebView.EvaluateJavaScriptAsync), (h, v, args, w) =>
			{
				if (args is EvaluateJavaScriptAsyncRequest request)
					EvaluateJavaScriptAsync(h, request);
			}, callStock: false);

			commands.MapGtk<global::Gtk.Widget>(nameof(IHybridWebView.InvokeJavaScriptAsync), (h, v, args, w) =>
			{
				if (args is HybridWebViewInvokeJavaScriptRequest request && (w is WebKitView || (h as GtkHybridWebViewHandler)?._engine is not null))
					InvokeJavaScriptAsync(h, v, request);
			}, callStock: false);

			commands.MapGtk<global::Gtk.Widget>(nameof(IHybridWebView.SendRawMessage), (h, v, args, w) =>
			{
				if (args is not HybridWebViewRawMessage message)
					return;

				if (w is WebKitView view)
					GtkWebViewScripting.Run(view, GtkWebViewScripts.ReceiveRawMessage(message.Message ?? string.Empty));
				else
					(h as GtkHybridWebViewHandler)?._webView2?.SendRawMessage(message.Message ?? string.Empty);
			}, callStock: false);
		}
	}
#pragma warning restore IL2026, IL3050
}
