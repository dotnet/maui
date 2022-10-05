namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		public static void MapPrefersLargeTitles(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapPrefersLargeTitles((INavigationViewHandler)handler, navigationPage);

		public static void MapIsNavigationBarTranslucent(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapPrefersLargeTitles((INavigationViewHandler)handler, navigationPage);

		public static void MapPrefersLargeTitles(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is NavigationViewHandler nvh && nvh.ViewController is ControlsNavigationController navigationController)
				Platform.NavigationPageExtensions.UpdatePrefersLargeTitles(navigationController, navigationPage);
		}

		public static void MapIsNavigationBarTranslucent(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is NavigationViewHandler nvh && nvh.ViewController is ControlsNavigationController navigationController)
				Platform.NavigationPageExtensions.UpdateIsNavigationBarTranslucent(navigationController, navigationPage);
		}
	}
}