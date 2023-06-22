#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		public static IPropertyMapper<IStackNavigationView, NavigationViewHandler> ControlsNavigationPageMapper =
			new PropertyMapper<NavigationPage, NavigationViewHandler>(NavigationViewHandler.Mapper)
			{
#if IOS
				[PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName] = MapPrefersLargeTitles,
				[PlatformConfiguration.iOSSpecific.NavigationPage.IsNavigationBarTranslucentProperty.PropertyName] = MapIsNavigationBarTranslucent,
#endif
			};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.NavigationPage legacy behaviors
			NavigationViewHandler.Mapper = ControlsNavigationPageMapper;
		}
	}
}
