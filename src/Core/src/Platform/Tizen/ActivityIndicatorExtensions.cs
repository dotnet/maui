using ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressBar activityIndicatorView, IActivityIndicator activityIndicator)
		{
			if (activityIndicator.IsRunning)
				activityIndicatorView.PlayPulse();
			else
				activityIndicatorView.StopPulse();
		}

		public static void UpdateColor(this ProgressBar activityIndicatorView, IActivityIndicator activityIndicator)
			=> activityIndicatorView.Color = activityIndicator.Color.ToPlatformEFL();
	}
}