namespace Microsoft.Maui
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this MauiActivityIndicator activityIndicatorView, IActivityIndicator activityIndicator)
		{
			if (activityIndicator.IsRunning)
				activityIndicatorView.StartAnimating();
			else
				activityIndicatorView.StopAnimating();
		}

		public static void UpdateColor(this MauiActivityIndicator activityIndicatorView, IActivityIndicator activityIndicator)
		{
			activityIndicatorView.Color = activityIndicator.Color == Color.Default ? null : activityIndicator.Color.ToNative();
		}
	}
}