using Gtk;

namespace Microsoft.Maui
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this Spinner nativeActivityIndicator, IActivityIndicator activityIndicator)
		{
			if (activityIndicator.IsRunning)
				nativeActivityIndicator.Start();
			else
				nativeActivityIndicator.Stop();
		}
	}
}