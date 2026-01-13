using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			progressBar.Visibility = GetActivityIndicatorVisibility(activityIndicator);
		}

		internal static ViewStates GetActivityIndicatorVisibility(this IActivityIndicator activityIndicator)
		{
			if (activityIndicator.Visibility == Visibility.Visible)
			{
				return activityIndicator.IsRunning ? ViewStates.Visible : ViewStates.Invisible;
			}
			else
			{
				return activityIndicator.Visibility.ToPlatformVisibility();
			}
		}

		public static void UpdateColor(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			var color = activityIndicator.Color;

			if (color != null)
				progressBar.IndeterminateDrawable = progressBar.IndeterminateDrawable.SafeSetColorFilter(color.ToPlatform(), FilterMode.SrcIn);
			else
				progressBar.IndeterminateDrawable = progressBar.IndeterminateDrawable.SafeClearColorFilter();
		}
	}
}