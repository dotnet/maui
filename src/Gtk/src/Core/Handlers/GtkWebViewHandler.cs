using System;
using System.Net;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Web;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="WebViewHandler"/> over the browser engine of the platform, chosen at run time by <c>GtkWebBackend</c>: a WebKitGTK 6
	/// <c>WebKit.WebView</c> on Linux, a WebView2 control hosted in a child window of the GTK toplevel on Windows (<c>MauiGtkWebView2Host</c>).
	/// When there is no engine (any other system, no <c>libwebkitgtk-6.0</c>, no WebView2 Runtime) the platform view is a <c>Gtk.Label</c> that
	/// says why; see <see cref="GtkWebKit"/>.
	/// </summary>
	/// <remarks>
	/// <para>Navigation: <c>Navigating</c> is raised for the loads the app starts and, from the engine, for the navigations the page starts (and can
	/// cancel them), <c>Navigated</c> when the load finishes or fails. <c>GoBack</c>/<c>GoForward</c>/<c>Reload</c> set the navigation event the next
	/// <c>Navigating</c>/<c>Navigated</c> report. Links that open a new window are loaded in the same view.</para>
	/// <para>JavaScript: <c>EvaluateJavaScriptAsync</c> returns a string result as it is and any other value as JSON. Cookies: WebKitGTK does not
	/// synchronise the cookie container of the virtual view; on WebView2 the cookies of the container for a URL are written to the browser before
	/// each navigation to it and the browser's cookies for it are added to the container after the load.</para>
	/// <para>WebView2: the control is created when the host widget is realized, so a web view that is not in a window yet loads nothing; the
	/// operations issued before are kept (scripts in order, the last navigation). <c>HtmlWebViewSource.BaseUrl</c> is ignored and the HTML is limited
	/// to 2 MB. See <c>MauiGtkWebView2Host</c> for the placement, focus and "airspace" rules.</para>
	/// </remarks>
	public class GtkWebViewHandler : WebViewHandler
	{
		IGtkWebEngine? _engine;
		WebNavigationEvent _pendingEvent = WebNavigationEvent.NewPage;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkWebViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkWebViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <summary>The engine behind the platform view, or <see langword="null"/> for a placeholder or a disconnected handler.</summary>
		internal IGtkWebEngine? Engine => _engine;

		/// <inheritdoc/>
		protected override object CreatePlatformView() => GtkWebBackend.CreatePlatformView("WebView");

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not global::Gtk.Widget widget || GtkWebBackend.CreateEngine(widget) is not { } engine)
				return;

			_engine = engine;
			engine.Navigating += OnEngineNavigating;
			engine.Navigated += OnEngineNavigated;
			engine.HistoryChanged += OnEngineHistoryChanged;
			engine.ProcessTerminated += OnEngineProcessTerminated;
			engine.Ready += OnEngineReady;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (_engine is { } engine)
			{
				engine.Navigating -= OnEngineNavigating;
				engine.Navigated -= OnEngineNavigated;
				engine.HistoryChanged -= OnEngineHistoryChanged;
				engine.ProcessTerminated -= OnEngineProcessTerminated;
				engine.Ready -= OnEngineReady;
				engine.Dispose();
			}

			_engine = null;
			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IWebView? View => ((IElementHandler)this).VirtualView as IWebView;

		/// <summary>Loads the virtual view's <see cref="IWebView.Source"/> into the web view.</summary>
		void UpdateSource()
		{
			if (_engine is null || View is not { } virtualView)
				return;

			_pendingEvent = WebNavigationEvent.NewPage;
			virtualView.Source?.Load(new WebViewDelegate(this));
		}

		void UpdateUserAgent()
		{
			if (_engine is null || View is not { } virtualView)
				return;

			if (virtualView.UserAgent is { } userAgent)
				_engine.UserAgent = userAgent;
			else if (_engine.UserAgent is { } engineAgent)
				virtualView.UserAgent = engineAgent;
		}

		void GoBack()
		{
			if (_engine is null || !_engine.CanGoBack)
				return;

			_pendingEvent = WebNavigationEvent.Back;
			_engine.GoBack();
			UpdateCanGoBackForward();
		}

		void GoForward()
		{
			if (_engine is null || !_engine.CanGoForward)
				return;

			_pendingEvent = WebNavigationEvent.Forward;
			_engine.GoForward();
			UpdateCanGoBackForward();
		}

		void Reload()
		{
			if (_engine is null)
				return;

			if (RaiseNavigating(WebNavigationEvent.Refresh, _engine.Uri))
				return;

			_pendingEvent = WebNavigationEvent.Refresh;
			_engine.Reload();
		}

		void RunScript(string script) => _engine?.RunScript(script);

		void EvaluateScript(EvaluateJavaScriptAsyncRequest request) => _engine?.EvaluateScript(request.Script, request);

		void UpdateCanGoBackForward()
		{
			if (_engine is null || View is not { } virtualView)
				return;

			virtualView.CanGoBack = _engine.CanGoBack;
			virtualView.CanGoForward = _engine.CanGoForward;
		}

		void OnEngineHistoryChanged(object? sender, EventArgs e) => UpdateCanGoBackForward();

		void OnEngineReady(object? sender, EventArgs e) => UpdateUserAgent();

		void OnEngineProcessTerminated(object? sender, EventArgs e) =>
			View?.ProcessTerminated(new WebProcessTerminatedEventArgs());

		void OnEngineNavigated(object? sender, GtkWebNavigatedEventArgs args)
		{
			RaiseNavigated(args.Uri, args.Result);

			if (args.Result == WebNavigationResult.Success)
			{
				UpdateCanGoBackForward();
				SyncCookiesFromEngine(args.Uri);
			}
		}

		/// <summary>The page started a navigation (a link, a form, a script, a redirect, back/forward).</summary>
		void OnEngineNavigating(object? sender, GtkWebNavigatingEventArgs args)
		{
			if (View is not { } virtualView)
				return;

			var navigationEvent = _pendingEvent != WebNavigationEvent.NewPage ? _pendingEvent : args.NavigationEvent;
			if (virtualView.Navigating(navigationEvent, args.Uri))
			{
				_pendingEvent = WebNavigationEvent.NewPage;
				args.Cancel = true;
				return;
			}

			SyncCookiesToEngine(args.Uri);
		}

		/// <summary>
		/// The engine does not report the loads the app starts, so those raise <c>Navigating</c> here. Returns <see langword="true"/> when the app
		/// cancelled the navigation.
		/// </summary>
		bool RaiseNavigating(WebNavigationEvent navigationEvent, string? url)
		{
			if (View is not { } virtualView || string.IsNullOrEmpty(url))
				return false;

			var cancel = virtualView.Navigating(navigationEvent, url);
			if (cancel)
				_pendingEvent = WebNavigationEvent.NewPage;
			else
				SyncCookiesToEngine(url);

			return cancel;
		}

		void RaiseNavigated(string? url, WebNavigationResult result)
		{
			var navigationEvent = _pendingEvent;
			_pendingEvent = WebNavigationEvent.NewPage;

			View?.Navigated(navigationEvent, url ?? string.Empty, result);
		}

		static Uri? CookieUri(string? url) =>
			Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) ? uri : null;

		/// <summary>Writes the cookies of the virtual view's container for <paramref name="url"/> into the engine's store.</summary>
		void SyncCookiesToEngine(string? url)
		{
			if (_engine?.Cookies is not { } store || View?.Cookies is not { } container || CookieUri(url) is not { } uri)
				return;

			var cookies = container.GetCookies(uri);
			if (cookies.Count > 0)
				store.Set(cookies);
		}

		/// <summary>Adds the engine's cookies for <paramref name="url"/> to the virtual view's container.</summary>
		async void SyncCookiesFromEngine(string? url)
		{
			if (_engine?.Cookies is not { } store || View?.Cookies is not { } container || CookieUri(url) is not { } uri)
				return;

			try
			{
				var cookies = await store.GetAsync(uri);
				if (cookies.Count > 0)
					container.Add(uri, cookies);
			}
			catch (Exception)
			{
				// The store went away (disconnect, controller closed) while the cookies were read.
			}
		}

		sealed class WebViewDelegate : IWebViewDelegate
		{
			readonly GtkWebViewHandler _handler;

			public WebViewDelegate(GtkWebViewHandler handler) => _handler = handler;

			public void LoadHtml(string? html, string? baseUrl)
			{
				if (_handler._engine is not { } engine)
					return;

				if (_handler.RaiseNavigating(WebNavigationEvent.NewPage, string.IsNullOrEmpty(baseUrl) ? "about:blank" : baseUrl))
					return;

				engine.LoadHtml(html ?? string.Empty, baseUrl);
			}

			public void LoadUrl(string? url)
			{
				if (_handler._engine is not { } engine)
					return;

				var target = string.IsNullOrEmpty(url) ? "about:blank" : url;
				if (_handler.RaiseNavigating(WebNavigationEvent.NewPage, target))
					return;

				engine.LoadUri(target);
			}
		}

		internal static readonly IPropertyMapper<IWebView, IWebViewHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IWebView, IWebViewHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IWebView, IWebViewHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IWebView, IWebViewHandler>(WebViewHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IWebView, IWebViewHandler>(WebViewHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IWebView, IWebViewHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IWebView, IWebViewHandler>(WebViewHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IWebView, IWebViewHandler>(WebViewHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IWebView, IWebViewHandler> mapper, GtkCommandMapperBuilder<IWebView, IWebViewHandler> commands)
		{
			mapper.MapGtk<global::Gtk.Widget>(nameof(IWebView.Source), (h, v, w) => (h as GtkWebViewHandler)?.UpdateSource());
			mapper.MapGtk<global::Gtk.Widget>(nameof(IWebView.UserAgent), (h, v, w) => (h as GtkWebViewHandler)?.UpdateUserAgent());

			commands.MapGtk<global::Gtk.Widget>(nameof(IWebView.GoBack), (h, v, args, w) => (h as GtkWebViewHandler)?.GoBack());
			commands.MapGtk<global::Gtk.Widget>(nameof(IWebView.GoForward), (h, v, args, w) => (h as GtkWebViewHandler)?.GoForward());
			commands.MapGtk<global::Gtk.Widget>(nameof(IWebView.Reload), (h, v, args, w) => (h as GtkWebViewHandler)?.Reload());
			commands.MapGtk<global::Gtk.Widget>(nameof(IWebView.Eval), (h, v, args, w) =>
			{
				if (args is string script)
					(h as GtkWebViewHandler)?.RunScript(script);
			});
			commands.MapGtk<global::Gtk.Widget>(nameof(IWebView.EvaluateJavaScriptAsync), (h, v, args, w) =>
			{
				if (args is EvaluateJavaScriptAsyncRequest request)
					(h as GtkWebViewHandler)?.EvaluateScript(request);
			});
		}
	}
}
