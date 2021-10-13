using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, NView>
	{
		protected override NView CreatePlatformView() => new();

		public static void MapSource(IWebViewHandler handler, IWebView webView)
		{
		}
	}
}
