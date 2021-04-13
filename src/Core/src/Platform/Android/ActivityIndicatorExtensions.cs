using Android.Views;
using Android.Widget;

namespace Microsoft.Maui
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressBar progressBar, IActivityIndicator activityIndicator) =>
			progressBar.Visibility = activityIndicator.IsRunning ? ViewStates.Visible : ViewStates.Invisible;

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