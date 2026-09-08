#nullable disable
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		public static void MapPrefersLargeTitles(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapPrefersLargeTitles((INavigationViewHandler)handler, navigationPage);

		public static void MapPrefersLargeTitles(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is UINavigationController navigationController)
				Platform.NavigationPageExtensions.UpdatePrefersLargeTitles(navigationController, navigationPage);
		}
	}
}