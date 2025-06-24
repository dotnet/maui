#nullable disable

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebView
	{
		internal static void RemapForControls()
		{
			// Adjust the mappings to add platform-specific behavior for BlazorWebView
#if IOS
			BlazorWebViewHandler.BlazorWebViewMapper.ReplaceMapping<BlazorWebView, BlazorWebViewHandler>(PlatformConfiguration.iOSSpecific.BlazorWebView.IsScrollBounceEnabledProperty.PropertyName, MapIsScrollBounceEnabled);
#endif
		}
	}
}