using Android.Views;
using Android.Widget;

namespace Microsoft.Maui
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			if (activityIndicator.Visibility == Visibility.Visible)
			{
				progressBar.Visibility = activityIndicator.IsRunning ? ViewStates.Visible : ViewStates.Invisible;
			}
			else
			{
				progressBar.Visibility = activityIndicator.Visibility.ToNativeVisibility();
			}
		}

		public static void UpdateColor(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			var color = activityIndicator.Color;

			if (color != null)
				progressBar.IndeterminateDrawable?.SetColorFilter(color.ToNative(), FilterMode.SrcIn);
			else
				progressBar.IndeterminateDrawable?.ClearColorFilter();
		}
	}
}