#nullable disable
using System;
using System.Threading;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		static int s_remappedForControls;

		internal new static void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			VisualElement.RemapForControls();

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
