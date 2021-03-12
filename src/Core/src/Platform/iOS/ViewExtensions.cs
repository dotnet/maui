using UIKit;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static UIColor? GetBackgroundColor(this UIView view)
			=> view?.BackgroundColor;

		public static void UpdateIsEnabled(this UIView nativeView, IView view)
		{
			if (!(nativeView is UIControl uiControl))
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void UpdateBackgroundColor(this UIView nativeView, IView view)
		{
			if (nativeView == null)
				return;

			var color = view.BackgroundColor;

			if (!color.IsDefault)
				nativeView.BackgroundColor = color.ToNative();
		}

		public static void UpdateAutomationId(this UIView nativeView, IView view) =>
			nativeView.AccessibilityIdentifier = view.AutomationId;
	}
}
