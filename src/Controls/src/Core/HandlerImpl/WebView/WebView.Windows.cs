using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{		
		public static void MapIsJavaScriptAlertEnabled(WebViewHandler handler, WebView webView)
		{
			handler.IsJavaScriptAlertEnabled = webView.OnThisPlatform().IsJavaScriptAlertEnabled();
		}
	}
}