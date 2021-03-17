namespace Microsoft.Maui
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this NativeActivityIndicator activityIndicatorView, IActivityIndicator activityIndicator)
		{
			if (activityIndicator.IsRunning)
				activityIndicatorView.StartAnimating();
			else
				activityIndicatorView.StopAnimating();
		}

		public static void UpdateColor(this NativeActivityIndicator activityIndicatorView, IActivityIndicator activityIndicator)
		{
			activityIndicatorView.Color = activityIndicator.Color == Color.Default ? null : activityIndicator.Color.ToNative();
		}
	}
}