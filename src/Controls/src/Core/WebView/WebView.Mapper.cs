#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		[Obsolete("Use WebViewHandler.Mapper instead.")]
		public static IPropertyMapper<IWebView, WebViewHandler> ControlsWebViewMapper = new PropertyMapper<WebView, WebViewHandler>(WebViewHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.WebView legacy behaviors
#if ANDROID
			WebViewHandler.Mapper.ReplaceMapping<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.DisplayZoomControlsProperty.PropertyName, MapDisplayZoomControls);
			WebViewHandler.Mapper.ReplaceMapping<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.EnableZoomControlsProperty.PropertyName, MapEnableZoomControls);
			WebViewHandler.Mapper.ReplaceMapping<WebView, IWebViewHandler>(PlatformConfiguration.AndroidSpecific.WebView.MixedContentModeProperty.PropertyName, MapMixedContentMode);
#endif
		}
	}
}
