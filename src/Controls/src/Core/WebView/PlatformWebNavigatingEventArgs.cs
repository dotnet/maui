namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides platform-specific information about the <see cref="WebNavigatingEventArgs"/> event.
	/// </summary>
	public class PlatformWebNavigatingEventArgs
	{
#if WINDOWS

		internal PlatformWebNavigatingEventArgs(WebViewNavigatingEventArgs args)
		{
			NavigationArgs = args.NavigationArgs;
			NewWindowArgs = args.NewWindowArgs;
		}

		/// <summary>
		/// Gets the native navigation starting event args. Non-null for MainFrame and Frame navigations.
		/// </summary>
		/// <remarks>
		/// This is only available on Windows.
		/// </remarks>
		public global::Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs? NavigationArgs { get; }

		/// <summary>
		/// Gets the native new window requested event args. Non-null for NewWindow navigations.
		/// </summary>
		/// <remarks>
		/// This is only available on Windows.
		/// </remarks>
		public global::Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs? NewWindowArgs { get; }

#elif IOS || MACCATALYST

		internal PlatformWebNavigatingEventArgs(WebViewNavigatingEventArgs args)
		{
			NavigationAction = args.NavigationAction;
		}

		/// <summary>
		/// Gets the native navigation action that triggered the event. Contains frame and request info.
		/// </summary>
		/// <remarks>
		/// This is only available on iOS and Mac Catalyst.
		/// </remarks>
		public global::WebKit.WKNavigationAction NavigationAction { get; }

#elif ANDROID

		internal PlatformWebNavigatingEventArgs(WebViewNavigatingEventArgs args)
		{
			Request = args.Request;
		}

		/// <summary>
		/// Gets the native web resource request. Contains URL, headers, and IsForMainFrame info.
		/// </summary>
		/// <remarks>
		/// This is only available on Android.
		/// </remarks>
		public global::Android.Webkit.IWebResourceRequest? Request { get; }

#else

		internal PlatformWebNavigatingEventArgs(WebViewNavigatingEventArgs args)
		{
		}

#endif
	}
}
