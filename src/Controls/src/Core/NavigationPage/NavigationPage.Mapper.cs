#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using System.Diagnostics.CodeAnalysis;

[assembly:Experimental ("XCODE_26_0_PREVIEW")]
namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.NavigationPage legacy behaviors
#if IOS
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName, MapPrefersLargeTitles);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.IsNavigationBarTranslucentProperty.PropertyName, MapIsNavigationBarTranslucent);
#endif
		}
	}
}
