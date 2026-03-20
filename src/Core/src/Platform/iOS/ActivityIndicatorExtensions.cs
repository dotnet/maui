using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
		{
			// Since Visibility and IsRunning are dependent on each other, we handle Visibility explicitly.
			// Only show and animate if both IsRunning AND Visibility == Visible
			if (activityIndicator.IsRunning && activityIndicator.Visibility == Visibility.Visible)
			{
				activityIndicatorView.Inflate();
				activityIndicatorView.Hidden = false;
				activityIndicatorView.StartAnimating();
			}
			else
			{
				if (activityIndicatorView.IsAnimating)
					activityIndicatorView.StopAnimating();
				
                // Collapsed requires layout constraint (CollapseConstraint) to zero out size;
                // Hidden only sets Hidden = true (preserving layout space).
                if (activityIndicator.Visibility == Visibility.Collapsed)
                {
                    activityIndicatorView.Hidden = true;
                    activityIndicatorView.Collapse();
                }
                else
                {
                    activityIndicatorView.Inflate();
                    activityIndicatorView.Hidden = activityIndicator.Visibility != Visibility.Visible;
                }
			}
		}

		public static void UpdateColor(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
			=> activityIndicatorView.Color = activityIndicator.Color?.ToPlatform();
	}
}