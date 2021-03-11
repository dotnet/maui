using Android.Graphics.Drawables;
using Microsoft.Maui;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static void UpdateIsEnabled(this AView nativeView, IView view)
		{
			if (nativeView != null)
				nativeView.Enabled = view.IsEnabled;
		}

		public static void UpdateBackground(this AView nativeView, IView view)
		{
			var background = view.Background;
			nativeView.UpdateBackground(background);
		}

		public static void SetBackground(this AView view, Drawable? drawable)
		{
			view.Background = drawable;
		}
	}
}