#if __IOS__ || MACCATALYST
using NativeView = WebKit.WKWebView;
#elif __ANDROID__
using NativeView = Android.Webkit.WebView;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Controls.WebView2;
#elif NETSTANDARD
using NativeView = System.Object;
# endif

using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/WebView.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebView']/Docs" />
	public partial class WebView : IWebView
	{
		bool _canGoBack;
		bool _canGoForward;

		IWebViewSource IWebView.Source => Source;

		bool IWebView.CanGoBack
		{
			get => _canGoBack;
			set
			{
				_canGoBack = value;
				((IWebViewController)this).CanGoBack = _canGoBack;
				Handler?.UpdateValue(nameof(IWebView.CanGoBack));
			}
		}

		bool IWebView.CanGoForward
		{
			get => _canGoForward;
			set
			{
				_canGoForward = value;
				((IWebViewController)this).CanGoForward = _canGoForward;
				Handler?.UpdateValue(nameof(IWebView.CanGoForward));
			}
		}

		Task<string> IWebView.EvaluateJavaScriptAsync(string script)
		{
			var virtualView = (IWebView)Handler.VirtualView;
			var nativeView = (NativeView)Handler.NativeView;

			if (virtualView == null || nativeView == null)
				return Task.FromResult(string.Empty);

			return nativeView.EvaluateJavaScriptAsync(virtualView, script);
		}
	}
}