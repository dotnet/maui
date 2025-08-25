using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
		{
			activityIndicatorView.UpdateActivityIndicatorVisibility(activityIndicator);

			if (activityIndicator.IsRunning && !activityIndicatorView.Hidden)
				activityIndicatorView.StartAnimating();
			else if(activityIndicatorView.IsAnimating)
				activityIndicatorView.StopAnimating();
		}

		internal static void UpdateActivityIndicatorVisibility(this UIActivityIndicatorView platformView,IActivityIndicator activityIndicator)
		{		
			switch(activityIndicator.Visibility)
			{
				case Visibility.Visible:
					platformView.Inflate();
					platformView.Hidden = !activityIndicator.IsRunning;
					break;
				case Visibility.Hidden:
					platformView.Inflate();
					platformView.Hidden = true;
					break;
				case Visibility.Collapsed:
					platformView.Hidden = true;
					platformView.Collapse();
					break;
			}
		}

		public static void UpdateColor(this UIActivityIndicatorView activityIndicatorView, IActivityIndicator activityIndicator)
			=> activityIndicatorView.Color = activityIndicator.Color?.ToPlatform();
	}
}