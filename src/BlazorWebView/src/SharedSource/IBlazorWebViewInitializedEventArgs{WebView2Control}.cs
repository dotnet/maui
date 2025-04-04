#if WEBVIEW2_WINFORMS
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
#elif WEBVIEW2_WPF
using WebView2Control = Microsoft.Web.WebView2.Wpf.IWebView2;
#elif WINDOWS && WEBVIEW2_MAUI
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
#elif ANDROID
using AWebView = Android.Webkit.WebView;
#elif IOS || MACCATALYST
using WebKit;
#elif TIZEN
using TWebView = Tizen.NUI.BaseComponents.WebView;
#endif

namespace Microsoft.AspNetCore.Components.WebView
{
	public interface IBlazorWebViewInitializedEventArgs<out WebView2Control>
	{
#nullable disable
#if WINDOWS
		public WebView2Control WebView { get; }
#elif ANDROID
		public AWebView WebView { get; }
#elif MACCATALYST || IOS
		public WKWebView WebView { get; }
#elif TIZEN
		public TWebView WebView { get; }
#endif
	}
}
