using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary><see cref="IGtkWebEngine"/> over the WebView2 control of a <see cref="MauiGtkWebView2Host"/>.</summary>
	/// <remarks>
	/// <para>The controller exists only while the host is realized and is created asynchronously. Until then the engine queues what it is asked
	/// to do (scripts in order, only the last navigation) and runs it when <see cref="IGtkWebEngine.Ready"/> is raised. After a re-realization
	/// (the widget was moved to another window) the controller is new: the engine navigates to the address the old one showed; history is lost.</para>
	/// <para>WebView2 raises <c>NavigationStarting</c> for the app's own loads too; the engine swallows the one that follows
	/// <see cref="LoadUri"/>/<see cref="LoadHtml"/>/<see cref="Reload"/> because the handler already raised <c>Navigating</c> for it.
	/// <see cref="LoadHtml"/> uses <c>NavigateToString</c>, which has no base URL and a size limit of 2 MB (a larger document fails the navigation).</para>
	/// <para>Seam for the HybridWebView and BlazorWebView tracks: subscribe to <see cref="MauiGtkWebView2Host.CoreWebView2Initialized"/> to configure the
	/// <see cref="CoreWebView2"/> before the first navigation; the engine adds nothing there but the navigation, history, process-failure and new-window
	/// handlers, so <c>WebResourceRequested</c>, <c>WebMessageReceived</c> and <c>AddScriptToExecuteOnDocumentCreatedAsync</c> are free.</para>
	/// </remarks>
	internal sealed class MauiGtkWebView2Engine : IGtkWebEngine
	{
		readonly MauiGtkWebView2Host _host;
		readonly List<QueuedOperation> _queue = new();
		CoreWebView2? _core;
		WebView2Cookies? _cookies;
		string? _userAgent;
		Func<string>? _replay;
		string? _pendingHtmlBase;
		string? _currentHtmlBase;
		bool _htmlPending;
		bool _suppressStarting;
		ulong _cancelledNavigationId;
		string? _lastStartUri;
		bool _disposed;

		readonly struct QueuedOperation
		{
			public QueuedOperation(bool isNavigation, Action<CoreWebView2> run)
			{
				IsNavigation = isNavigation;
				Run = run;
			}

			public bool IsNavigation { get; }

			public Action<CoreWebView2> Run { get; }
		}

		internal MauiGtkWebView2Engine(MauiGtkWebView2Host host)
		{
			_host = host;
			host.ControllerCreated += OnControllerCreated;
			host.ControllerClosing += OnControllerClosing;
			host.ControllerFailed += OnControllerFailed;
		}

		/// <inheritdoc/>
		public global::Gtk.Widget Widget => _host;

		/// <summary>The WebView2 control, or <see langword="null"/> before <see cref="Ready"/>.</summary>
		internal CoreWebView2? Core => _core;

		/// <inheritdoc/>
		public string? Uri => _core is null ? null : _currentHtmlBase ?? CurrentSource(_core);

		/// <inheritdoc/>
		public string? Title => _core?.DocumentTitle;

		/// <inheritdoc/>
		public bool CanGoBack => _core?.CanGoBack ?? false;

		/// <inheritdoc/>
		public bool CanGoForward => _core?.CanGoForward ?? false;

		/// <inheritdoc/>
		public string? UserAgent
		{
			get => _core?.Settings.UserAgent;
			set
			{
				_userAgent = value;
				if (_core is not null && value is not null)
					_core.Settings.UserAgent = value;
			}
		}

		/// <inheritdoc/>
		public IGtkWebCookies? Cookies => _cookies;

		/// <inheritdoc/>
		public event EventHandler<GtkWebNavigatingEventArgs>? Navigating;

		/// <inheritdoc/>
		public event EventHandler<GtkWebNavigatedEventArgs>? Navigated;

		/// <inheritdoc/>
		public event EventHandler? HistoryChanged;

		/// <inheritdoc/>
		public event EventHandler? ProcessTerminated;

		/// <inheritdoc/>
		public event EventHandler? Ready;

		/// <inheritdoc/>
		public void LoadUri(string uri)
		{
			_replay = () => uri;
			Enqueue(true, core => StartAppNavigation(core, uri, null, () => core.Navigate(uri)));
		}

		/// <inheritdoc/>
		public void LoadHtml(string html, string? baseUrl)
		{
			_replay = null;
			Enqueue(true, core => StartAppNavigation(core, string.IsNullOrEmpty(baseUrl) ? "about:blank" : baseUrl, baseUrl ?? "about:blank", () => core.NavigateToString(html)));
		}

		/// <inheritdoc/>
		public void GoBack()
		{
			if (_core is { CanGoBack: true } core)
				core.GoBack();
		}

		/// <inheritdoc/>
		public void GoForward()
		{
			if (_core is { CanGoForward: true } core)
				core.GoForward();
		}

		/// <inheritdoc/>
		public void Reload()
		{
			if (_core is { } core)
				StartAppNavigation(core, core.Source, _currentHtmlBase, core.Reload);
		}

		/// <inheritdoc/>
		public void Stop() => _core?.Stop();

		/// <inheritdoc/>
		public void RunScript(string script) =>
			Enqueue(false, core => Observe(core.ExecuteScriptAsync(script)));

		/// <inheritdoc/>
		public void EvaluateScript(string script, EvaluateJavaScriptAsyncRequest request) =>
			Enqueue(false, core => CompleteEvaluation(core, script, request));

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_host.ControllerCreated -= OnControllerCreated;
			_host.ControllerClosing -= OnControllerClosing;
			_host.ControllerFailed -= OnControllerFailed;
			Detach();
			_queue.Clear();
			_host.CloseController();
		}

		void Enqueue(bool isNavigation, Action<CoreWebView2> run)
		{
			if (_disposed)
				return;

			if (_core is { } core)
			{
				run(core);
				return;
			}

			if (isNavigation)
				_queue.RemoveAll(o => o.IsNavigation);

			_queue.Add(new QueuedOperation(isNavigation, run));
		}

		// WebView2 reports an empty Source for some documents (data: URLs); the address of the navigation that started is the best answer then.
		string? CurrentSource(CoreWebView2 core) => string.IsNullOrEmpty(core.Source) ? _lastStartUri : core.Source;

		void StartAppNavigation(CoreWebView2 core, string? uri, string? htmlBase, Action navigate)
		{
			_suppressStarting = true;
			_pendingHtmlBase = htmlBase;
			_htmlPending = htmlBase is not null;

			try
			{
				navigate();
			}
			catch (Exception e) when (e is ArgumentException or InvalidOperationException or System.Runtime.InteropServices.COMException)
			{
				_suppressStarting = false;
				_pendingHtmlBase = null;
				_htmlPending = false;
				Navigated?.Invoke(this, new GtkWebNavigatedEventArgs(uri, WebNavigationResult.Failure));
			}
		}

		void OnControllerCreated(object? sender, IGtkWebView2Controller controller)
		{
			if (_disposed || controller.Core is not { } core)
				return;

			_core = core;
			_cookies = new WebView2Cookies(core);

			if (_userAgent is not null)
				core.Settings.UserAgent = _userAgent;

			core.NavigationStarting += OnNavigationStarting;
			core.NavigationCompleted += OnNavigationCompleted;
			core.HistoryChanged += OnHistoryChanged;
			core.ProcessFailed += OnProcessFailed;
			core.NewWindowRequested += OnNewWindowRequested;

			Ready?.Invoke(this, EventArgs.Empty);

			if (_replay is { } replay && _queue.Count == 0)
			{
				// A re-realized host: show what the closed controller showed.
				var uri = replay();
				if (!string.IsNullOrEmpty(uri) && uri != "about:blank")
					StartAppNavigation(core, uri, null, () => core.Navigate(uri));
			}

			var queued = _queue.ToArray();
			_queue.Clear();
			foreach (var operation in queued)
				operation.Run(core);
		}

		void OnControllerClosing(object? sender, EventArgs e)
		{
			if (_core is { } core && !string.IsNullOrEmpty(core.Source) && core.Source != "about:blank")
			{
				var source = core.Source;
				_replay = () => source;
			}

			Detach();
		}

		void OnControllerFailed(object? sender, Exception e)
		{
			if (_queue.Exists(o => o.IsNavigation))
				Navigated?.Invoke(this, new GtkWebNavigatedEventArgs(null, WebNavigationResult.Failure));

			_queue.Clear();
		}

		void Detach()
		{
			if (_core is { } core)
			{
				core.NavigationStarting -= OnNavigationStarting;
				core.NavigationCompleted -= OnNavigationCompleted;
				core.HistoryChanged -= OnHistoryChanged;
				core.ProcessFailed -= OnProcessFailed;
				core.NewWindowRequested -= OnNewWindowRequested;
			}

			_core = null;
			_cookies = null;
			_suppressStarting = false;
			_htmlPending = false;
		}

		void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
		{
			_lastStartUri = e.Uri;

			if (_suppressStarting && !e.IsRedirected)
			{
				// Started by the app: the handler raised Navigating before calling the engine.
				_suppressStarting = false;
				_currentHtmlBase = _htmlPending ? _pendingHtmlBase : null;
				_htmlPending = false;
				return;
			}

			var args = new GtkWebNavigatingEventArgs(e.Uri, WebNavigationEvent.NewPage);
			Navigating?.Invoke(this, args);

			if (args.Cancel)
			{
				e.Cancel = true;
				_cancelledNavigationId = e.NavigationId;
			}
			else
			{
				_currentHtmlBase = null;
			}
		}

		void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			_suppressStarting = false;

			if (e.NavigationId == _cancelledNavigationId && _cancelledNavigationId != 0)
			{
				// The app cancelled this navigation in Navigating; WebView2 still completes it.
				_cancelledNavigationId = 0;
				return;
			}

			var result = e.IsSuccess
				? WebNavigationResult.Success
				: e.WebErrorStatus == CoreWebView2WebErrorStatus.Timeout ? WebNavigationResult.Timeout : WebNavigationResult.Failure;

			var uri = e.IsSuccess ? Uri ?? _lastStartUri : _lastStartUri ?? Uri;
			Navigated?.Invoke(this, new GtkWebNavigatedEventArgs(uri, result));
		}

		void OnHistoryChanged(object? sender, object e) => HistoryChanged?.Invoke(this, EventArgs.Empty);

		void OnProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
		{
			if (e.ProcessFailedKind is CoreWebView2ProcessFailedKind.BrowserProcessExited or CoreWebView2ProcessFailedKind.RenderProcessExited)
				ProcessTerminated?.Invoke(this, EventArgs.Empty);
		}

		void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
		{
			// target=_blank and window.open: there is no second web view, so the link loads here.
			e.Handled = true;

			if (_core is not { } core)
				return;

			var args = new GtkWebNavigatingEventArgs(e.Uri, WebNavigationEvent.NewPage);
			Navigating?.Invoke(this, args);
			if (args.Cancel)
				return;

			var uri = e.Uri;
			StartAppNavigation(core, uri, null, () => core.Navigate(uri));
		}

		static void CompleteEvaluation(CoreWebView2 core, string script, EvaluateJavaScriptAsyncRequest request)
		{
			Task<string> task;
			try
			{
				task = core.ExecuteScriptAsync(script);
			}
			catch (Exception e)
			{
				request.TrySetException(e);
				return;
			}

			task.ContinueWith(
				t =>
				{
					// Runs on the thread that completed the script (the GTK thread).
					if (t.IsFaulted)
						request.TrySetException(t.Exception!.GetBaseException());
					else if (t.IsCanceled)
						request.TrySetCanceled();
					else
						request.TrySetResult(GtkWebViewScripts.ConvertWebView2Result(t.Result)!);
				},
				TaskContinuationOptions.ExecuteSynchronously);
		}

		static void Observe(Task task) =>
			task.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);

		sealed class WebView2Cookies : IGtkWebCookies
		{
			readonly CoreWebView2 _core;

			internal WebView2Cookies(CoreWebView2 core) => _core = core;

			public void Set(CookieCollection cookies)
			{
				var manager = _core.CookieManager;
				foreach (Cookie cookie in cookies)
				{
					if (cookie.Expired)
					{
						// WebView2 applies the deletion asynchronously: a read right after may still return the cookie.
						manager.DeleteCookie(manager.CreateCookieWithSystemNetCookie(cookie));
					}
					else
						manager.AddOrUpdateCookie(manager.CreateCookieWithSystemNetCookie(cookie));
				}
			}

			public async Task<CookieCollection> GetAsync(Uri uri)
			{
				var result = new CookieCollection();
				foreach (var cookie in await _core.CookieManager.GetCookiesAsync(uri.AbsoluteUri))
					result.Add(cookie.ToSystemNetCookie());

				return result;
			}
		}
	}
}
