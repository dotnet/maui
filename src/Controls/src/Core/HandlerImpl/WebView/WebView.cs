namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		public static IPropertyMapper<IWebView, WebViewHandler> ControlsWebViewMapper = new PropertyMapper<WebView, WebViewHandler>(WebViewHandler.Mapper)
		{
#if ANDROID
			[nameof(PlatformConfiguration.AndroidSpecific.WebView.DisplayZoomControlsProperty.PropertyName)] = MapDisplayZoomControls,
			[nameof(PlatformConfiguration.AndroidSpecific.WebView.EnableZoomControlsProperty.PropertyName)] = MapEnableZoomControls,
			[nameof(PlatformConfiguration.AndroidSpecific.WebView.MixedContentModeProperty.PropertyName)] = MapMixedContentMode,
#endif
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.WebView legacy behaviors
			WebViewHandler.Mapper = ControlsWebViewMapper;
		}
	}
}