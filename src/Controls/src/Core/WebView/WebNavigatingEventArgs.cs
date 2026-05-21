#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="WebView.Navigating"/> event, raised before navigation begins.
	/// </summary>
	public class WebNavigatingEventArgs : WebNavigationEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebNavigatingEventArgs"/> class.
		/// </summary>
		/// <param name="navigationEvent">The type of navigation event.</param>
		/// <param name="source">The source of the web view content.</param>
		/// <param name="url">The URL being navigated to.</param>
		public WebNavigatingEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url) : base(navigationEvent, source, url)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebNavigatingEventArgs"/> class with target and platform args.
		/// </summary>
		/// <param name="navigationEvent">The type of navigation event.</param>
		/// <param name="source">The source of the web view content.</param>
		/// <param name="url">The URL being navigated to.</param>
		/// <param name="target">The navigation target (main frame, iframe, or new window).</param>
		/// <param name="platformArgs">Platform-specific event arguments.</param>
		public WebNavigatingEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url, WebNavigationTarget target, PlatformWebNavigatingEventArgs platformArgs) : base(navigationEvent, source, url)
		{
			Target = target;
			PlatformArgs = platformArgs;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to cancel the navigation.
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Gets the frame target of the navigation (main frame, iframe, or new window).
		/// </summary>
		public WebNavigationTarget Target { get; } = WebNavigationTarget.MainFrame;

		/// <summary>
		/// Gets the platform-specific event arguments for the navigation event.
		/// </summary>
		public PlatformWebNavigatingEventArgs PlatformArgs { get; }
	}
}