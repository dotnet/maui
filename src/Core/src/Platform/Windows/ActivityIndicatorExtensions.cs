#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressRing platformActivityIndicator, IActivityIndicator virtualView)
		{
			platformActivityIndicator.IsActive = virtualView.IsRunning;
		}
		public static void UpdateColor(this ProgressRing platformActivityIndicator, IActivityIndicator activityIndicator)
		{
			platformActivityIndicator.UpdateColor(activityIndicator, null);
		}

		public static void UpdateColor(this ProgressRing platformActivityIndicator, IActivityIndicator activityIndicator, object? foregroundDefault)
		{
			Color color = activityIndicator.Color;

			if (color.IsDefault())
			{
				if (foregroundDefault != null)
					platformActivityIndicator.RestoreForegroundCache(foregroundDefault);
			}
			else
			{
				platformActivityIndicator.Foreground = color.ToPlatform();
			}
		}

		public static void UpdateWidth(this ProgressRing platformActivityIndicator, IActivityIndicator activityIndicator)
		{
			if (Dimension.IsExplicitSet(activityIndicator.Width))
			{
				// Only set a value for this if it's been explicitly set in the platform code.
				platformActivityIndicator.Width = activityIndicator.Width;
			}
			// Otherwise, don't set it to anything (even NaN) because it will try to fill all the space you give it
		}

		public static void UpdateHeight(this ProgressRing platformActivityIndicator, IActivityIndicator activityIndicator)
		{
			if (Dimension.IsExplicitSet(activityIndicator.Height))
			{
				// Only set a value for this if it's been explicitly set in the platform code.
				platformActivityIndicator.Height = activityIndicator.Height;
			}
			// Otherwise, don't set it to anything (even NaN) because it will try to fill all the space you give it
		}
	}
}