using UIKit;

namespace Microsoft.Maui.Platform
{
    public static class UIViewControllerExtensions
    {
        public static void UpdateNavigationBarTitle(this UIViewController viewController, string title)
		{
            if (!string.IsNullOrWhiteSpace(title))
			{
                viewController.NavigationItem.Title = title;
            }
		}

		public static void UpdateBackButtonTitle(this UIViewController viewController, string backButtonTitle)
		{
            if (backButtonTitle != null)
			{
				viewController.NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = backButtonTitle, Style = UIBarButtonItemStyle.Plain };
			}
		}

		public static void InvalidateMeasure(this UIViewController viewController)
		{
			if (viewController != null)
			{
				viewController.View?.SetNeedsLayout();
				viewController.View?.Superview?.SetNeedsLayout();
			}
		}
	}
}