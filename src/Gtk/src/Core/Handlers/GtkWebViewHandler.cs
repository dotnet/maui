using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Web;
using Microsoft.Maui.Handlers;
using WebKitView = WebKit.WebView;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="WebViewHandler"/> over a WebKitGTK 6 <c>WebKit.WebView</c>. When WebKitGTK cannot be loaded (any non-Linux system, or no
	/// <c>libwebkitgtk-6.0</c>) the platform view is a <c>Gtk.Label</c> that says why; see <see cref="GtkWebKit"/>.
	/// </summary>
	/// <remarks>
	/// <para>Navigation: <c>Navigating</c> is raised from the WebKit navigation policy decision (and can cancel it), <c>Navigated</c> when the
	/// load finishes or fails. <c>GoBack</c>/<c>GoForward</c>/<c>Reload</c> set the navigation event the next <c>Navigating</c>/<c>Navigated</c>
	/// report. Links that open a new window are loaded in the same view.</para>
	/// <para>JavaScript: <c>EvaluateJavaScriptAsync</c> returns a string result as it is and any other value as JSON. The cookie container
	/// of the virtual view is not synchronised.</para>
	/// </remarks>
	public class GtkWebViewHandler : WebViewHandler
	{
		WebKitView? _view;
		WebNavigationEvent _pendingEvent = WebNavigationEvent.NewPage;
		bool _cancelled;
		bool _failed;

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

		/// <inheritdoc/>
		protected override object CreatePlatformView() => GtkWebKit.CreatePlatformView("WebView");

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not WebKitView view)
				return;

			_view = view;
			view.LoadChanged += OnLoadChanged;
			view.LoadFailed += OnLoadFailed;
			view.DecidePolicy += OnDecidePolicy;
			view.WebProcessTerminated += OnWebProcessTerminated;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is WebKitView view)
			{
				view.LoadChanged -= OnLoadChanged;
				view.LoadFailed -= OnLoadFailed;
				view.DecidePolicy -= OnDecidePolicy;
				view.WebProcessTerminated -= OnWebProcessTerminated;
				view.StopLoading();
			}

			_view = null;
			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IWebView? View => ((IElementHandler)this).VirtualView as IWebView;

		/// <summary>Loads the virtual view's <see cref="IWebView.Source"/> into the web view.</summary>
		void UpdateSource()
		{
			if (_view is null || View is not { } virtualView)
				return;

			_pendingEvent = WebNavigationEvent.NewPage;
			virtualView.Source?.Load(new WebViewDelegate(_view));
		}

		void UpdateUserAgent()
		{
			if (_view is null || View is not { } virtualView)
				return;

			var settings = _view.Settings;
			if (virtualView.UserAgent is { } userAgent)
				settings.UserAgent = userAgent;
			else
				virtualView.UserAgent = settings.UserAgent;
		}

		void GoBack()
		{
			if (_view is null || !_view.CanGoBack())
				return;

			_pendingEvent = WebNavigationEvent.Back;
			_view.GoBack();
			UpdateCanGoBackForward();
		}

		void GoForward()
		{
			if (_view is null || !_view.CanGoForward())
				return;

			_pendingEvent = WebNavigationEvent.Forward;
			_view.GoForward();
			UpdateCanGoBackForward();
		}

		void Reload()
		{
			if (_view is null)
				return;

			_pendingEvent = WebNavigationEvent.Refresh;
			_view.Reload();
		}

		void UpdateCanGoBackForward()
		{
			if (_view is null || View is not { } virtualView)
				return;

			virtualView.CanGoBack = _view.CanGoBack();
			virtualView.CanGoForward = _view.CanGoForward();
		}

		void OnLoadChanged(object? sender, WebKit.LoadChangedArgs args)
		{
			if (_view is null)
				return;

			switch (args.LoadEvent)
			{
				case WebKit.LoadEvent.Committed:
					UpdateCanGoBackForward();
					break;

				case WebKit.LoadEvent.Finished:
					if (_failed)
					{
						// load-failed already reported this load.
						_failed = false;
						break;
					}

					RaiseNavigated(_view.Uri, WebNavigationResult.Success);
					UpdateCanGoBackForward();
					break;
			}
		}

		void OnLoadFailed(object? sender, WebKit.LoadFailedArgs args)
		{
			args.RetVal = false;

			if (_cancelled)
			{
				// The app cancelled this navigation in Navigating; WebKit reports it as a failure.
				_cancelled = false;
				_failed = true;
				return;
			}

			_failed = true;
			RaiseNavigated(args.FailingUri, WebNavigationResult.Failure);
		}

		void RaiseNavigated(string? url, WebNavigationResult result)
		{
			var navigationEvent = _pendingEvent;
			_pendingEvent = WebNavigationEvent.NewPage;

			View?.Navigated(navigationEvent, url ?? string.Empty, result);
		}

		void OnDecidePolicy(object? sender, WebKit.DecidePolicyArgs args)
		{
			if (_view is null || View is not { } virtualView || args.Decision is null)
				return;

			if (args.DecisionType != WebKit.PolicyDecisionType.NavigationAction && args.DecisionType != WebKit.PolicyDecisionType.NewWindowAction)
				return;

			var decision = args.Decision as WebKit.NavigationPolicyDecision ?? new WebKit.NavigationPolicyDecision(args.Decision.Handle);
			var action = decision.NavigationAction;
			var uri = action?.Request?.Uri;
			if (action is null || string.IsNullOrEmpty(uri))
				return;

			_cancelled = false;
			var navigationEvent = GtkWebViewScripts.ResolveNavigationEvent(action.NavigationType, _pendingEvent);
			var cancel = virtualView.Navigating(navigationEvent, uri);

			if (cancel)
			{
				_pendingEvent = WebNavigationEvent.NewPage;
				_cancelled = true;
				decision.Ignore();
				args.RetVal = true;
				return;
			}

			if (args.DecisionType == WebKit.PolicyDecisionType.NewWindowAction)
			{
				// target=_blank and window.open: there is no second web view, so the link loads here.
				decision.Ignore();
				args.RetVal = true;
				_view.LoadUri(uri);
			}
		}

		void OnWebProcessTerminated(object? sender, WebKit.WebProcessTerminatedArgs args) =>
			View?.ProcessTerminated(new WebProcessTerminatedEventArgs());

		sealed class WebViewDelegate : IWebViewDelegate
		{
			readonly WebKitView _view;

			public WebViewDelegate(WebKitView view)
			{
				_view = view;
			}

			public void LoadHtml(string? html, string? baseUrl) => _view.LoadHtml(html ?? string.Empty, baseUrl);

			public void LoadUrl(string? url) => _view.LoadUri(string.IsNullOrEmpty(url) ? "about:blank" : url);
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
			mapper.MapGtk<WebKitView>(nameof(IWebView.Source), (h, v, w) => (h as GtkWebViewHandler)?.UpdateSource());
			mapper.MapGtk<WebKitView>(nameof(IWebView.UserAgent), (h, v, w) => (h as GtkWebViewHandler)?.UpdateUserAgent());

			commands.MapGtk<WebKitView>(nameof(IWebView.GoBack), (h, v, args, w) => (h as GtkWebViewHandler)?.GoBack());
			commands.MapGtk<WebKitView>(nameof(IWebView.GoForward), (h, v, args, w) => (h as GtkWebViewHandler)?.GoForward());
			commands.MapGtk<WebKitView>(nameof(IWebView.Reload), (h, v, args, w) => (h as GtkWebViewHandler)?.Reload());
			commands.MapGtk<WebKitView>(nameof(IWebView.Eval), (h, v, args, w) =>
			{
				if (args is string script)
					GtkWebViewScripting.Run(w, script);
			});
			commands.MapGtk<WebKitView>(nameof(IWebView.EvaluateJavaScriptAsync), (h, v, args, w) =>
			{
				if (args is EvaluateJavaScriptAsyncRequest request)
					GtkWebViewScripting.Evaluate(w, request.Script, request);
			});
		}
	}
}
