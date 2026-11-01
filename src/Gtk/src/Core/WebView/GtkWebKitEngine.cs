using System;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary><see cref="IGtkWebEngine"/> over a WebKitGTK 6 <c>WebKit.WebView</c>.</summary>
	/// <remarks>
	/// The WebKit navigation policy (<c>decide-policy</c>) reports the navigations the page starts, <c>load-failed</c> and
	/// <c>load-changed</c> the outcome; both signal quirks are described on <see cref="GtkWebKitSignals"/>. Links that open a new window
	/// are loaded in the same view.
	/// </remarks>
	internal sealed class GtkWebKitEngine : IGtkWebEngine
	{
		readonly global::WebKit.WebView _view;
		readonly GtkWebKitSignals _signals;
		bool _cancelled;
		bool _failed;
		bool _disposed;

		/// <summary>Wraps <paramref name="view"/> and connects its signals.</summary>
		internal GtkWebKitEngine(global::WebKit.WebView view)
		{
			_view = view;
			view.LoadChanged += OnLoadChanged;
			_signals = new GtkWebKitSignals(view, OnDecidePolicy, OnLoadFailed);
			view.WebProcessTerminated += OnWebProcessTerminated;
		}

		/// <summary>The wrapped WebKit view.</summary>
		internal global::WebKit.WebView View => _view;

		/// <inheritdoc/>
		public global::Gtk.Widget Widget => _view;

		/// <inheritdoc/>
		public string? Uri => _view.Uri;

		/// <inheritdoc/>
		public string? Title => _view.Title;

		/// <inheritdoc/>
		public bool CanGoBack => _view.CanGoBack();

		/// <inheritdoc/>
		public bool CanGoForward => _view.CanGoForward();

		/// <inheritdoc/>
		public string? UserAgent
		{
			get => _view.Settings.UserAgent;
			set => _view.Settings.UserAgent = value;
		}

		/// <inheritdoc/>
		public IGtkWebCookies? Cookies => null;

		/// <inheritdoc/>
		public event EventHandler<GtkWebNavigatingEventArgs>? Navigating;

		/// <inheritdoc/>
		public event EventHandler<GtkWebNavigatedEventArgs>? Navigated;

		/// <inheritdoc/>
		public event EventHandler? HistoryChanged;

		/// <inheritdoc/>
		public event EventHandler? ProcessTerminated;

		/// <inheritdoc/>
		public event EventHandler? Ready
		{
			add { }
			remove { }
		}

		/// <inheritdoc/>
		public void LoadUri(string uri) => _view.LoadUri(uri);

		/// <inheritdoc/>
		public void LoadHtml(string html, string? baseUrl) => _view.LoadHtml(html, baseUrl);

		/// <inheritdoc/>
		public void GoBack() => _view.GoBack();

		/// <inheritdoc/>
		public void GoForward() => _view.GoForward();

		/// <inheritdoc/>
		public void Reload() => _view.Reload();

		/// <inheritdoc/>
		public void Stop() => _view.StopLoading();

		/// <inheritdoc/>
		public void RunScript(string script) => GtkWebViewScripting.Run(_view, script);

		/// <inheritdoc/>
		public void EvaluateScript(string script, EvaluateJavaScriptAsyncRequest request) =>
			GtkWebViewScripting.Evaluate(_view, script, request);

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_view.LoadChanged -= OnLoadChanged;
			_view.WebProcessTerminated -= OnWebProcessTerminated;
			_view.StopLoading();
			_signals.Dispose();
		}

		void OnLoadChanged(object? sender, WebKit.LoadChangedArgs args)
		{
			switch (args.LoadEvent)
			{
				case WebKit.LoadEvent.Committed:
					HistoryChanged?.Invoke(this, EventArgs.Empty);
					break;

				case WebKit.LoadEvent.Finished:
					if (_failed)
					{
						// load-failed already reported this load.
						_failed = false;
						break;
					}

					Navigated?.Invoke(this, new GtkWebNavigatedEventArgs(_view.Uri, WebNavigationResult.Success));
					HistoryChanged?.Invoke(this, EventArgs.Empty);
					break;
			}
		}

		bool OnLoadFailed(WebKit.LoadEvent loadEvent, string? failingUri)
		{
			if (_cancelled)
			{
				// The app cancelled this navigation in Navigating; WebKit reports it as a failure.
				_cancelled = false;
				_failed = true;
				return false;
			}

			_failed = true;
			Navigated?.Invoke(this, new GtkWebNavigatedEventArgs(failingUri, WebNavigationResult.Failure));
			return false;
		}

		bool OnDecidePolicy(IntPtr decisionHandle, WebKit.PolicyDecisionType decisionType)
		{
			if (decisionHandle == IntPtr.Zero)
				return false;

			if (decisionType != WebKit.PolicyDecisionType.NavigationAction && decisionType != WebKit.PolicyDecisionType.NewWindowAction)
				return false;

			if (GLib.Object.GetObject(decisionHandle) is not WebKit.NavigationPolicyDecision decision)
				return false;

			var action = decision.NavigationAction;
			var uri = action?.Request?.Uri;
			if (action is null || string.IsNullOrEmpty(uri))
				return false;

			_cancelled = false;
			var args = new GtkWebNavigatingEventArgs(uri, GtkWebViewScripts.ResolveNavigationEvent(action.NavigationType, WebNavigationEvent.NewPage));
			Navigating?.Invoke(this, args);

			if (args.Cancel)
			{
				_cancelled = true;
				decision.Ignore();
				return true;
			}

			if (decisionType == WebKit.PolicyDecisionType.NewWindowAction)
			{
				// target=_blank and window.open: there is no second web view, so the link loads here.
				decision.Ignore();
				_view.LoadUri(uri);
				return true;
			}

			return false;
		}

		void OnWebProcessTerminated(object? sender, WebKit.WebProcessTerminatedArgs args) =>
			ProcessTerminated?.Invoke(this, EventArgs.Empty);
	}
}
