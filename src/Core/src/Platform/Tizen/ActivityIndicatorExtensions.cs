using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ActivityIndicator platformView, IActivityIndicator activityIndicator) => platformView.IsRunning = activityIndicator.IsRunning;

		public static void UpdateColor(this ActivityIndicator platformView, IActivityIndicator activityIndicator)
			=> platformView.Color = activityIndicator.Color.ToPlatform();
	}
}