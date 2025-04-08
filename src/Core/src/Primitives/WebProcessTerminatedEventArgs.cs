#if ANDROID
using Android.Views;
using Android.Webkit;
#endif

namespace Microsoft.Maui
{
	public class WebProcessTerminatedEventArgs
	{
#if ANDROID
		internal WebProcessTerminatedEventArgs(View? sender, RenderProcessGoneDetail? renderProcessGoneDetail)
		{
			Sender = sender;
			RenderProcessGoneDetail = renderProcessGoneDetail;
		}

		public View? Sender { get; }
		public RenderProcessGoneDetail? RenderProcessGoneDetail { get; }
#elif IOS || MACCATALYST
		internal WebProcessTerminatedEventArgs(WebKit.WKWebView sender)
		{
			Sender = sender;
		}

		public WebKit.WKWebView Sender { get; }
#elif WINDOWS
		internal WebProcessTerminatedEventArgs(Web.WebView2.Core.CoreWebView2 sender, Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs coreWebView2ProcessFailedEventArgs)
		{
			Sender = sender;
			CoreWebView2ProcessFailedEventArgs = coreWebView2ProcessFailedEventArgs;
		}

		public Web.WebView2.Core.CoreWebView2 Sender { get; }
		public Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs CoreWebView2ProcessFailedEventArgs { get; }
#endif
	}
}
