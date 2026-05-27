#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.NavigationPage legacy behaviors
#if IOS
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName, MapPrefersLargeTitles);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(NavigationPage.BarBackgroundColorProperty.PropertyName, MapBarBackground);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(NavigationPage.BarBackgroundProperty.PropertyName, MapBarBackground);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(NavigationPage.BarTextColorProperty.PropertyName, MapBarTextColor);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparatorProperty.PropertyName, MapHideNavigationBarSeparator);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.StatusBarTextColorModeProperty.PropertyName, MapStatusBarTextColorMode);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName, MapPrefersHomeIndicatorAutoHidden);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName, MapPrefersStatusBarHidden);
#endif
		}
	}
}
