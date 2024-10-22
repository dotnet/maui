namespace Microsoft.Maui.Controls
{
	public class PlatformWebViewProcessTerminatedEventArgs
	{
#if ANDROID
		internal PlatformWebViewProcessTerminatedEventArgs(Android.Views.View? sender, Android.Webkit.RenderProcessGoneDetail? renderProcessGoneDetail)
		{
			Sender = sender;
			RenderProcessGoneDetail = renderProcessGoneDetail;
		}

		/// <summary>
		/// Gets the native view attached to the event.
		/// </summary>
		public Android.Views.View? Sender { get; }

		/// <summary>
		/// Gets the native event or handler attached to the view.
		/// </summary>
		public Android.Webkit.RenderProcessGoneDetail? RenderProcessGoneDetail { get; }
#elif IOS || MACCATALYST
		internal PlatformWebViewProcessTerminatedEventArgs(WebKit.WKWebView sender)
		{
			Sender = sender;
		}
				
		/// <summary>
		/// Gets the native view attached to the event.
		/// </summary>
		public WebKit.WKWebView Sender { get; }
#elif WINDOWS

		internal PlatformWebViewProcessTerminatedEventArgs(Web.WebView2.Core.CoreWebView2 sender, Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs coreWebView2ProcessFailedEventArgs)
		{
			Sender = sender;
			CoreWebView2ProcessFailedEventArgs = coreWebView2ProcessFailedEventArgs;
		}

		/// <summary>
		/// Gets the native view attached to the event.
		/// </summary>
		public Web.WebView2.Core.CoreWebView2 Sender { get; }
		
		/// <summary>
		/// Gets the native event or handler attached to the view.
		/// </summary>
		public Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs CoreWebView2ProcessFailedEventArgs { get; }
#endif
	}
}