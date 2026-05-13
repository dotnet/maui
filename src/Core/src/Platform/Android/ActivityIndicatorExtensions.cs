using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			// Guard: check IsDisposed() before accessing any view properties to avoid ObjectDisposedException
			// on a ProgressBar whose native handle has already been released (see ViewExtensions pattern).
			if (progressBar.IsDisposed())
			{
				return;
			}

			// Pre-compute the desired visibility from the current activityIndicator state before deferring,
			// so the lambda performs a simple write with no risk of reading stale handler state.
			// Defer via Post() only when a layout traversal is in progress OR the view is not yet attached
			// (RecyclerView header collapse scenario, see https://github.com/dotnet/maui/issues/33780).
			// On the common path (attached, idle), apply synchronously so that existing tests and callers
			// that read state immediately after update continue to work correctly.
			if (progressBar.IsInLayout || !progressBar.IsAttachedToWindow)
			{
				var targetVisibility = GetActivityIndicatorVisibility(activityIndicator);
				progressBar.Post(() =>
				{
					// Guard: skip if the view was recycled/disposed or detached before the runnable fired.
					if (!progressBar.IsDisposed())
					{
						progressBar.Visibility = targetVisibility;
					}
				});
			}
			else
			{
				progressBar.Visibility = GetActivityIndicatorVisibility(activityIndicator);
			}
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
				progressBar.IndeterminateDrawable?.SetColorFilter(color.ToPlatform(), FilterMode.SrcIn);
			else
				progressBar.IndeterminateDrawable?.ClearColorFilter();
		}
	}
}