using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
	/// <see cref="HybridWebViewHandler"/> over a WebKitGTK 6 <c>WebKit.WebView</c> (a <c>Gtk.Label</c> placeholder when WebKitGTK cannot be loaded).
	/// </summary>
	/// <remarks>
	/// <para>The web app is served from the app package through the <c>app://0.0.0.1/</c> custom scheme (WebKit does not let an app serve
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

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var widget = GtkWebKit.CreatePlatformView("HybridWebView");
			if (widget is not WebKitView view)
				return widget;

			View?.WebViewInitializationStarted(new WebViewInitializationStartedEventArgs());

			var responder = new GtkHybridWebViewResponder(
				() => View?.HybridRoot,
				() => View?.DefaultFile,
				body => InvokeDotNetAsync(new MemoryStream(body, writable: false)),
				async path => await FileSystem.AppPackageFileExistsAsync(path) ? await FileSystem.OpenAppPackageFileAsync(path) : null,
				GetEmbeddedStream,
				Logger);

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

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is WebKitView view)
			{
				_view = view;
				view.LoadUri(GtkHybridWebViewResponder.AppOriginUri.ToString());
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
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
			if (handler.PlatformView is not WebKitView view)
			{
				request.TrySetCanceled();
				return;
			}

			var inner = new EvaluateJavaScriptAsyncRequest(GtkWebViewScripts.WrapHybridEvaluate(request.Script));
			GtkWebViewScripting.Evaluate(view, inner.Script, inner);

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
			commands.MapGtk<WebKitView>(nameof(IHybridWebView.EvaluateJavaScriptAsync), (h, v, args, w) =>
			{
				if (args is EvaluateJavaScriptAsyncRequest request)
					EvaluateJavaScriptAsync(h, request);
			}, callStock: false);

			commands.MapGtk<WebKitView>(nameof(IHybridWebView.InvokeJavaScriptAsync), (h, v, args, w) =>
			{
				if (args is HybridWebViewInvokeJavaScriptRequest request)
					InvokeJavaScriptAsync(h, v, request);
			}, callStock: false);

			commands.MapGtk<WebKitView>(nameof(IHybridWebView.SendRawMessage), (h, v, args, w) =>
			{
				if (args is HybridWebViewRawMessage message)
					GtkWebViewScripting.Run(w, GtkWebViewScripts.ReceiveRawMessage(message.Message ?? string.Empty));
			}, callStock: false);
		}
	}
#pragma warning restore IL2026, IL3050
}
