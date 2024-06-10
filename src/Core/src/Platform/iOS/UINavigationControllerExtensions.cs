using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class UINavigationControllerExtensions
	{
		public static void UpdateNavigationBarVisibility(this UINavigationController navigationController, bool isNavigationbarVisible, bool animated)
		{
			if (navigationController.NavigationBarHidden == isNavigationbarVisible)
			{
				// TODO: when the toolbar is hidden and another page is pushed, the toolbar for the page that's about to hide
				// gets its toolbar back right before the animation of the push of the new page
				navigationController.SetNavigationBarHidden(!isNavigationbarVisible, animated);
			}
		}

		public static void UpdateBackButtonVisibility(this UINavigationController navigationController, bool backButtonVisible)
		{
			var navigationItem = navigationController.TopViewController?.NavigationItem;

			if (navigationItem == null)
			{
				return;
			}

			if (navigationItem.HidesBackButton == !backButtonVisible)
			{
				return;
			}

			navigationItem.HidesBackButton = !backButtonVisible;
		}
	}
}
