using System;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// The browser engine behind a GTK web handler: WebKitGTK 6 on Linux (<see cref="GtkWebKitEngine"/>) or WebView2 on Windows
	/// (<c>MauiGtkWebView2Engine</c>). The handler keeps the MAUI semantics (pending navigation event, cancel, <c>Navigated</c>, property
	/// mappers) and talks to the engine only through this interface.
	/// </summary>
	/// <remarks>
	/// <para>Seam for the HybridWebView and BlazorWebView handlers: <see cref="GtkWebBackend.CreateEngine"/> returns the engine of a
	/// platform view created by <see cref="GtkWebBackend.CreatePlatformView"/>. Engine-specific features (custom schemes or resource
	/// interception, script messages, document-created scripts) are reached through the engine's native object instead of this
	/// interface: the WebKit view for <see cref="GtkWebKitEngine"/>, <c>MauiGtkWebView2Host.CoreWebView2</c> for WebView2.</para>
	/// <para>Everything is called on the GTK thread; events are raised on it.</para>
	/// </remarks>
	internal interface IGtkWebEngine : IDisposable
	{
		/// <summary>The widget to put into the GTK tree.</summary>
		global::Gtk.Widget Widget { get; }

		/// <summary>The current URI, or <see langword="null"/> before the first load.</summary>
		string? Uri { get; }

		/// <summary>The title of the current document, or <see langword="null"/>.</summary>
		string? Title { get; }

		/// <summary>Whether the history has an entry before the current one.</summary>
		bool CanGoBack { get; }

		/// <summary>Whether the history has an entry after the current one.</summary>
		bool CanGoForward { get; }

		/// <summary>
		/// The user agent. Setting it applies it to following loads; reading it returns the effective one, or <see langword="null"/> while the
		/// engine is not ready (see <see cref="Ready"/>).
		/// </summary>
		string? UserAgent { get; set; }

		/// <summary>The cookie store of the engine, or <see langword="null"/> when the engine cannot synchronise cookies.</summary>
		IGtkWebCookies? Cookies { get; }

		/// <summary>
		/// Raised when a navigation starts that the app did not start itself (a link, a form, a script, a redirect, back/forward) - the app's own
		/// <see cref="LoadUri"/>/<see cref="LoadHtml"/>/<see cref="Reload"/> calls do not raise it, the handler raised <c>Navigating</c> before them.
		/// Setting <see cref="GtkWebNavigatingEventArgs.Cancel"/> cancels the navigation.
		/// </summary>
		event EventHandler<GtkWebNavigatingEventArgs>? Navigating;

		/// <summary>Raised when a load finished or failed. A load the app cancelled in <see cref="Navigating"/> raises nothing.</summary>
		event EventHandler<GtkWebNavigatedEventArgs>? Navigated;

		/// <summary>Raised when the history changed (<see cref="CanGoBack"/>/<see cref="CanGoForward"/>).</summary>
		event EventHandler? HistoryChanged;

		/// <summary>Raised when the engine's content process died.</summary>
		event EventHandler? ProcessTerminated;

		/// <summary>
		/// Raised when the engine becomes usable. Never raised for WebKit (usable at once); WebView2 creates its controller asynchronously once the
		/// host widget is realized (and again after a re-realization), so <see cref="UserAgent"/> reads <see langword="null"/> before.
		/// </summary>
		event EventHandler? Ready;

		/// <summary>Loads a URI. Operations issued before the engine is ready are kept and run, the last navigation only, when it is.</summary>
		void LoadUri(string uri);

		/// <summary>Loads HTML text; <paramref name="baseUrl"/> is ignored by engines that cannot set one (WebView2).</summary>
		void LoadHtml(string html, string? baseUrl);

		/// <summary>Navigates back.</summary>
		void GoBack();

		/// <summary>Navigates forward.</summary>
		void GoForward();

		/// <summary>Reloads the current page.</summary>
		void Reload();

		/// <summary>Stops the load in progress.</summary>
		void Stop();

		/// <summary>Runs a script and ignores its result.</summary>
		void RunScript(string script);

		/// <summary>Runs a script and completes <paramref name="request"/> with its result (a string, or JSON for other values; <see langword="null"/> for undefined/null/failed scripts).</summary>
		void EvaluateScript(string script, EvaluateJavaScriptAsyncRequest request);
	}

	/// <summary>The cookie store of a web engine.</summary>
	internal interface IGtkWebCookies
	{
		/// <summary>Adds or replaces <paramref name="cookies"/>.</summary>
		void Set(CookieCollection cookies);

		/// <summary>The cookies the engine would send to <paramref name="uri"/>.</summary>
		Task<CookieCollection> GetAsync(Uri uri);
	}

	/// <summary>A navigation the page started.</summary>
	internal sealed class GtkWebNavigatingEventArgs : EventArgs
	{
		/// <summary>Creates the arguments.</summary>
		public GtkWebNavigatingEventArgs(string uri, WebNavigationEvent navigationEvent)
		{
			Uri = uri;
			NavigationEvent = navigationEvent;
		}

		/// <summary>The target URI.</summary>
		public string Uri { get; }

		/// <summary>What the engine knows about the navigation (<c>NewPage</c>, <c>Back</c>/<c>Forward</c>, <c>Refresh</c>); the handler's pending event wins.</summary>
		public WebNavigationEvent NavigationEvent { get; }

		/// <summary>Whether the app cancelled the navigation.</summary>
		public bool Cancel { get; set; }
	}

	/// <summary>A load that finished or failed.</summary>
	internal sealed class GtkWebNavigatedEventArgs : EventArgs
	{
		/// <summary>Creates the arguments.</summary>
		public GtkWebNavigatedEventArgs(string? uri, WebNavigationResult result)
		{
			Uri = uri;
			Result = result;
		}

		/// <summary>The URI that was loaded (the failing one on a failure).</summary>
		public string? Uri { get; }

		/// <summary>The outcome.</summary>
		public WebNavigationResult Result { get; }
	}
}
