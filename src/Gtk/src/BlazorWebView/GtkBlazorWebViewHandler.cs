using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Web;
using WebKitView = WebKit.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// <see cref="BlazorWebViewHandler"/> over a WebKitGTK 6 <c>WebKit.WebView</c> (a <c>Gtk.Label</c> placeholder when WebKitGTK cannot be loaded).
	/// </summary>
	/// <remarks>
	/// <para>The app is served from the <c>wwwroot</c> content next to the executable through the <c>app://0.0.0.1/</c> custom scheme. The page talks
	/// to .NET through <c>webkit.messageHandlers.webwindowinterop</c>, exactly as on iOS. Links are routed through
	/// <see cref="IBlazorWebView.UrlLoading"/>: <see cref="UrlLoadingStrategy.OpenExternally"/> opens the system browser.</para>
	/// <para>Register it with <see cref="BlazorWebViewGtkExtensions.UseGtkPlatformHandler"/>.</para>
	/// </remarks>
	public class GtkBlazorWebViewHandler : BlazorWebViewHandler
	{
		const string BlazorInitScript = @"
			window.__receiveMessageCallbacks = [];
			window.__dispatchMessageCallback = function(message) {
				window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
			};
			window.external = {
				sendMessage: function(message) {
					window.webkit.messageHandlers.webwindowinterop.postMessage(message);
				},
				receiveMessage: function(callback) {
					window.__receiveMessageCallbacks.push(callback);
				}
			};

			Blazor.start();

			(function () {
				window.onpageshow = function(event) {
					if (event.persisted) {
						window.location.reload();
					}
				};
			})();
		";

		const string UseBlockingDisposalSwitch = "BlazorWebView.UseBlockingDisposal";

		WebKitView? _view;
		GtkWebKitSignals? _signals;
		IDisposable? _route;
		GtkWebViewManager? _webviewManager;
		ILogger? _logger;
		string? _hostPage;
		RootComponentsCollection? _rootComponents;

		/// <summary>Creates the handler with the Blazor and GTK mappers.</summary>
		public GtkBlazorWebViewHandler()
			: base(GtkMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the default one.</param>
		public GtkBlazorWebViewHandler(PropertyMapper? mapper)
			: base(mapper ?? GtkMapper)
		{
		}

		ILogger Logger => _logger ??= Services?.GetService<ILogger<GtkBlazorWebViewHandler>>() ?? NullLogger<GtkBlazorWebViewHandler>.Instance;

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IBlazorWebView? View => ((IElementHandler)this).VirtualView as IBlazorWebView;

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var widget = GtkWebKit.CreatePlatformView("BlazorWebView");
			if (widget is not WebKitView view)
				return widget;

			View?.BlazorWebViewInitializing(new BlazorWebViewInitializingEventArgs());

			var content = view.UserContentManager;
			content.RegisterScriptMessageHandler(GtkWebViewScripts.HybridMessageHandlerName, null);
			content.ScriptMessageReceived += OnScriptMessageReceived;
			content.AddScript(new WebKit.UserScript(
				BlazorInitScript,
				WebKit.UserContentInjectedFrames.TopFrame,
				WebKit.UserScriptInjectionTime.End,
				null!,
				null!));

			_route = GtkUriSchemes.Register(view, GtkWebViewScripts.AppScheme, request =>
				_webviewManager is null ? Task.FromResult(GtkUriSchemeResponse.NotFound()) : _webviewManager.RespondAsync(request));

			if (DeveloperTools.Enabled)
				view.Settings.EnableDeveloperExtras = true;

			View?.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs());

			return view;
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is WebKitView view)
			{
				_view = view;
				_signals = new GtkWebKitSignals(view, OnDecidePolicy, null);
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is WebKitView view)
			{
				view.UserContentManager.ScriptMessageReceived -= OnScriptMessageReceived;
				view.UserContentManager.UnregisterScriptMessageHandler(GtkWebViewScripts.HybridMessageHandlerName, null);
				view.StopLoading();
			}

			_signals?.Dispose();
			_signals = null;
			_route?.Dispose();
			_route = null;

			if (_webviewManager is { } manager)
			{
				// Start the disposal...
				var disposalTask = manager.DisposeAsync().AsTask();

				if (AppContext.TryGetSwitch(UseBlockingDisposalSwitch, out var blocking) && blocking)
				{
					// Opt-in: wait for the disposal (this can deadlock).
					disposalTask.GetAwaiter().GetResult();
				}
				else
				{
					disposalTask.FireAndForget(Logger);
				}

				_webviewManager = null;
			}

			if (_rootComponents is not null)
				_rootComponents.CollectionChanged -= OnRootComponentsCollectionChanged;

			_rootComponents = null;
			_view = null;

			base.DisconnectHandler(platformView);
		}

		/// <inheritdoc/>
		public override IFileProvider CreateFileProvider(string contentRootDir) => new GtkMauiAssetFileProvider(contentRootDir);

		/// <inheritdoc/>
		public override async Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
		{
			ArgumentNullException.ThrowIfNull(workItem);

			if (_webviewManager is null)
				return false;

			return await _webviewManager.TryDispatchAsync(workItem);
		}

		void SetHostPage(IBlazorWebView view)
		{
			_hostPage = view.HostPage;
			StartWebViewCoreIfPossible();
		}

		void SetRootComponents(IBlazorWebView view)
		{
			if (_rootComponents is not null)
			{
				// Remove any previously-known root components and unhook events.
				_rootComponents.Clear();
				_rootComponents.CollectionChanged -= OnRootComponentsCollectionChanged;
			}

			_rootComponents = view.RootComponents;

			if (_rootComponents is not null)
			{
				// Add new root components and hook events.
				if (_rootComponents.Count > 0 && _webviewManager is not null)
				{
					_webviewManager.Dispatcher.AssertAccess();
					foreach (var component in _rootComponents)
						_ = component.AddToWebViewManagerAsync(_webviewManager);
				}

				_rootComponents.CollectionChanged += OnRootComponentsCollectionChanged;
			}

			StartWebViewCoreIfPossible();
		}

		void OnRootComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
		{
			// If we haven't initialized yet, this is a no-op.
			if (_webviewManager is not { } manager)
				return;

			// Dispatch because this is going to be async, and we want to catch any errors.
			var dispatchTask = manager.Dispatcher.InvokeAsync(async () =>
			{
				var newItems = eventArgs.NewItems?.Cast<RootComponent>() ?? Enumerable.Empty<RootComponent>();
				var oldItems = eventArgs.OldItems?.Cast<RootComponent>() ?? Enumerable.Empty<RootComponent>();

				foreach (var item in newItems.Except(oldItems))
					await item.AddToWebViewManagerAsync(manager);

				foreach (var item in oldItems.Except(newItems))
					await item.RemoveFromWebViewManagerAsync(manager);
			});

			_ = dispatchTask.ObserveExceptionsAsync(Logger);
		}

		void StartWebViewCoreIfPossible()
		{
			if (_hostPage is null || Services is null || _webviewManager is not null || _view is null || View is not { } virtualView)
				return;

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case.
			var contentRootDir = Path.GetDirectoryName(_hostPage) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, _hostPage);

			Logger.CreatingFileProvider(contentRootDir, hostPageRelativePath);
			var fileProvider = virtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new GtkWebViewManager(
				new WebKitTransport(_view),
				Services,
				new MauiDispatcher(Services.GetRequiredService<IDispatcher>()),
				fileProvider,
				virtualView.JSComponents,
				contentRootDir,
				hostPageRelativePath,
				Logger);

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

			if (_rootComponents is not null)
			{
				foreach (var rootComponent in _rootComponents)
				{
					Logger.AddingRootComponent(rootComponent.ComponentType?.FullName ?? string.Empty, rootComponent.Selector ?? string.Empty, rootComponent.Parameters?.Count ?? 0);

					// Since the page isn't loaded yet, this will always complete synchronously.
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}

			Logger.StartingInitialNavigation(virtualView.StartPath);
			_webviewManager.Navigate(virtualView.StartPath);
		}

		void OnScriptMessageReceived(object? sender, WebKit.ScriptMessageReceivedArgs args)
		{
			try
			{
				var value = args.Value;
				if (value is null || !value.IsString || _webviewManager is null || !Uri.TryCreate(_view?.Uri, UriKind.Absolute, out var source))
					return;

				// WebViewManager ignores messages from pages outside the app origin.
				_webviewManager.MessageReceivedInternal(source, value.ToString());
			}
			catch (Exception e)
			{
				// A failing component must not unwind into the GTK signal emission.
				Logger.LogError(e, "Failed to process a BlazorWebView message: {Message}", e.Message);
			}
		}

		bool OnDecidePolicy(IntPtr decisionHandle, WebKit.PolicyDecisionType decisionType)
		{
			if (decisionHandle == IntPtr.Zero || View is not { } virtualView)
				return false;

			if (decisionType != WebKit.PolicyDecisionType.NavigationAction && decisionType != WebKit.PolicyDecisionType.NewWindowAction)
				return false;

			if (GLib.Object.GetObject(decisionHandle) is not WebKit.NavigationPolicyDecision decision)
				return false;

			var requestUri = decision.NavigationAction?.Request?.Uri;
			if (!Uri.TryCreate(requestUri, UriKind.Absolute, out var uri) || uri.Scheme == "about")
				return false;

			var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, GtkWebViewManager.AppOriginUri);
			virtualView.UrlLoading(callbackArgs);

			Logger.NavigationEvent(uri, callbackArgs.UrlLoadingStrategy);

			if (callbackArgs.UrlLoadingStrategy == UrlLoadingStrategy.OpenExternally)
				Launcher.Default.OpenAsync(uri).FireAndForget(Logger);

			if (callbackArgs.UrlLoadingStrategy != UrlLoadingStrategy.OpenInWebView)
			{
				decision.Ignore();
				return true;
			}

			return false;
		}

		sealed class WebKitTransport : IGtkBlazorTransport
		{
			readonly WebKitView _view;

			public WebKitTransport(WebKitView view)
			{
				_view = view;
			}

			public void LoadUri(string uri) => _view.LoadUri(uri);

			public void RunScript(string script) => GtkWebViewScripting.Run(_view, script);
		}

		static readonly PropertyMapper<IBlazorWebView, BlazorWebViewHandler> GtkMapper = CreateMapper();

		static PropertyMapper<IBlazorWebView, BlazorWebViewHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IBlazorWebView, BlazorWebViewHandler>(BlazorWebViewMapper).AddViewMappings();
			mapper.MapGtk<global::Gtk.Widget>(nameof(IBlazorWebView.HostPage), (h, v, w) => (h as GtkBlazorWebViewHandler)?.SetHostPage(v));
			mapper.MapGtk<global::Gtk.Widget>(nameof(IBlazorWebView.RootComponents), (h, v, w) => (h as GtkBlazorWebViewHandler)?.SetRootComponents(v));
			return (PropertyMapper<IBlazorWebView, BlazorWebViewHandler>)mapper.Build();
		}
	}
}
