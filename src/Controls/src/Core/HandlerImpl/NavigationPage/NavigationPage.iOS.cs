namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		public static void MapPrefersLargeTitles(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler.ViewController is ControlsNavigationController navigationController)
				Platform.NavigationPageExtensions.UpdatePrefersLargeTitles(navigationController, navigationPage);
		}

		public static void MapIsNavigationBarTranslucent(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler.ViewController is ControlsNavigationController navigationController)
				Platform.NavigationPageExtensions.UpdateIsNavigationBarTranslucent(navigationController, navigationPage);
		}
	}
}