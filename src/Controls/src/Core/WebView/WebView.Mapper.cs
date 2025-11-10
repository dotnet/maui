#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.WebView legacy behaviors
#if ANDROID
			WebViewHandler.Mapper.ReplaceMapping<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.DisplayZoomControlsProperty.PropertyName, MapDisplayZoomControls);
			WebViewHandler.Mapper.ReplaceMapping<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.EnableZoomControlsProperty.PropertyName, MapEnableZoomControls);
			WebViewHandler.Mapper.ReplaceMapping<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.MixedContentModeProperty.PropertyName, MapMixedContentMode);
			WebViewHandler.Mapper.ReplaceMapping<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.JavaScriptEnabledProperty.PropertyName, MapJavaScriptEnabled);
#endif
		}
	}
}
