#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this MauiActivityIndicator mauiActivityIndicator, IActivityIndicator activityIndicator)
		{
			mauiActivityIndicator.ElementOpacity = activityIndicator.IsRunning ? activityIndicator.Opacity : 0;
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
				mauiActivityIndicator.Foreground = color.ToPlatform();
			}
		}
	}
}