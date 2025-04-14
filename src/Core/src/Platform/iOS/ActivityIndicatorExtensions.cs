using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
		{
			activityIndicatorView.Hidden = GetActivityIndicatorVisibility(activityIndicator);

			if (activityIndicator.IsRunning && !activityIndicatorView.Hidden)
				activityIndicatorView.StartAnimating();
			else if(activityIndicatorView.IsAnimating)
				activityIndicatorView.StopAnimating();
		}

		private static bool GetActivityIndicatorVisibility(IActivityIndicator activityIndicator)
		{
			if (activityIndicator.Visibility == Visibility.Visible)
			{
				return !activityIndicator.IsRunning;
			}
			else
			{
				return true;
			}
		}

		public static void UpdateColor(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
			=> activityIndicatorView.Color = activityIndicator.Color?.ToPlatform();
	}
}