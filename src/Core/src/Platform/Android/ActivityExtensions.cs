using Android.App;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	internal static partial class ActivityExtensions
	{
		internal static Rect GetWindowFrame(this Activity activity)
		{
			var bounds = PlatformInterop.GetCurrentWindowMetrics(activity);
			return activity.FromPixels(bounds);
		}

		internal static void UpdateX(this Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateY(this Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateWidth(this Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateHeight(this Activity activity, IWindow window) =>
			activity.UpdateUnsupportedCoordinate(window);

		internal static void UpdateUnsupportedCoordinate(this Activity activity, IWindow window) =>
			window.FrameChanged(activity.GetWindowFrame());
	}
}
