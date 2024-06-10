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
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.IsNavigationBarTranslucentProperty.PropertyName, MapIsNavigationBarTranslucent);
#endif
#if IOS || MACCATALYST
			// Only map the Toolbar when it is on the NavigationPage, for now
			WindowHandler.Mapper.ReplaceMapping<Window, IWindowHandler>(nameof(IToolbarElement.Toolbar), MapToolbar);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparatorProperty.PropertyName, MapHideNavigationBarSeparator);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty.PropertyName, MapPreferredStatusBarUpdateAnimation);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName, MapPrefersHomeIndicatorAutoHidden);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.StatusBarTextColorModeProperty.PropertyName, MapStatusBarTextColorMode);
			NavigationViewHandler.Mapper.AppendToMapping<NavigationPage, NavigationViewHandler>(NavigationPage.CurrentPageProperty.PropertyName, MapCurrentPage);
#endif
		}
	}
}
