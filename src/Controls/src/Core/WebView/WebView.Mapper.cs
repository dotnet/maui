#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		static WebView()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
			RemappingHelper.EnsureBaseTypeRemapped(typeof(WebView), typeof(VisualElement));

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
