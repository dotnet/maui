using Foundation;
using UIKit;
using Xamarin.Forms;

namespace Xamarin.Platform
{
	public static class ViewExtensions
	{

		public static void SetBackgroundColor(this UIView view, UIColor color)
			=> view.BackgroundColor = color;
		public static UIColor GetBackgroundColor(this UIView view) =>
			view.BackgroundColor;


		public static void UpdateIsEnabled(this UIView nativeView, IView view)
		{
			if (!(nativeView is UIControl uiControl))
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void UpdateBackgroundColor(this UIView nativeView, IView view)
		{
			var color = view.BackgroundColor;

			if (color != null && !color.IsDefault)
				nativeView.BackgroundColor = color.ToNative();
		}
	}
}
