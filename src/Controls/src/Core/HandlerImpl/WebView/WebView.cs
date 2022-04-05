namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		public static IPropertyMapper<IWebView, WebViewHandler> ControlsWebViewMapper = new PropertyMapper<WebView, WebViewHandler>(WebViewHandler.Mapper)
			{
#if ANDROID
				["DisplayZoomControls"] = MapDisplayZoomControls,
				["EnableZoomControls"] = MapEnableZoomControls,
				["MixedContentMode"] = MapMixedContentMode,
#endif
			};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.WebView legacy behaviors
			WebViewHandler.Mapper = ControlsWebViewMapper;
		}
	}
}