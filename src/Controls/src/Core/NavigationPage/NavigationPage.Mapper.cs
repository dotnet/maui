#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		[Obsolete("Use NavigationViewHandler.Mapper instead.")]
		public static IPropertyMapper<IStackNavigationView, NavigationViewHandler> ControlsNavigationPageMapper =
			new PropertyMapper<NavigationPage, NavigationViewHandler>(NavigationViewHandler.Mapper);

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
