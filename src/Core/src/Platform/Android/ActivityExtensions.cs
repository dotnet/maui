using AndroidX.Window.Layout;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	internal static partial class ActivityExtensions
	{
		internal static Rect GetWindowFrame(this Android.App.Activity activity)
		{
			var wmc = WindowMetricsCalculator.Companion.OrCreate;
			var wm = wmc.ComputeCurrentWindowMetrics(activity);
			var bounds = wm.Bounds;
			return activity.FromPixels(bounds);
		}

		internal static void UpdateX(this Android.App.Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateY(this Android.App.Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateWidth(this Android.App.Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateHeight(this Android.App.Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateUnsupportedCoordinate(this Android.App.Activity activity, IWindow window) =>
			window.FrameChanged(activity.GetWindowFrame());
	}
}
