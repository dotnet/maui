#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		public static IPropertyMapper<IWebView, WebViewHandler> ControlsWebViewMapper = new PropertyMapper<WebView, WebViewHandler>(WebViewHandler.Mapper)
		{
#if ANDROID
			[PlatformConfiguration.AndroidSpecific.WebView.DisplayZoomControlsProperty.PropertyName] = MapDisplayZoomControls,
			[PlatformConfiguration.AndroidSpecific.WebView.EnableZoomControlsProperty.PropertyName] = MapEnableZoomControls,
			[PlatformConfiguration.AndroidSpecific.WebView.MixedContentModeProperty.PropertyName] = MapMixedContentMode,
#endif
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.WebView legacy behaviors
			WebViewHandler.Mapper = ControlsWebViewMapper;
		}
	}
}
