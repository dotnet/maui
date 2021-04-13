using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView2>
	{
		protected override WebView2 CreateNativeView() => new WebView2();

		[MissingMapper]
		public static void MapSource(WebViewHandler handler, IWebView view) { }
	}
}