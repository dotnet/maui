using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class UINavigationControllerExtensions
	{
		public static void UpdateNavigationBarVisibility(this UINavigationController navigationController, bool isNavigationbarVisible, bool animated)
		{
			if (navigationController.NavigationBarHidden == isNavigationbarVisible)
			{
				// TODO: the view underneath this jumps up when the toolbar is hidden and down when it's shown
				// rather than smoothly animating the transition. It's not clear how to fix this
				// Also, when the toolbar is hidden and another page is pushed, the toolbar for the page that's about to hide
				// gets its toolbar back right before the animation of the push of the new page
				navigationController.SetNavigationBarHidden(!isNavigationbarVisible, animated);
			}
		}
	}
}
