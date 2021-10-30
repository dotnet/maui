#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this MauiActivityIndicator mauiActivityIndicator, IActivityIndicator activityIndicator)
		{
			// TODO: Use IView Opacity if the ActivityIndicator is running.
			mauiActivityIndicator.ElementOpacity = activityIndicator.IsRunning ? 1 : 0;
		}
		public static void UpdateColor(this MauiActivityIndicator mauiActivityIndicator, IActivityIndicator activityIndicator)
		{
			mauiActivityIndicator.UpdateColor(activityIndicator, null);
		}

		public static void UpdateColor(this MauiActivityIndicator mauiActivityIndicator, IActivityIndicator activityIndicator, object? foregroundDefault)
		{
			Color color = activityIndicator.Color;

			if (color.IsDefault())
			{
				if (foregroundDefault != null)
					mauiActivityIndicator.RestoreForegroundCache(foregroundDefault);
			}
			else
			{
				mauiActivityIndicator.Foreground = color.ToNative();
			}
		}
	}
}