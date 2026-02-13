using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
		{

				// Only show and animate if both IsRunning AND Visibility == Visible
			if (activityIndicator.IsRunning && activityIndicator.Visibility == Visibility.Visible)
			{
				activityIndicatorView.Hidden = false;
 				activityIndicatorView.StartAnimating();
			}
			else
			{
				if (activityIndicatorView.IsAnimating)
					activityIndicatorView.StopAnimating();
				activityIndicatorView.Hidden = true;
			}
		}

		public static void UpdateColor(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
			=> activityIndicatorView.Color = activityIndicator.Color?.ToPlatform();
	}
}