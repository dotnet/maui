using Android.App;
using Microsoft.Maui.Graphics;
using AndroidX.Core.View;

namespace Microsoft.Maui
{
	public static class WindowExtensions
	{
		public static void SetStatusBarColor(this Activity activity, Color color) =>
			SetStatusBarColor(activity, color, color?.GetLuminosity() < 0.5);

		public static void SetStatusBarColor(this Activity activity, Color color, bool lightForeground)
		{
			if (!NativeVersion.Supports(NativeApis.StatusBarColor))
				return;

			var window = activity.Window;
			if (window == null)
				return;

			// TODO: handle when the status bar is to be hidden
			if (color == null)
				return;

			// let the system know we are going to draw it
			window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
			window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);

			// set the color
			window.SetStatusBarColor(color.ToNative());

			// set the text color
			using var wic = new WindowInsetsControllerCompat(window, window.DecorView);
			wic.AppearanceLightStatusBars = lightForeground;
		}
	}
}