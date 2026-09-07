using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			// Guard: check IsDisposed() before touching any view properties to avoid an
			// ObjectDisposedException on a ProgressBar whose native handle was already released.
			if (progressBar.IsDisposed())
			{
				return;
			}

			// Defer via Post() only when a layout traversal is in progress OR the view is not yet
			// attached (RecyclerView header collapse scenario). On the common path (attached, idle),
			// apply synchronously so existing tests and callers that read state immediately after the
			// update continue to work correctly.
			if (progressBar.IsInLayout || !progressBar.IsAttachedToWindow)
			{
				progressBar.Post(() =>
				{
					// Guard: skip if the view was disposed before the runnable fired.
					if (!progressBar.IsDisposed())
					{
						// Re-read the LIVE visibility here instead of capturing a snapshot before the
						// Post(). Capturing a stale "show" value caused this deferred runnable to
						// resurrect an indicator that was hidden after it was queued.
						progressBar.Visibility = GetActivityIndicatorVisibility(activityIndicator);
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