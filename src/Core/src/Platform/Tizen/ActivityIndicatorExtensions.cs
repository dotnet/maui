using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ActivityIndicator nativeView, IActivityIndicator activityIndicator) => nativeView.IsRunning = activityIndicator.IsRunning;

		public static void UpdateColor(this ActivityIndicator nativeView, IActivityIndicator activityIndicator)
			=> nativeView.Color = activityIndicator.Color.ToNative();
	}
}