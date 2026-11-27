#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.WebView legacy behaviors
#if ANDROID
			WebViewHandler.Mapper.ReplaceMappingForControls<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.DisplayZoomControlsProperty.PropertyName, MapDisplayZoomControls);
			WebViewHandler.Mapper.ReplaceMappingForControls<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.EnableZoomControlsProperty.PropertyName, MapEnableZoomControls);
			WebViewHandler.Mapper.ReplaceMappingForControls<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.MixedContentModeProperty.PropertyName, MapMixedContentMode);
			WebViewHandler.Mapper.ReplaceMappingForControls<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.JavaScriptEnabledProperty.PropertyName, MapJavaScriptEnabled);
#endif
		}
	}
}
