#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this MauiActivityIndicator mauiActivityIndicator, IActivityIndicator activityIndicator)
		{
			//mauiActivityIndicator.ElementOpacity = activityIndicator.IsRunning ? activityIndicator.Opacity : 0;
			mauiActivityIndicator.IsActive = activityIndicator.IsRunning;
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

		public static void UpdateWidth(this MauiActivityIndicator platformView, IActivityIndicator view)
		{
			if (Dimension.IsExplicitSet(view.Width))
			{
				platformView.Width = view.Width;
			}

			//if (Dimension.IsExplicitSet(view.Width) || !double.IsNaN(platformView.Width))
			//{
			//	// Only set a value for this if it's been explicitly set in the platform code
			//	platformView.Width = view.Width;
			//}

			// Otherwise, don't set it to anything (even NaN) because it will try to fill all the space you give it
		}

		public static void UpdateHeight(this MauiActivityIndicator platformView, IActivityIndicator view)
		{
			if (Dimension.IsExplicitSet(view.Height))
			{
				platformView.Height = view.Height;
			}

			//if (Dimension.IsExplicitSet(view.Height) || !double.IsNaN(platformView.Height))
			//{
			//	// Only set a value for this if it's been explicitly set in the platform code
			//	platformView.Height = view.Height;
			//}

			// Otherwise, don't set it to anything (even NaN) because it will try to fill all the space you give it
		}
	}
}